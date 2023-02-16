using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.ChangeLog;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Daimler.Providence.Service.DAL
{
    public partial class DataAccessLayer
    {
        #region Configuration

        /// <inheritdoc />
        public async Task<GetConfiguration> GetConfiguration(string configurationKey, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetConfiguration started. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbConfiguration = await dbContext.Configurations.FirstOrDefaultAsync(c => c.Key.ToLower() == configurationKey.ToLower() && c.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                    if (dbConfiguration == null)
                    {
                        var message = $"Configuration doesn't exist in the Database. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')";
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, dbConfiguration);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetConfiguration>> GetConfigurations(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetConfigurations started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var configurations = new ConcurrentBag<GetConfiguration>();
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    var dbConfigurations = await dbContext.Configurations.Where(c => c.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    foreach (var dbConfiguration in dbConfigurations)
                    {
                        var configuration = ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, dbConfiguration);
                        configurations.Add(configuration);
                    }
                    return configurations.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetConfiguration> AddConfiguration(PostConfiguration configuration, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddConfiguration started. (ConfigurationKey: '{configuration.Key}', Environment: '{configuration.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message;
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(configuration.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Check if item exists already
                    var dbConfigurationExists = await dbContext.Configurations.FirstOrDefaultAsync(c => c.Key.ToLower() == configuration.Key.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbConfigurationExists != null)
                    {
                        message = $"Configuration already exists in Database. (ConfigurationKey: '{configuration.Key}', Environment: '{configuration.EnvironmentSubscriptionId}')";
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }

                    var newConfiguration = ProvidenceModelMapper.MapMdConfigurationToDbConfiguration(dbEnvironment, configuration);
                    dbContext.Configurations.Add(newConfiguration);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbConfiguration = await dbContext.Configurations.FirstOrDefaultAsync(d => d.Key.ToLower() == configuration.Key.ToLower() && d.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (newDbConfiguration == null)
                    {
                        message = $"Configuration could not be created. (ConfigurationKey: '{configuration.Key}', Environment: '{configuration.EnvironmentSubscriptionId}')";
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, newDbConfiguration));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newDbConfiguration.Id, ChangeElementType.Configuration, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);

                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, newDbConfiguration);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateConfiguration(string configurationKey, string environmentSubscriptionId, PutConfiguration configuration, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateConfiguration started. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Check if item exists already
                    var dbConfiguration = await dbContext.Configurations.FirstOrDefaultAsync(c => c.Key.ToLower() == configurationKey.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbConfiguration != null)
                    {
                        var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, dbConfiguration));
                        dbConfiguration.Value = configuration.Value;
                        if (configuration.Description != null)
                        {
                            dbConfiguration.Description = configuration.Description;
                        }

                        // Create ChangeLog Entry
                        var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, dbConfiguration));
                        var changeLog = CreateChangeLog(ChangeOperation.Update, dbConfiguration.Id, ChangeElementType.Configuration, dbEnvironment, oldValue, newValue);
                        dbContext.Changelogs.Add(changeLog);
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new ProvidenceException($"Configuration doesn't exist in the Database. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')", HttpStatusCode.NotFound);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteConfiguration(string configurationKey, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                using (var dbContext = GetContext())
                {
                    AILogger.Log(SeverityLevel.Information, $"DeleteConfiguration started. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')");
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbConfiguration = await dbContext.Configurations.FirstOrDefaultAsync(c => c.Key.ToLower() == configurationKey.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbConfiguration == null)
                    {
                        var message = $"Configuration doesn't exist in the Database. (ConfigurationKey: '{configurationKey}', Environment: '{environmentSubscriptionId}')";
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbConfigurationToMdConfiguration(dbEnvironment, dbConfiguration));
                    dbContext.Configurations.Remove(dbConfiguration);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, dbConfiguration.Id, ChangeElementType.Configuration, dbEnvironment, oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}