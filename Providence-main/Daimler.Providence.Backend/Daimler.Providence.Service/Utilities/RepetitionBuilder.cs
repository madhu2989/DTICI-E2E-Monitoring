using System;
using System.Collections.Generic;
using System.Net;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Builder for recurring appointments
    /// </summary>
    public class RepetitionBuilder
    {
        /// <summary>
        /// Method to get all dates for a recurring Deployment.
        /// </summary>
        /// <param name="startDate">The StartDate of the Deployment repetition.</param>
        /// <param name="endDate">The EndDate of the Deployment repetition.</param>
        /// <param name="repeatType">The Repetition type of the Deployment (for example: daily, weekly, monthly).</param>
        /// <param name="repeatInterval">The RepeatInterval of the Deployment (for example: Every n-th day/week/month).</param>
        /// <param name="keepWeekDay">The flag indicating if the recurring Deployment shall always be on the same day of the week within a month (for example: Every 2nd monday in a month).</param>
        public IList<DateTime> GetRepetitionDates(DateTime startDate, DateTime endDate, RepeatType repeatType, int repeatInterval, bool? keepWeekDay)
        {
            switch (repeatType)
            {
                case RepeatType.Daily:
                    return GetDailyRepetitions(startDate, endDate, repeatInterval);
                case RepeatType.Weekly:
                    return GetWeeklyRepetitions(startDate, endDate, repeatInterval);
                case RepeatType.Monthly:
                    return GetMonthlyRepetitions(startDate, endDate, repeatInterval, keepWeekDay);
                default:
                {
                    var message = $"Creating Recurring Deployment failed. Reason: Value for repeatType '{repeatType}' is unknown.";
                    throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                }
            }
        }

        #region Daily repetition

        private static IList<DateTime> GetDailyRepetitions(DateTime startDate, DateTime endDate, int repeatInterval)
        {
            IList<DateTime> values = new List<DateTime>();

            var dt = startDate;
            do
            {
                values.Add(dt);
                dt = dt.AddDays(repeatInterval);
            } while (dt <= endDate);
            return values;
        }

        #endregion

        #region Weekly repetition

        private static IList<DateTime> GetWeeklyRepetitions(DateTime startDate, DateTime endDate, int repeatInterval)
        {
            IList<DateTime> values = new List<DateTime>();

            var dt = startDate;
            do
            {
                values.Add(dt);
                dt = dt.AddDays(repeatInterval * 7);
            } while (dt <= endDate);

            return values;
        }

        #endregion

        #region Monthly repetition

        private static IList<DateTime> GetMonthlyRepetitions(DateTime startDate, DateTime endDate, int repeatInterval, bool? keepWeekday)
        {
            IList<DateTime> values = new List<DateTime>();

            var monthDay = startDate.Day;
            var weekday = startDate.DayOfWeek;
            var weekdayNumber = GetWeekdayNumber(startDate, weekday);
            var dt = startDate;
            do
            {
                if (keepWeekday.HasValue && keepWeekday == true)
                {
                    dt = GetMonthWeekdayNumber(dt, weekday, weekdayNumber);
                    values.Add(dt);
                    dt = dt.AddMonths(repeatInterval);
                }
                else
                {
                    values.Add(dt);
                    dt = dt.AddMonths(repeatInterval);
                    dt = GetCorrectedMonthDate(dt, monthDay);
                }
            } while (dt <= endDate);

            return values;
        }

        /// <summary>
        /// Correct an input date to be equal to or less than the specified month day value.
        /// </summary>
        /// <param name="input">Date to check to ensure it matches the specified month day value or
        /// the max number of days for that month, whichever comes first.</param>
        /// <param name="desiredMonthDay">Number of desired month day</param>
        /// <returns>Corrected date</returns>
        private static DateTime GetCorrectedMonthDate(DateTime input, int desiredMonthDay)
        {
            var dt = input;

            // Ensure the day value hasn't changed. This will occur if the month is Feb.
            // All dates after that will have the same day.
            if (dt.Day < desiredMonthDay && DateTime.DaysInMonth(dt.Year, dt.Month) > dt.Day)
            {
                int monthDay = desiredMonthDay > DateTime.DaysInMonth(dt.Year, dt.Month)
                    // specified day is greater than the number of days in the month
                    ? DateTime.DaysInMonth(dt.Year, dt.Month)
                    // specified date is less than number of days in month
                    : desiredMonthDay;
                dt = new DateTime(dt.Year, dt.Month, monthDay);
            }
            return dt;
        }

        private static int GetWeekdayNumber(DateTime currentDate, DayOfWeek weekday)
        {
            var daysOfMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            var day = 1;
            var weekdayCount = 0;
            var returnDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            do
            {
                // Check for day of the week
                if (returnDate.DayOfWeek == weekday)
                {
                    weekdayCount++;

                    if (currentDate.Day <= day)
                    {
                        break;
                    }
                }
                returnDate = returnDate.AddDays(1);
                day++;
            } while (day <= daysOfMonth);

            return weekdayCount;
        }

        private static DateTime GetMonthWeekdayNumber(DateTime currentDate, DayOfWeek weekday, int weekdayNumber)
        {
            var daysOfMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            var day = 1;
            var weekdayCount = 0;
            var returnDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastValue = returnDate;
            var found = false;
            do
            {
                // Check for day of the week
                if (returnDate.DayOfWeek == weekday)
                {
                    weekdayCount++;
                    lastValue = returnDate;

                    if (weekdayCount == weekdayNumber)
                    {
                        found = true;
                        break;
                    }
                }
                returnDate = returnDate.AddDays(1);
                day++;
            } while (day <= daysOfMonth);

            // If getting the last weekday of the month then set the returning value to the last weekday found.
            if (!found)
                returnDate = lastValue;

            return returnDate;
        }

        #endregion
    }
}