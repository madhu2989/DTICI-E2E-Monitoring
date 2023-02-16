using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.SLA;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using State = Daimler.Providence.Service.Models.StateTransition.State;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for calculating SLA values.
    /// </summary>
    public class SlaCalculationManager : ISlaCalculationManager
    {
        #region Private Members 

        private readonly IStorageAbstraction _storageAbstraction;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public SlaCalculationManager(IStorageAbstraction storageAbstraction)
        {
            _storageAbstraction = storageAbstraction;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<SlaBlobRecord> CalculateSlaAsync(string environmentSubscriptionId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"StartSlaCalculation started. (Environment: '{environmentSubscriptionId}')");
                var dbEnvironments = await _storageAbstraction.GetEnvironments(token).ConfigureAwait(false);

                var warningThreshold = await GetWarningThreshold(environmentSubscriptionId, token).ConfigureAwait(false);
                var errorThreshold = await GetErrorThreshold(environmentSubscriptionId, token).ConfigureAwait(false);
                var includeWarnings = await GetIncludeWarnings(environmentSubscriptionId, token).ConfigureAwait(false);

                // Get all Elements from DB
                var dbEnvironment = dbEnvironments.FirstOrDefault(e => e.SubscriptionId.ToLower() == environmentSubscriptionId.ToLower());
                var elements = await _storageAbstraction.GetAllElementsOfEnvironmentTree(dbEnvironment.Id, token).ConfigureAwait(false);

                // Calculate the SLA    
                var slaDataPerElement = new Dictionary<string, SlaData>();
                var slaDataPerElementPerDay = new Dictionary<string, SlaData[]>();
                foreach (var element in elements)
                {
                    if (element.CreationDate == DateTime.MinValue)
                    {
                        slaDataPerElement[element.ElementId] = null;
                        slaDataPerElementPerDay[element.ElementId] = null;
                    }
                    else
                    {
                        var currentTime = DateTime.UtcNow;
                        currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second, DateTimeKind.Utc);

                        // Clamp StartDate and EndDate between CreationDate of the Element and DateTime.UtcNow 
                        var elementStart = new[] { startDate, element.CreationDate }.Max();
                        var elementEnd = new[] { endDate, currentTime }.Min();

                        // Possible if element.CreationDate > end
                        if (elementEnd < elementStart)
                        {
                            slaDataPerElement[element.ElementId] = null;
                            slaDataPerElementPerDay[element.ElementId] = null;
                        }
                        else
                        {
                            // Calculate SLA for whole time interval
                            slaDataPerElement[element.ElementId] = await CalculateSlaData(dbEnvironment.Id, element, elementStart, elementEnd, includeWarnings, errorThreshold, warningThreshold, token).ConfigureAwait(false);

                            // Clamp StartDate and EndDate between CreationDate of the Element and DateTime.UtcNow 
                            elementStart = startDate;
                            elementEnd = new[] { endDate, currentTime }.Min();

                            // Calculate slices (one slice per day)
                            var sliceCount = GetDaysInTimeInterval(elementStart, elementEnd);
                            var slaHistory = new SlaData[sliceCount];
                            for (var i = 0; i < sliceCount; i++)
                            {
                                // First slice needs special calculation
                                DateTime sliceStart;
                                DateTime sliceEnd;
                                if (i == 0)
                                {
                                    sliceStart = elementStart;
                                    sliceEnd = new DateTime(sliceStart.Year, sliceStart.Month, sliceStart.Day, 23, 59, 59);
                                }
                                else if (i == sliceCount - 1)
                                {
                                    sliceStart = new DateTime(elementEnd.Year, elementEnd.Month, elementEnd.Day, 0, 0, 0);
                                    sliceEnd = elementEnd;
                                }
                                else
                                {
                                    sliceStart = new DateTime(elementStart.Year, elementStart.Month, elementStart.Day, 0, 0, 0).AddDays(i);
                                    sliceEnd = new DateTime(elementStart.Year, elementStart.Month, elementStart.Day, 23, 59, 59).AddDays(i);
                                }
                                slaHistory[i] = await CalculateSlaData(dbEnvironment.Id, element, sliceStart, sliceEnd, includeWarnings, errorThreshold, warningThreshold, token).ConfigureAwait(false);
                            }
                            slaDataPerElementPerDay[element.ElementId] = slaHistory;
                        }
                    }
                }

                // Write SLA to Blob
                var slaBlobRecord = new SlaBlobRecord
                {
                    SlaDataPerElement = slaDataPerElement,
                    SlaDataPerElementPerDay = slaDataPerElementPerDay
                };
                return slaBlobRecord;
            }
        }
            
        /// <inheritdoc />
        public async Task<Dictionary<string, SlaDataRaw>> GetRawSlaDataAsync(string environmentSubscriptionId, string elementId, DateTime start, DateTime end, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetRawSlaDataAsync started. (Environment: '{environmentSubscriptionId}', ElementId: '{elementId}')");

                var includeWarnings = await GetIncludeWarnings(environmentSubscriptionId, token).ConfigureAwait(false);

                // Calculate SLA
                var environmentId = (await _storageAbstraction.GetEnvironment(environmentSubscriptionId, token).ConfigureAwait(false)).Id;

                // Get all Elements from DB
                var elements = await _storageAbstraction.GetAllElementsOfEnvironmentTree(environmentId, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(elementId))
                {
                    elements = elements.Where(e => e.ElementId.Equals(elementId, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var result = new Dictionary<string, SlaDataRaw>();
                foreach (var element in elements)
                {
                    // If the element was created after the SLA interval -> SLA cannot be calculated
                    if (element.CreationDate == DateTime.MinValue || end < element.CreationDate)
                    {
                        //Calculate SLA
                        result[element.ElementId] = new SlaDataRaw
                        {
                            RawData = new List<Models.SLA.StateTransitionHistory>(),
                            IncludeWarnings = includeWarnings,
                            DownTime = new TimeSpan(0),
                            UpTime = new TimeSpan(0),
                            TimeInterval = new TimeSpan(end.Subtract(start).Ticks),
                            CalculatedValue = 0
                        };
                    }
                    else
                    {
                        var currentTime = DateTime.UtcNow;
                        currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second, DateTimeKind.Utc);

                        // Clamp StartDate and EndDate between CreationDate of the Element and DateTime.UtcNow 
                        var elementStart = new[] { start, element.CreationDate }.Max();
                        var elementEnd = new[] { end, currentTime }.Min();

                        //Get all entries between dates (including the entries that started before startDate or ended after endDate)
                        var entriesBetweenDates = (await _storageAbstraction.GetStateTransitionHistoriesBetweenDates(environmentId, element.ElementId, elementStart, elementEnd, token).ConfigureAwait(false)).OrderBy(e => e.StartDate).ToList();
                        if (!entriesBetweenDates.Any())
                        {
                            //Calculate SLA
                            result[element.ElementId] = new SlaDataRaw
                            {
                                RawData = entriesBetweenDates,
                                IncludeWarnings = includeWarnings,
                                DownTime = new TimeSpan(0),
                                UpTime = new TimeSpan(end.Subtract(start).Ticks),
                                TimeInterval = new TimeSpan(end.Subtract(start).Ticks),
                                CalculatedValue = 100
                            };
                        }
                        else
                        {
                            //Get the first and the last of them
                            var first = entriesBetweenDates.First();
                            var last = entriesBetweenDates.Last();

                            //Trim Start and EndDate of first and last entry
                            if (first.StartDate <= start)
                            {
                                first.StartDate = start;
                            }
                            if (last.EndDate >= end || last.EndDate == null)
                            {
                                last.EndDate = end;
                            }

                            var entriesWithoutEndDate = entriesBetweenDates.Where(e => e.EndDate == null).ToList();
                            if (entriesWithoutEndDate.Any())
                            {
                                var ids = entriesWithoutEndDate.Select(e => e.Id);
                                var message = $"Calculating SLA failed. Reason: Invalid data found for StateTransitionHistory DB entries with Ids: '{string.Join(", ", ids)}'";
                                throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                            }

                            //Count error/warning time of all entries
                            long downTime = 0;
                            foreach (var entry in entriesBetweenDates)
                            {
                                //Count Error state, if includeWarnings is set, count Warning state too
                                if (entry.State == State.Error || (includeWarnings && entry.State == State.Warning))
                                {
                                    downTime += (entry.EndDate.Value - entry.StartDate.Value).Ticks;
                                }
                            }

                            //Calculate SLA
                            result[element.ElementId] = new SlaDataRaw
                            {
                                RawData = entriesBetweenDates,
                                IncludeWarnings = includeWarnings,
                                DownTime = new TimeSpan(downTime),
                                UpTime = new TimeSpan(end.Subtract(start).Ticks - downTime),
                                TimeInterval = new TimeSpan(end.Subtract(start).Ticks),
                                CalculatedValue = CalculateSlaValue(elementStart, elementEnd, downTime)
                            };
                        }
                    }
                }
                return result;
            }
        }

        #endregion

        #region Private Methods

        private async Task<double> GetErrorThreshold(string environmentSubscriptionId, CancellationToken token)
        {
            var conf = await _storageAbstraction.GetConfiguration("SLA_Error_Threshold", environmentSubscriptionId, token).ConfigureAwait(false);
            if (double.TryParse(conf.Value, out var threshold))
            {
                return threshold * 100;
            }
            var message = $"Calculating SLA failed. Reason: Value '{conf.Value}' for configurationKey '{conf.Key}' could not be parsed.";
            throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
        }

        private async Task<double> GetWarningThreshold(string environmentSubscriptionId, CancellationToken token)
        {
            var conf = await _storageAbstraction.GetConfiguration("SLA_Warning_Threshold", environmentSubscriptionId, token).ConfigureAwait(false);
            if (double.TryParse(conf.Value, out var threshold))
            {
                return threshold * 100;
            }
            var message = $"Calculating SLA failed. Reason: Value '{conf.Value}' for configurationKey '{conf.Key}' could not be parsed.";
            throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
        }

        private async Task<bool> GetIncludeWarnings(string environmentSubscriptionId, CancellationToken token)
        {
            var conf = await _storageAbstraction.GetConfiguration("SLA_Include_Warnings", environmentSubscriptionId, token).ConfigureAwait(false);
            if (bool.TryParse(conf.Value, out var includeWarnings))
            {
                return includeWarnings;
            }
            var message = $"Calculating SLA failed. Reason: Value '{conf.Value}' for configurationKey '{conf.Key}' could not be parsed.";
            throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
        }

        private async Task<SlaData> CalculateSlaData(int environmentId, EnvironmentElement element, DateTime start, DateTime end, bool includeWarnings, double errorThreshold, double warningThreshold, CancellationToken token)
        {
            // If the element was created after the slice interval -> SLA cannot be calculated for this interval
            if (end < element.CreationDate)
            {
                return null;
            }

            //Get all entries between dates (including the entries that started before startDate or ended after endDate)
            var entriesBetweenDates = (await _storageAbstraction.GetStateTransitionHistoriesBetweenDates(environmentId, element.ElementId, start, end, token).ConfigureAwait(false)).OrderBy(e => e.StartDate).ToList();
            if (!entriesBetweenDates.Any())
            {
                // Get the ElementType
                var defaultSla = new SlaData
                {
                    Value = 100,
                    ElementType = element.ElementType,
                    StartDate = start,
                    EndDate = end,
                    Level = SlaLevel.Ok
                };
                return defaultSla;
            }

            //Get the first and the last of them
            var first = entriesBetweenDates.First();
            var last = entriesBetweenDates.Last();

            //Trim Start and EndDate of first and last entry
            if (first.StartDate <= start)
            {
                first.StartDate = start;
            }
            if (last.EndDate >= end || last.EndDate == null)
            {
                last.EndDate = end;
            }

            var entriesWithoutEndDate = entriesBetweenDates.Where(e => e.EndDate == null).ToList();
            if (entriesWithoutEndDate.Any())
            {
                var ids = entriesWithoutEndDate.Select(e => e.Id);
                var message = $"Calculating SLA failed. Reason: Invalid data found for StateTransitionHistory DB entries with Ids: '{string.Join(", ", ids)}'";
                throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
            }

            //Count error/warning time of all entries
            long errorDuration = 0;
            foreach (var entry in entriesBetweenDates)
            {
                //Count Error state, if includeWarnings is set, count Warning state too
                if (entry.State == State.Error || (includeWarnings && entry.State == State.Warning))
                {
                    errorDuration += (entry.EndDate.Value - entry.StartDate.Value).Ticks;
                }
            }

            //Calculate SLA
            var result = new SlaData
            {
                Value = CalculateSlaValue(start, end, errorDuration),
                ElementType = element.ElementType,
                StartDate = start,
                EndDate = end
            };

            var errorOrWarning = result.Value <= errorThreshold ? SlaLevel.Error : SlaLevel.Warning;
            result.Level = result.Value > warningThreshold ? SlaLevel.Ok : errorOrWarning;
            return result;
        }

        private static double CalculateSlaValue(DateTime start, DateTime end, double errorDuration)
        {
            var value = 1 - (errorDuration / (end - start).Ticks);
            value = Math.Round(value * 100, 2);
            if (value > 100)
            {
                value = 100;
            }
            else if (value < 0)
            {
                value = 0;
            }
            return value;
        }

        private static int GetDaysInTimeInterval(DateTime start, DateTime end)
        {
            var days = 0;
            var temp = new DateTime(start.Year, start.Month, start.Day);
            while (temp.Day != end.Day || temp.Month != end.Month || temp.Year != end.Year)
            {
                temp = temp.AddDays(1);
                days++;
            }
            return days + 1;
        }

        #endregion
    }
}