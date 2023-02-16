using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using DB = Daimler.Providence.Database;

namespace Daimler.Providence.Service.DAL
{
    public partial class DataAccessLayer
    {
        #region ChangeLogs

        /// <inheritdoc />
        public async Task<GetChangeLog> GetChangeLog(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetChangeLog started. (ChangeLogId: '{id}').");
                using (var dbContext = GetContext())
                {
                    var environments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbChangeLog = await dbContext.Changelogs.FirstOrDefaultAsync(c => c.Id == id, token).ConfigureAwait(false);
                    if (dbChangeLog == null)
                    {
                        throw new ProvidenceException($"ChangeLog doesn't exist in the Database. (ChangeLogId: '{id}')", HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbChangeLogToMdChangeLog(environments, dbChangeLog);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetChangeLog>> GetChangeLogs(DateTime startDate, DateTime endDate, int? elementId, string environmentName, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var changeLogs = new ConcurrentBag<GetChangeLog>();
                using (var dbContext = GetContext())
                {
                    List<DB.Changelog> dbChangeLogs;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    if (elementId.HasValue)
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetChangeLogs started. (StartDate: '{startDate}', EndDate: '{endDate}', Element: '{elementId}')");
                        dbChangeLogs = await dbContext.Changelogs
                            .Where(c => c.ElementId == elementId.Value)
                            .Where(c => c.ChangeDate >= startDate && c.ChangeDate <= endDate).ToListAsync(token).ConfigureAwait(false);
                    }
                    else if (!string.IsNullOrEmpty(environmentName))
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetChangeLogs started. (StartDate: '{startDate}', EndDate: '{endDate}', Environment: '{environmentName}')");
                        var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);
                        dbChangeLogs = await dbContext.Changelogs
                            .Where(c => c.EnvironmentId == dbEnvironment.Id)
                            .Where(c => c.ChangeDate >= startDate && c.ChangeDate <= endDate).ToListAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetChangeLogs started. (StartDate: '{startDate}', EndDate: '{endDate}')");
                        dbChangeLogs = await dbContext.Changelogs.Where(c => c.ChangeDate >= startDate && c.ChangeDate <= endDate).ToListAsync(token).ConfigureAwait(false);
                    }
                    dbChangeLogs.ForEach(dbChangeLog => changeLogs.Add(ProvidenceModelMapper.MapDbChangeLogToMdChangeLog(dbEnvironments, dbChangeLog)));
                }
                return changeLogs.ToList();
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task DeleteExpiredChangeLogs(DateTime cutOffDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteExpiredChangeLogs started. (CutOffDate: '{cutOffDate}')");
                using (var dbContext = GetContext())
                {
                    var count = (await dbContext.GetChangelogsCount().ConfigureAwait(false)).FirstOrDefault()?.ChangelogsCount;
                    AILogger.Log(SeverityLevel.Information, $"ChangeLogs count before deleting: '{count}' ");

                    await dbContext.DeleteExpiredChangelogs(cutOffDate).ConfigureAwait(false);
                    count = (await dbContext.GetChangelogsCount().ConfigureAwait(false)).FirstOrDefault()?.ChangelogsCount;
                    AILogger.Log(SeverityLevel.Information, $"ChangeLogs count after deleting: '{count}' ");
                }
            }
        }

        #endregion

        #region Private Methods

        private static DB.Changelog CreateChangeLog(ChangeOperation operation, int elementId, ChangeElementType elementType, DB.Environment environment, string oldValue, string newValue)
        {
            return new DB.Changelog
            {
                Environment = environment,
                ChangeDate = DateTime.UtcNow,
                Username = ThreadContext.GetCurrentUserName(),
                ElementId = elementId,
                ElementType = (int)elementType,
                EnvironmentId = environment.Id,
                Operation = (int)operation,
                ValueOld = oldValue,
                ValueNew = newValue
            };
        }

        #endregion
    }
}