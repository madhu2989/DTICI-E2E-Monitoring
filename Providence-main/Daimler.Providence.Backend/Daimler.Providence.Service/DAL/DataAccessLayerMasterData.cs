using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.ChangeLog;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.SLA;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using Action = Daimler.Providence.Service.Models.ImportExport.Action;
using Check = Daimler.Providence.Service.Models.ImportExport.Check;
using Component = Daimler.Providence.Service.Models.ImportExport.Component;
using DB = Daimler.Providence.Database;
using Environment = Daimler.Providence.Service.Models.ImportExport.Environment;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Models.StateTransition;
using Microsoft.EntityFrameworkCore;

namespace Daimler.Providence.Service.DAL
{
    public partial class DataAccessLayer
    {
        #region Environments

        /// <inheritdoc />
        public async Task<GetEnvironment> GetEnvironment(string elementId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetEnvironment started. (ElementId: '{elementId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await dbContext.Environments.Include(e => e.Services).Include(e => e.Checks).FirstOrDefaultAsync(e => e.ElementId.ToLower() == elementId.ToLower(), token).ConfigureAwait(false);
                    if (dbEnvironment == null)
                    {
                        var message = $"Environment doesn't exist in the Database. (ElementId: '{elementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbEnvironmentToMdEnvironment(dbEnvironment);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetEnvironment>> GetEnvironments(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var environments = new ConcurrentBag<GetEnvironment>();
                AILogger.Log(SeverityLevel.Information, "GetEnvironments started.");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.Include(e => e.Services).Include(e => e.Checks).ToListAsync(token).ConfigureAwait(false);
                    Parallel.ForEach(dbEnvironments, e => environments.Add(ProvidenceModelMapper.MapDbEnvironmentToMdEnvironment(e)));
                }
                return environments.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetEnvironment> AddEnvironment(PostEnvironment environment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddEnvironment started. (ElementId: '{environment?.ElementId}')");
                using (var dbContext = GetContext())
                {
                    string message;
                    var dbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId.ToLower() == environment.ElementId.ToLower() || e.Name.ToLower() == environment.Name.ToLower(), token).ConfigureAwait(false);
                    if (dbEnvironment != null)
                    {
                        message = $"Environment with same Name/ElementId already exists in the Database. (Name: '{environment?.Name}', ElementId: '{environment?.ElementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }
                    var newEnvironment = ProvidenceModelMapper.MapMdEnvironmentToDbEnvironment(environment);
                    dbContext.Environments.Add(newEnvironment);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId.ToLower() == environment.ElementId.ToLower(), token).ConfigureAwait(false);
                    if (newDbEnvironment == null)
                    {
                        message = $"Environment could not be created. (ElementId: '{environment?.ElementId}')";
                        AILogger.Log(SeverityLevel.Error, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    return ProvidenceModelMapper.MapDbEnvironmentToMdEnvironment(newDbEnvironment);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateEnvironment(string elementId, PutEnvironment environment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateEnvironment started. (ElementId: '{elementId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId.ToLower() == elementId.ToLower(), token).ConfigureAwait(false);
                    if (dbEnvironment == null)
                    {
                        var message = $"Environment doesn't exist in the Database. (ElementId: '{elementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // In case of Name change: Check if there is already an other Environment with the same Name
                    var duplicateDbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.Name.ToLower() == environment.Name.ToLower() && e.ElementId.ToLower() != elementId.ToLower(), token).ConfigureAwait(false);
                    if (duplicateDbEnvironment != null)
                    {
                        var message = $"Environment with same Name already exists in the Database. (Name: '{environment?.Name}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }
                    dbEnvironment.Description = environment?.Description;
                    dbEnvironment.Name = environment?.Name;
                    dbEnvironment.IsDemo = environment?.IsDemo;
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteEnvironment(string elementId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteEnvironment started. (ElementId: '{elementId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower()).ConfigureAwait(false);
                    if (dbEnvironment == null)
                    {
                        var message = $"Environment doesn't exist in the Database. (ElementId: '{elementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    dbContext.Environments.Remove(dbEnvironment);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var dbChecks = await dbContext.Checks.Where(c => c.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    foreach (var dbCheck in dbChecks)
                    {
                        dbContext.Checks.Remove(dbCheck);
                    }
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    await Task.Run(() => dbContext.DeleteUnusedComponents(), token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Services

        /// <inheritdoc />
        public async Task<GetService> GetService(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetService started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbService = await dbContext.Services.Include(s => s.Actions).FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        var message = $"Service doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbServiceToMdService(dbContext.Environments, dbService);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetService>> GetServices(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var services = new ConcurrentBag<GetService>();
                using (var dbContext = GetContext())
                {
                    List<DB.Service> dbServices;
                    if (string.IsNullOrEmpty(environmentSubscriptionId))
                    {
                        AILogger.Log(SeverityLevel.Information, "GetServices started. (Environment: 'All')");
                        dbServices = await dbContext.Services.Include(s => s.Actions).ToListAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetServices started. (Environment: '{environmentSubscriptionId}')");
                        var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                        dbServices = await dbContext.Services.Where(s => s.EnvironmentId == dbEnvironment.Id).Include(s => s.Actions).ToListAsync(token).ConfigureAwait(false);
                    }
                    dbServices.ForEach(dbService => services.Add(ProvidenceModelMapper.MapDbServiceToMdService(dbContext.Environments, dbService)));
                }
                return services.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetService> AddService(PostService service, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddService started. (ElementId: '{service?.ElementId}', Environment: '{service?.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message;

                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(service?.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var allEnvironmentElements = await dbContext.GetAllElementsWithEnvironmentId(dbEnvironment?.Id, token).ConfigureAwait(false);

                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == service.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService != null || allEnvironmentElements.ToList().Any(e => e.ElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        message = dbService != null
                            ? $"Service already exists in the Database. (ElementId: '{service?.ElementId}', Environment: '{service?.EnvironmentSubscriptionId}') )"
                            : $"Another Element of type '{allEnvironmentElements.FirstOrDefault(e => e.ElementId.Equals(service.ElementId, StringComparison.OrdinalIgnoreCase))?.Type}' already exists in the Database. (ElementId: '{service?.ElementId}', Environment: '{service?.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }

                    var newService = ProvidenceModelMapper.MapMdServiceToDbService(dbContext.Environments, service);

                    // Get all Actions for the Service
                    var serviceActions = new ConcurrentBag<DB.Action>();
                    if (service?.Actions != null)
                    {
                        var dbActions = await dbContext.Actions.ToListAsync(token).ConfigureAwait(false);
                        foreach (var serviceAction in service.Actions.Distinct())
                        {
                            var dbAction = dbActions.FirstOrDefault(a => a.ElementId.Equals(serviceAction, StringComparison.OrdinalIgnoreCase) && a.EnvironmentId == dbEnvironment.Id);
                            if (dbAction == null)
                            {
                                message = $"Action doesn't exist in the Database. (ElementId: '{serviceAction}', Environment: '{service.EnvironmentSubscriptionId}')";
                                AILogger.Log(SeverityLevel.Warning, message);
                                throw new ProvidenceException(message, HttpStatusCode.NotFound);
                            }
                            serviceActions.Add(dbAction);
                        }
                        serviceActions.ToList().ForEach(a => newService.Actions.Add(a));
                    }
                    dbContext.Services.Add(newService);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == service.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (newDbService == null)
                    {
                        message = $"Service could not be created. (ElementId: '{service?.ElementId}', Environment: '{service?.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    return ProvidenceModelMapper.MapDbServiceToMdService(dbContext.Environments, newDbService);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateService(string elementId, string environmentSubscriptionId, PutService service, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateService started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId})");
                using (var dbContext = GetContext())
                {
                    string message;

                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        message = $"Service doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Get all Actions for the Service
                    var serviceActions = new ConcurrentBag<DB.Action>();
                    if (service?.Actions != null)
                    {
                        var dbActions = await dbContext.Actions.ToListAsync(token).ConfigureAwait(false);
                        foreach (var serviceAction in service.Actions.Distinct())
                        {
                            var dbAction = dbActions.FirstOrDefault(a => a.ElementId.Equals(serviceAction, StringComparison.OrdinalIgnoreCase) && a.EnvironmentId == dbEnvironment.Id);
                            if (dbAction == null)
                            {
                                message = $"Action doesn't exist in the Database. (ElementId: '{serviceAction}')";
                                AILogger.Log(SeverityLevel.Warning, message);
                                throw new ProvidenceException(message, HttpStatusCode.NotFound);
                            }
                            serviceActions.Add(dbAction);
                        }

                        // All Actions which are now unassigned need to be deleted -> unassigned Actions are not allowed
                        var actionsToDelete = dbService.Actions.Where(s => !serviceActions.Any(es => es.ElementId.Equals(s.ElementId, StringComparison.OrdinalIgnoreCase) && s.EnvironmentId == dbEnvironment.Id)).ToList();
                        foreach (var actionToDelete in actionsToDelete)
                        {
                            var dbAction = await dbContext.Actions.FirstOrDefaultAsync(s => s.ElementId.ToLower() == actionToDelete.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                            if (dbAction != null)
                            {
                                dbContext.Actions.Remove(dbAction);
                                AILogger.Log(SeverityLevel.Information, $"Deleting unassigned Actions. (ElementId: '{actionToDelete.ElementId}', Environment: '{dbEnvironment.ElementId}')");
                            }
                        }
                        dbService.Actions.Clear();
                        serviceActions.ToList().ForEach(a => dbService.Actions.Add(a));
                    }
                    dbService.Name = service?.Name;
                    dbService.Description = service?.Description;
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteService(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteService started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        var message = $"Service doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    dbContext.Services.Remove(dbService);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Actions

        /// <inheritdoc />
        public async Task<GetAction> GetAction(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAction started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbServices = await dbContext.Services.ToListAsync(token).ConfigureAwait(false);

                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbAction = await dbContext.Actions.Include(a => a.Components).FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbAction == null)
                    {
                        var message = $"Action doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbActionToMdAction(dbEnvironments, dbServices, dbAction);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetAction>> GetActions(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var actions = new ConcurrentBag<GetAction>();
                using (var dbContext = GetContext())
                {
                    List<DB.Action> dbActions;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbServices = await dbContext.Services.ToListAsync(token).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(environmentSubscriptionId))
                    {
                        AILogger.Log(SeverityLevel.Information, "GetActions started.");
                        dbActions = await dbContext.Actions.Include(a => a.Components).ToListAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetActions started. (Environment: '{environmentSubscriptionId}')");
                        var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                        dbActions = await dbContext.Actions.Include(a => a.Components).Where(s => s.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    }
                    dbActions.ForEach(dbAction => actions.Add(ProvidenceModelMapper.MapDbActionToMdAction(dbEnvironments, dbServices, dbAction)));
                }
                return actions.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetAction> AddAction(PostAction action, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddAction started. (ElementId: '{action?.ElementId}', Environment: '{action?.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message;

                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbServices = await dbContext.Services.ToListAsync(token).ConfigureAwait(false);

                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(action?.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var allEnvironmentElements = await dbContext.GetAllElementsWithEnvironmentId(dbEnvironment?.Id, token);
                    var dbAction = await dbContext.Actions.FirstOrDefaultAsync(s => s.ElementId.ToLower() == action.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbAction != null || allEnvironmentElements.Any(e => e.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        message = dbAction != null
                            ? $"Action already exists in the Database. (ElementId: '{action?.ElementId}', Environment: '{action?.EnvironmentSubscriptionId}')"
                            : $"Another Element of type '{allEnvironmentElements.FirstOrDefault(e => e.ElementId.Equals(action.ElementId, StringComparison.OrdinalIgnoreCase))?.Type}' already exists in the Database. (ElementId: '{action?.ElementId}', Environment: '{action?.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }

                    // Check the ServiceId
                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == action.ServiceElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        message = $"Service doesn't exist in the Database. (ElementId: '{action?.ServiceElementId}', Environment: '{action?.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var newAction = ProvidenceModelMapper.MapMdActionToDbAction(dbEnvironments, dbServices, action);

                    // Get all Actions for the Service
                    var actionComponents = new List<DB.Component>();
                    if (action?.Components != null)
                    {
                        var dbComponents = await dbContext.Components.ToListAsync(token).ConfigureAwait(false);
                        foreach (var actionComponent in action.Components.Distinct())
                        {
                            var dbComponent = dbComponents.FirstOrDefault(a => a.ElementId.Equals(actionComponent, StringComparison.OrdinalIgnoreCase) && a.EnvironmentId == dbEnvironment.Id);
                            if (dbComponent == null)
                            {
                                message = $"Action doesn't exist in the Database. (ElementId: '{action.ElementId}', Environment: '{action.EnvironmentSubscriptionId}')";
                                AILogger.Log(SeverityLevel.Warning, message);
                                throw new ProvidenceException(message, HttpStatusCode.NotFound);
                            }
                            actionComponents.Add(dbComponent);
                        }
                        actionComponents.ForEach(c => newAction.Components.Add(c));
                    }
                    dbContext.Actions.Add(newAction);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbAction = await dbContext.Actions.FirstOrDefaultAsync(s => s.ElementId.ToLower() == action.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (newDbAction == null)
                    {
                        message = $"Action could not be created. (ElementId: '{action?.ElementId}', Environment: '{action?.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    return ProvidenceModelMapper.MapDbActionToMdAction(dbEnvironments, dbServices, newDbAction);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateAction(string elementId, string environmentSubscriptionId, PutAction action, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateAction started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message; DB.Environment targetdbEnvironment = null;


                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(action.SubscriptionId))
                    {
                        if (!action.SubscriptionId.Equals(environmentSubscriptionId))
                        {
                            targetdbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(action.SubscriptionId, dbContext).ConfigureAwait(false);
                        }
                    }

                    var dbAction = await dbContext.Actions.Include(a => a.MappingActionComponents).FirstOrDefaultAsync(a => a.ElementId.ToLower() == elementId.ToLower() && a.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbAction == null)
                    {
                        message = $"Action doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Check the ServiceId
                    var dbService = await dbContext.Services.FirstOrDefaultAsync(s => s.ElementId.ToLower() == action.ServiceElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbService == null)
                    {
                        message = $"Service doesn't exist in the Database. (ElementId: '{action?.ServiceElementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    dbAction.ServiceId = dbService.Id;

                    if (targetdbEnvironment != null)
                    {
                        var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);

                        var actionComponents = new List<DB.Component>();
                        if (action?.Components != null)
                        {
                            var dbComponents = await dbContext.Components.ToListAsync(token).ConfigureAwait(false);
                            foreach (var actionComponent in action.Components.Distinct())
                            {
                                var dbComponent = dbComponents.FirstOrDefault(c => c.ElementId.Equals(actionComponent, StringComparison.OrdinalIgnoreCase) && c.EnvironmentId == targetdbEnvironment.Id);
                                if (dbComponent == null)
                                {
                                    message = $"Component doesn't exist in the Database.  (ElementId: '{actionComponent}', Environment: '{environmentSubscriptionId}')";
                                    AILogger.Log(SeverityLevel.Warning, message);
                                    throw new ProvidenceException(message, HttpStatusCode.NotFound);
                                }
                                var dbComponentExists = dbComponents.FirstOrDefault(c => c.ElementId.Equals(actionComponent, StringComparison.OrdinalIgnoreCase) && c.EnvironmentId == dbEnvironment.Id);
                                if (dbComponentExists == null)
                                {
                                    PostComponent newComp = new PostComponent() { ElementId = dbComponent.ElementId, Description = dbComponent.Description, ComponentType = dbComponent.ComponentType, Name = dbComponent.Name, EnvironmentSubscriptionId = dbEnvironment.ElementId };
                                    var newComponent = ProvidenceModelMapper.MapMdComponentToDbComponent(dbEnvironments, newComp);
                                    dbContext.Components.Add(newComponent);
                                }
                                else
                                {
                                    if (dbComponentExists.Description.Contains(ProvidenceConstants.UnassignedServiceName))
                                    {
                                        dbComponentExists.Description = dbComponent.Description;
                                        dbComponentExists.Name = dbComponent.Name;
                                        var dbActionUnassignedMapping = await dbContext.MappingActionComponents.FirstOrDefaultAsync(a => a.Action.Name.Equals(ProvidenceConstants.UnassignedActionName) && a.ComponentId == dbComponentExists.Id && a.Action.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                                        if (dbActionUnassignedMapping != null)
                                        {
                                            dbContext.MappingActionComponents.Remove(dbActionUnassignedMapping);
                                        }
                                    }
                                }
                                //actionComponents.Add(dbComponent);
                            }
                            await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                            dbAction.Components.Clear();
                            foreach (var actionComponent in action.Components.Distinct())
                            {
                                var mapDbComponent = await dbContext.Components.FirstOrDefaultAsync(c => c.ElementId.ToLower().Equals(actionComponent.ToLower()) && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                                if (mapDbComponent == null)
                                {
                                    message = $"Component could not be created. (ElementId: '{mapDbComponent.ElementId}', Environment: '{mapDbComponent.EnvironmentId}')";
                                    AILogger.Log(SeverityLevel.Warning, message);
                                    throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                                }
                                dbAction.Components.Add(mapDbComponent);
                            }

                        }
                        dbAction.Name = action?.Name;
                        dbAction.Description = action?.Description;
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        // Get all Actions for the Service

                        var actionComponents = new List<DB.Component>();
                        if (action?.Components != null)
                        {
                            var dbComponents = await dbContext.Components.ToListAsync(token).ConfigureAwait(false);
                            foreach (var actionComponent in action.Components.Distinct())
                            {
                                var dbComponent = dbComponents.FirstOrDefault(c => c.ElementId.Equals(actionComponent, StringComparison.OrdinalIgnoreCase) && c.EnvironmentId == dbEnvironment.Id);
                                if (dbComponent == null)
                                {
                                    message = $"Component doesn't exist in the Database.  (ElementId: '{actionComponent}', Environment: '{environmentSubscriptionId}')";
                                    AILogger.Log(SeverityLevel.Warning, message);
                                    throw new ProvidenceException(message, HttpStatusCode.NotFound);
                                }
                                actionComponents.Add(dbComponent);
                            }
                            dbAction.Components.Clear();
                            actionComponents.ForEach(c => dbAction.Components.Add(c));
                        }
                        dbAction.Name = action?.Name;
                        dbAction.Description = action?.Description;
                        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    }

                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteAction(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteAction started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbAction = await dbContext.Actions.FirstOrDefaultAsync(a => a.ElementId.ToLower() == elementId.ToLower() && a.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbAction == null)
                    {
                        var message = $"Action doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    dbContext.Actions.Remove(dbAction);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Components

        /// <inheritdoc />
        public async Task<GetComponent> GetComponent(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetComponent started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbComponent = await dbContext.Components.Include(c => c.Actions).FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbComponent == null)
                    {
                        var message = $"Component doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbComponentToMdComponent(dbEnvironments, dbComponent);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetComponent>> GetComponents(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var components = new ConcurrentBag<GetComponent>();
                using (var dbContext = GetContext())
                {
                    List<DB.Component> dbComponents;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(environmentSubscriptionId))
                    {
                        AILogger.Log(SeverityLevel.Information, "GetComponents started. (Environment: 'All')");
                        dbComponents = await dbContext.Components.Include(c => c.Actions).ToListAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetComponents started. (Environment: '{environmentSubscriptionId}')");
                        var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                        dbComponents = await dbContext.Components.Include(c => c.Actions).Where(s => s.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    }
                    dbComponents.ForEach(c => components.Add(ProvidenceModelMapper.MapDbComponentToMdComponent(dbEnvironments, c)));
                }
                return components.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetComponent> AddComponent(PostComponent component, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddComponent started. (ElementId: '{component.ElementId}', Environment: '{component.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(component?.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    var allEnvironmentElements = await dbContext.GetAllElementsWithEnvironmentId(dbEnvironment?.Id, token);
                    var dbComponent = await dbContext.Components.FirstOrDefaultAsync(s => s.ElementId.ToLower() == component.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbComponent != null || allEnvironmentElements.Any(e => e.ElementId.Equals(component.ElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        message = dbComponent != null
                            ? $"Component already exists in the Database. (ElementId: '{component.ElementId}', Environment: '{component.EnvironmentSubscriptionId}')"
                            : $"Another Element of type '{allEnvironmentElements.FirstOrDefault(e => e.ElementId.Equals(component.ElementId, StringComparison.OrdinalIgnoreCase))?.Type}' already exists in the Database. (ElementId: '{component.ElementId}', Environment: '{component.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }

                    var newComponent = ProvidenceModelMapper.MapMdComponentToDbComponent(dbEnvironments, component);

                    dbContext.Components.Add(newComponent);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbComponent = await dbContext.Components.FirstOrDefaultAsync(s => s.ElementId.ToLower() == component.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (newDbComponent == null)
                    {
                        message = $"Component could not be created. (ElementId: '{component.ElementId}', Environment: '{component.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    return ProvidenceModelMapper.MapDbComponentToMdComponent(dbEnvironments, newDbComponent);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateComponent(string elementId, string environmentSubscriptionId, PutComponent component, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateComponent started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbComponent = await dbContext.Components.FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbComponent == null)
                    {
                        var message = $"Component doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    dbComponent.Name = component?.Name;
                    dbComponent.Description = component?.Description;
                    dbComponent.ComponentType = component?.ComponentType;

                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteComponent(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteComponent started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbComponent = await dbContext.Components.FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbComponent == null)
                    {
                        var message = $"Component doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    dbContext.Components.Remove(dbComponent);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Checks

        /// <inheritdoc />
        public async Task<GetCheck> GetCheck(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetCheck started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbCheck = await dbContext.Checks.FirstOrDefaultAsync(s => s.ElementId.ToLower() == elementId.ToLower() && s.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbCheck == null)
                    {
                        var message = $"Check doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, dbCheck);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetCheck>> GetChecks(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var checks = new ConcurrentBag<GetCheck>();
                using (var dbContext = GetContext())
                {
                    List<DB.Check> dbChecks;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(environmentSubscriptionId))
                    {
                        AILogger.Log(SeverityLevel.Information, "GetChecks started. (Environment: 'All')");
                        dbChecks = await dbContext.Checks.ToListAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"GetChecks started. (Environment: '{environmentSubscriptionId}')");

                        var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                        dbChecks = await dbContext.Checks.Where(s => s.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    }
                    dbChecks.ForEach(c => checks.Add(ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, c)));
                    return checks.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetCheck> AddCheck(PostCheck check, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddCheck started. (ElementId: '{check.ElementId}', Environment: '{check.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    string message;
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(check?.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var allEnvironmentElements = await dbContext.GetAllElementsWithEnvironmentId(dbEnvironment?.Id, token);
                    var dbCheck = await dbContext.Checks.FirstOrDefaultAsync(c => c.ElementId.ToLower() == check.ElementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbCheck != null || allEnvironmentElements.Any(e => e.ElementId.Equals(check.ElementId, StringComparison.OrdinalIgnoreCase)))
                    {
                        message = dbCheck != null
                            ? $"Check already exists in the Database. (ElementId: '{check.ElementId}', Environment: '{check.EnvironmentSubscriptionId}')"
                            : $"Another Element of type '{allEnvironmentElements.FirstOrDefault(e => e.ElementId.Equals(check.ElementId, StringComparison.OrdinalIgnoreCase))?.Type}' already exists in the Database. (ElementId: '{check.ElementId}', Environment: '{check.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.Conflict);
                    }

                    var newCheck = ProvidenceModelMapper.MapMdCheckToDbCheck(dbEnvironments, check);
                    dbContext.Checks.Add(newCheck);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbCheck = await dbContext.Checks.FirstOrDefaultAsync(c => c.ElementId.ToLower() == check.ElementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (newDbCheck == null)
                    {
                        message = $"Check could not be created. (ElementId: '{check.ElementId}', Environment: '{check.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, newDbCheck));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newDbCheck.Id, ChangeElementType.Check, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, newDbCheck);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateCheck(string elementId, string environmentSubscriptionId, PutCheck check, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateCheck started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbCheck = await dbContext.Checks.FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbCheck == null)
                    {
                        var message = $"Check doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, dbCheck));

                    dbCheck.Name = check?.Name;
                    dbCheck.Description = check?.Description;
                    dbCheck.Frequency = check?.Frequency;
                    dbCheck.VstsStory = check?.VstsLink;

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, dbCheck));
                    var changeLog = CreateChangeLog(ChangeOperation.Update, dbCheck.Id, ChangeElementType.Check, dbEnvironment, oldValue, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteCheck(string elementId, string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteCheck started. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbCheck = await dbContext.Checks.FirstOrDefaultAsync(c => c.ElementId.ToLower() == elementId.ToLower() && c.EnvironmentId == dbEnvironment.Id, token).ConfigureAwait(false);
                    if (dbCheck == null)
                    {
                        var message = $"Check doesn't exist in the Database. (ElementId: '{elementId}', Environment: '{environmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbCheckToMdCheck(dbEnvironments, dbCheck));
                    dbContext.Checks.Remove(dbCheck);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, dbCheck.Id, ChangeElementType.Check, dbEnvironment, oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Deployments

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetDeploymentHistory(string environmentSubscriptionId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetDeploymentHistory started. (StartDate: '{startDate}', EndDate: '{endDate}', Environment: '{environmentSubscriptionId}')");
                var deployments = new ConcurrentBag<GetDeployment>();
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var history = await dbContext.GetDeploymentHistory(dbEnvironment.Id, startDate, endDate, token).ConfigureAwait(false);
                    history.ForEach(d => deployments.Add(ProvidenceModelMapper.MapDeploymentHistory(d)));
                    return deployments.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetDeployment> GetDeployment(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetDeploymentHistory started. (DeploymentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbDeployment = await dbContext.Deployments.FirstOrDefaultAsync(e => e.Id == id, token).ConfigureAwait(false);
                    if (dbDeployment == null)
                    {
                        AILogger.Log(SeverityLevel.Warning, $"Deployment doesn't exist in the Database. (DeploymentId: '{id}')");
                        throw new ProvidenceException($"Deployment doesn't exist in the Database. (DeploymentId: '{id}')", HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, dbDeployment);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetDeployment>> GetDeployments(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var deployments = new ConcurrentBag<GetDeployment>();
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(environmentSubscriptionId))
                    {
                        var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                        AILogger.Log(SeverityLevel.Information, $"GetDeployments started. (Environment: '{environmentSubscriptionId}')");
                        var dbDeployments = await dbContext.Deployments.Where(d => (!d.ParentId.HasValue || d.ParentId == 0) && d.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                        dbDeployments.ForEach(d => deployments.Add(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, d)));
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, "GetDeployments started. (Environment: 'All')");
                        var dbDeployments = await dbContext.Deployments.Where(d => !d.ParentId.HasValue || d.ParentId == 0).ToListAsync(token).ConfigureAwait(false);
                        dbDeployments.ForEach(d => deployments.Add(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, d)));
                    }
                }
                return deployments.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetDeployment> AddDeployment(PostDeployment deployment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddDeployment started. (Environment: '{deployment.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(deployment.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);

                    var newDeployment = ProvidenceModelMapper.MapMdDeploymentToDbDeployment(dbEnvironments, deployment);
                    await dbContext.Deployments.AddAsync(newDeployment);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    var newDbDeployment = await dbContext.Deployments.FirstOrDefaultAsync(d => d.Id == newDeployment.Id, token).ConfigureAwait(false);
                    if (newDbDeployment == null)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Creating Deployment failed. (Environment: '{deployment.EnvironmentSubscriptionId}')");
                        throw new ProvidenceException($"Creating Deployment failed. (Environment: '{deployment.EnvironmentSubscriptionId}')", HttpStatusCode.NotFound);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, newDbDeployment));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newDbDeployment.Id, deployment.RepeatInformation == null ? ChangeElementType.Deployment : ChangeElementType.DeploymentRecurring, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, newDbDeployment);
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetDeployment> UpdateDeployment(int id, PutDeployment deployment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateDeployment started. (DeploymentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbDeployment = await dbContext.Deployments.FirstOrDefaultAsync(d => d.Id == id, token).ConfigureAwait(false);
                    if (dbDeployment == null)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Deployment doesn't exist in the Database. (DeploymentId: '{id}')");
                        throw new ProvidenceException($"Deployment doesn't exist in the Database. (DeploymentId: '{id}')", HttpStatusCode.NotFound);
                    }

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, dbDeployment));

                    // Merge all ElementIds to a string an remove the last ','
                    dbDeployment.ElementIds = deployment.ElementIds.Aggregate((current, next) => current + "," + next);
                    dbDeployment.Description = deployment.Description;
                    dbDeployment.ShortDescription = deployment.ShortDescription;
                    dbDeployment.StartDate = deployment.StartDate;
                    dbDeployment.EndDate = deployment.EndDate;
                    dbDeployment.RepeatInformation = deployment.RepeatInformation == null ? null : JsonConvert.SerializeObject(deployment.RepeatInformation);
                    dbDeployment.CloseReason = deployment.CloseReason;

                    // Delete existing child deployments (because of possible update of recurring deployment to single deployment)
                    var dbDeployments = dbContext.Deployments.Where(d => d.ParentId == id);
                    if (await dbDeployments.AnyAsync(token).ConfigureAwait(false))
                    {
                        dbContext.Deployments.RemoveRange(dbDeployments);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, dbDeployment));
                    var changeLog = CreateChangeLog(ChangeOperation.Update, dbDeployment.Id, deployment.RepeatInformation == null ? ChangeElementType.Deployment : ChangeElementType.DeploymentRecurring,
                        dbEnvironments.FirstOrDefault(e => e.Id == dbDeployment.EnvironmentId), oldValue, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, dbDeployment);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteDeployment(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteDeployment started. (DeploymentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbDeployments = await dbContext.Deployments.Where(d => d.Id == id || d.ParentId == id).ToListAsync(token).ConfigureAwait(false);
                    if (!dbDeployments.Any())
                    {
                        AILogger.Log(SeverityLevel.Error, $"Deployment doesn't exist in the Database. (DeploymentId: '{id}')");
                        throw new ProvidenceException($"Deployment doesn't exist in the Database. (DeploymentId: '{id}')", HttpStatusCode.NotFound);
                    }
                    var dbDeployment = dbDeployments.First(d => d.Id == id);

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, dbDeployment));
                    dbContext.Deployments.RemoveRange(dbDeployments);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, id, dbDeployment.RepeatInformation == null ? ChangeElementType.Deployment : ChangeElementType.DeploymentRecurring,
                        dbEnvironments.FirstOrDefault(e => e.Id == dbDeployment.EnvironmentId), oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region StateTransitions

        /// <inheritdoc />
        public async Task<StateTransition> GetStateTransitionById(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateTransitionById started. (StateTransitionId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbStateTransitions = await dbContext.GetStateTransitionById(id, token).ConfigureAwait(false);
                    var dbStateTransition = dbStateTransitions.FirstOrDefault();
                    if (dbStateTransition == null)
                    {
                        var message = $"StateTransition doesn't exist in the Database. (StateTransitionId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapStateTransitionById(dbStateTransition);
                }
            }
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistory(string environmentName, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateTransitionHistory started. (Environment: '{environmentName}')");
                var stateTransitionHistory = new Dictionary<string, List<StateTransition>>();
                using (var dbContext = GetContext())
                {
                    var dbStateTransitions = new List<StateTransition>();
                    var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);
                    var dbHistory = await dbContext.GetStateTransitionHistory(dbEnvironment.Id, startDate, endDate, token).ConfigureAwait(false);
                    dbStateTransitions.AddRange(dbHistory.Select(ProvidenceModelMapper.MapStateTransitionHistory).ToList());

                    var initialStates = await dbContext.GetStates(dbEnvironment.Id, startDate, token).ConfigureAwait(false);
                    var mappedInitialStates = initialStates.Select(ProvidenceModelMapper.MapStatesReturnModel).ToList();
                    dbStateTransitions.AddRange(mappedInitialStates.Select(ProvidenceModelMapper.MapStateTransitionHistory));

                    // Create StateTransitionGroups used by Frontend
                    var filteredTransitionsGroups = dbStateTransitions.GroupBy(s => $"{s.ElementId}###{s.CheckId}###{s.AlertName}", StringComparer.OrdinalIgnoreCase);
                    var charsToTrim = new[] { '#' };
                    foreach (var filteredTransitionsGroup in filteredTransitionsGroups)
                    {
                        var key = filteredTransitionsGroup.Key.TrimStart(charsToTrim).TrimEnd(charsToTrim);
                        var value = filteredTransitionsGroup.OrderBy(s => s.SourceTimestamp).ToList();
                        stateTransitionHistory.Add(key, value);
                    }
                    return stateTransitionHistory;
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<StateTransition>> GetStateTransitionHistoryByElementId(string environmentName, string elementId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateTransitionHistory started. (Environment: '{environmentName}', ElementId: '{elementId}')");
                var history = new List<StateTransition>();
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentByName(environmentName, dbContext).ConfigureAwait(false);
                    var environmentElements = await dbContext.GetAllElementsWithEnvironmentId(dbEnvironment.Id, token).ConfigureAwait(false);
                    var element = environmentElements.FirstOrDefault(x => x.ElementId.Equals(elementId, StringComparison.OrdinalIgnoreCase));
                    if (element == null)
                    {
                        var message = $"Element doesn't exist in the Database. (Environment: '{environmentName}', ElementId: '{elementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Get all StateTransitions for a specific Element
                    var dbStateTransitions = await dbContext.GetStateTransitionHistoryByElementId(elementId, dbEnvironment.Id, startDate, endDate, token).ConfigureAwait(false);

                    // Sort all the StateTransitions by their RecordId/Guid
                    var stateTransitionsByGuid = new Dictionary<string, List<DB.GetStateTransitionHistoryByElementIdReturnModel>>();

                    foreach (var dbStateTransition in dbStateTransitions)
                    {
                        if (stateTransitionsByGuid.TryGetValue(dbStateTransition.Guid, out var entry))
                        {
                            entry.Add(dbStateTransition);
                        }
                        else
                        {
                            stateTransitionsByGuid.Add(dbStateTransition.Guid, new List<DB.GetStateTransitionHistoryByElementIdReturnModel> { dbStateTransition });
                        }
                    }

                    // Filter all StateTransitions for the same RecordId/Guid -> we only need one entry per RecordId/Guid
                    var filteredStateTransitions = new List<DB.GetStateTransitionHistoryByElementIdReturnModel>();
                    foreach (var guid in stateTransitionsByGuid)
                    {
                        var stateTransitions = guid.Value.OrderBy(s => s.SourceTimestamp).ToList();

                        // Check if there is a StateTransition with an AlertName
                        if (stateTransitions.Any(s => !string.IsNullOrEmpty(s.AlertName)))
                        {
                            filteredStateTransitions.Add(stateTransitions.Last(s => !string.IsNullOrEmpty(s.AlertName)));
                        }
                        // Check if there is a StateTransition with a CheckId
                        else if (stateTransitions.Any(s => !string.IsNullOrEmpty(s.CheckId)))
                        {
                            filteredStateTransitions.Add(stateTransitions.Last(s => !string.IsNullOrEmpty(s.CheckId)));
                        }
                        else
                        {
                            filteredStateTransitions.Add(stateTransitions.Last());
                        }
                    }

                    history.AddRange(filteredStateTransitions.Select(ProvidenceModelMapper.MapStateTransitionHistoryByElementId).ToList());
                    // Add the initial State for the Element
                    var initialStates = await dbContext.GetInitialStateByElementId(elementId, dbEnvironment.Id, startDate, token).ConfigureAwait(false);
                    history.AddRange(initialStates.Select(ProvidenceModelMapper.MapInitialStateForElementIdReturnModel).ToList());
                    // Also remove duplicate entries before returning result
                    return history.Distinct().ToList();
                }
            }
        }

        #endregion

        #region Recurring Deployments

        /// <inheritdoc />
        public async Task<List<GetDeployment>> AddRecurringDeployment(PostDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddRecurringDeployments started. (Environment: '{parentDeployment.EnvironmentSubscriptionId})");
                using (var dbContext = GetContext())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var deployments = new List<GetDeployment>();

                            // Create parent deployment
                            var dbDeployment = await AddDeployment(parentDeployment, token).ConfigureAwait(false);
                            deployments.Add(dbDeployment);

                            // Create child deployments
                            var recurringDeployments = await AddRecurringDeployments(dbDeployment.Id, childDeployments, dbContext, token).ConfigureAwait(false);
                            deployments.AddRange(recurringDeployments);

                            transaction.Commit();
                            return deployments;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetDeployment>> UpdateRecurringDeployment(int parentId, PutDeployment parentDeployment, IList<PostDeployment> childDeployments, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateRecurringDeployment started. (DeploymentId: '{parentId}')");
                using (var dbContext = GetContext())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var deployments = new List<GetDeployment>();

                            // Update existing parent deployment
                            var dbDeployment = await UpdateDeployment(parentId, parentDeployment, token).ConfigureAwait(false);
                            deployments.Add(dbDeployment);

                            // Create new child deployments
                            var recurringDeployments = await AddRecurringDeployments(parentId, childDeployments, dbContext, token).ConfigureAwait(false);
                            deployments.AddRange(recurringDeployments);

                            transaction.Commit();
                            return deployments;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        #endregion

        #region Alert Ignore Rules

        /// <inheritdoc />
        public async Task<GetAlertIgnoreRule> GetAlertIgnoreRule(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAlertIgnoreRule started. (AlertIgnoreId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbAlertIgnore = await dbContext.AlertIgnores.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertIgnore == null)
                    {
                        var message = $"AlertIgnoreRule doesn't exist in the Database. (AlertIgnoreId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, dbAlertIgnore);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetAlertIgnoreRule>> GetAlertIgnoreRules(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "GetAlertIgnoreRules started.");
                using (var dbContext = GetContext())
                {
                    var alertIgnoreRules = new ConcurrentBag<GetAlertIgnoreRule>();
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbAlertIgnores = await dbContext.AlertIgnores.ToListAsync(token).ConfigureAwait(false);
                    dbAlertIgnores.ForEach(a => alertIgnoreRules.Add(ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, a)));
                    return alertIgnoreRules.ToList();
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetAlertIgnoreRule> AddAlertIgnoreRule(PostAlertIgnoreRule alertIgnoreRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddAlertIgnoreRule started. (Environment: '{alertIgnoreRule.EnvironmentSubscriptionId})");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(alertIgnoreRule.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var newAlertIgnoreRule = ProvidenceModelMapper.MapMdAlertIgnoreToDbAlertIgnore(alertIgnoreRule);

                    dbContext.AlertIgnores.Add(newAlertIgnoreRule);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbAlertIgnoreRule = await dbContext.AlertIgnores.FirstOrDefaultAsync(a => a.Id == newAlertIgnoreRule.Id, token).ConfigureAwait(false);
                    if (newDbAlertIgnoreRule == null)
                    {
                        var message = $"AlertIgnoreRule could not be created. (Environment: '{dbEnvironment.ElementId})";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, newDbAlertIgnoreRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newDbAlertIgnoreRule.Id, ChangeElementType.AlertIgnore, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, newDbAlertIgnoreRule);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateAlertIgnoreRule(int id, PutAlertIgnoreRule alertIgnoreRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateAlertIgnoreRule started. (AlertIgnoreId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(alertIgnoreRule.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbAlertIgnoreRule = await dbContext.AlertIgnores.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertIgnoreRule == null)
                    {
                        var message = $"AlertIgnoreRule doesn't exist in the Database. (AlertIgnoreId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, dbAlertIgnoreRule));
                    if (dbAlertIgnoreRule.CreationDate > alertIgnoreRule.ExpirationDate)
                    {
                        var message = $"AlertIgnoreRule could not be updated because CreationDate > ExpirationDate. (AlertIgnoreId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.BadRequest);
                    }

                    dbAlertIgnoreRule.Name = alertIgnoreRule.Name;
                    dbAlertIgnoreRule.EnvironmentSubscriptionId = dbEnvironment.ElementId;
                    dbAlertIgnoreRule.ExpirationDate = alertIgnoreRule.ExpirationDate;
                    dbAlertIgnoreRule.IgnoreCondition = JsonConvert.SerializeObject(alertIgnoreRule.IgnoreCondition);

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, dbAlertIgnoreRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Update, dbAlertIgnoreRule.Id, ChangeElementType.AlertIgnore, dbEnvironment, oldValue, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteAlertIgnoreRule(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteAlertIgnoreRule started. (AlertIgnoreId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbAlertIgnoreRule = await dbContext.AlertIgnores.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertIgnoreRule == null)
                    {
                        var message = $"AlertIgnore doesn't exist in the Database. (AlertIgnoreId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbAlertIgnoreRuleToMdAlertIgnoreRule(dbEnvironments, dbAlertIgnoreRule));

                    dbContext.AlertIgnores.Remove(dbAlertIgnoreRule);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, id, ChangeElementType.AlertIgnore, await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId == dbAlertIgnoreRule.EnvironmentSubscriptionId, token).ConfigureAwait(false), oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Import / Export

        /// <inheritdoc />
        public async Task<Environment> GetExportEnvironment(string environmentSubscriptionId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetExportEnvironment started. (Environment: '{environmentSubscriptionId}')");
                var exportEnvironment = new Environment();
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Get Services
                    var exportServices = new List<Models.ImportExport.Service>();
                    var dbServices = await dbContext.Services.Where(s => s.EnvironmentId == dbEnvironment.Id).Include(s => s.Actions).ToListAsync(token).ConfigureAwait(false);
                    dbServices.ForEach(dbService =>
                    {
                        exportServices.Add(new Models.ImportExport.Service
                        {
                            Name = dbService.Name,
                            Description = dbService.Description,
                            ElementId = dbService.ElementId,
                            Actions = dbService.Actions.OrderBy(a => a.ElementId).Select(a => a.ElementId).ToList()
                        });
                    });
                    exportEnvironment.Services = exportServices;

                    // Get Actions
                    var exportActions = new List<Action>();
                    var dbActions = await dbContext.Actions.Where(a => a.EnvironmentId == dbEnvironment.Id).Include(s => s.Components).ToListAsync(token).ConfigureAwait(false);
                    dbActions.ForEach(dbAction =>
                    {
                        exportActions.Add(new Action
                        {
                            Name = dbAction.Name,
                            Description = dbAction.Description,
                            ElementId = dbAction.ElementId,
                            Components = dbAction.Components.OrderBy(c => c.ElementId).Select(c => c.ElementId).ToList()
                        });
                    });
                    exportEnvironment.Actions = exportActions;

                    // Get Components
                    var exportComponents = new List<Component>();
                    var dbComponents = await dbContext.Components.Where(c => c.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    dbComponents.ForEach(dbComponent =>
                    {
                        exportComponents.Add(new Component
                        {
                            Name = dbComponent.Name,
                            Description = dbComponent.Description,
                            ElementId = dbComponent.ElementId,
                            ComponentType = dbComponent.ComponentType
                        });
                    });
                    exportEnvironment.Components = exportComponents;

                    // Get Checks
                    var exportChecks = new List<Check>();
                    var dbChecks = await dbContext.Checks.Where(c => c.EnvironmentId == dbEnvironment.Id).ToListAsync(token).ConfigureAwait(false);
                    dbChecks.ForEach(dbCheck =>
                    {
                        exportChecks.Add(new Check
                        {
                            Name = dbCheck.Name,
                            Description = dbCheck.Description,
                            ElementId = dbCheck.ElementId,
                            VstsLink = dbCheck.VstsStory,
                            Frequency = dbCheck.Frequency ?? 0
                        });
                    });
                    exportEnvironment.Checks = exportChecks;
                }
                return exportEnvironment;
            }
        }

        /// <inheritdoc />
        public async Task AddSyncElementsToEnvironment(Environment environment, Dictionary<string, DateTime> creationDates, string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddSyncElementsToEnvironment started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Create all Components defined in the Environment
                    var dbComponents = new List<DB.Component>();
                    foreach (var component in environment.Components)
                    {
                        var dbComponent = new DB.Component
                        {
                            Name = component.Name,
                            Description = component.Description,
                            ElementId = component.ElementId,
                            ComponentType = component.ComponentType,
                            EnvironmentId = dbEnvironment.Id
                        };
                        dbComponent.CreateDate = creationDates.ContainsKey(dbComponent.ElementId) ? creationDates[dbComponent.ElementId] : DateTime.UtcNow;
                        dbComponents.Add(dbComponent);
                    }

                    // Create all Actions defined in the Environment
                    var dbActions = new List<DB.Action>();
                    foreach (var action in environment.Actions)
                    {
                        // Create all Components defined in the Action
                        foreach (var component in action.Components)
                        {
                            if (!dbComponents.Any(c => c.ElementId.Equals(component, StringComparison.OrdinalIgnoreCase)))
                            {
                                var dbComponent = new DB.Component
                                {
                                    Name = component,
                                    Description = null,
                                    ElementId = component,
                                    ComponentType = "Component",
                                    EnvironmentId = dbEnvironment.Id
                                };
                                dbComponent.CreateDate = creationDates.ContainsKey(dbComponent.ElementId) ? creationDates[dbComponent.ElementId] : DateTime.UtcNow;
                                dbComponents.Add(dbComponent);
                            }
                        }

                        var dbAction = new DB.Action
                        {
                            Name = action.Name,
                            Description = action.Description,
                            ElementId = action.ElementId,
                            EnvironmentId = dbEnvironment.Id,
                            Components = dbComponents.Where(c => action.Components.Any(ac => ac.Equals(c.ElementId, StringComparison.OrdinalIgnoreCase)) && c.EnvironmentId == dbEnvironment.Id).ToList()
                        };
                        dbAction.CreateDate = creationDates.ContainsKey(dbAction.ElementId) ? creationDates[dbAction.ElementId] : DateTime.UtcNow;
                        dbActions.Add(dbAction);
                    }

                    // Create all Services defined in the Environment
                    var dbServices = new List<DB.Service>();
                    foreach (var service in environment.Services)
                    {
                        var dbService = new DB.Service
                        {
                            Name = service.Name,
                            Description = service.Description,
                            ElementId = service.ElementId,
                            EnvironmentRef = dbEnvironment.Id,
                            EnvironmentId = dbEnvironment.Id,
                            Actions = dbActions.Where(a => service.Actions.Any(sa => sa.Equals(a.ElementId, StringComparison.OrdinalIgnoreCase)) && a.EnvironmentId == dbEnvironment.Id).ToList()
                        };
                        dbService.CreateDate = creationDates.ContainsKey(dbService.ElementId) ? creationDates[dbService.ElementId] : DateTime.UtcNow;
                        dbServices.Add(dbService);
                    }

                    // Create all Checks defined in the Environment
                    var dbChecks = new List<DB.Check>();
                    foreach (var check in environment.Checks)
                    {
                        var dbCheck = new DB.Check
                        {
                            ElementId = check.ElementId,
                            Description = check.Description,
                            Frequency = check.Frequency,
                            Name = check.Name,
                            VstsStory = check.VstsLink,
                            EnvironmentId = dbEnvironment.Id
                        };
                        dbChecks.Add(dbCheck);
                    }
                    dbContext.Components.AddRange(dbComponents);
                    dbContext.Actions.AddRange(dbActions);
                    dbContext.Services.AddRange(dbServices);
                    dbContext.Checks.AddRange(dbChecks);

                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateSyncElementsInEnvironment(Environment environment, string environmentSubscriptionId, ReplaceFlag replace) //TODO: Add CancelationToken
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateSyncElementsInEnvironment started. (Environment: '{environmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);

                    // Update / Replace Checks
                    foreach (var check in environment.Checks)
                    {
                        var dbCheck = await dbContext.Checks.FirstOrDefaultAsync(c => c.ElementId.ToLower() == check.ElementId.ToLower() && c.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                        if (dbCheck != null)
                        {
                            // Update if exists
                            dbCheck.Description = check.Description;
                            dbCheck.Frequency = check.Frequency;
                            dbCheck.VstsStory = check.VstsLink;
                            dbCheck.Name = check.Name;
                        }
                        else
                        {
                            var newDbCheck = new DB.Check
                            {
                                ElementId = check.ElementId,
                                Description = check.Description,
                                Frequency = check.Frequency,
                                Name = check.Name,
                                VstsStory = check.VstsLink,
                                EnvironmentId = dbEnvironment.Id
                            };
                            dbContext.Checks.Add(newDbCheck);
                        }
                    }

                    // Update / Replace Components
                    foreach (var component in environment.Components)
                    {
                        var dbComponent = await dbContext.Components.FirstOrDefaultAsync(c => c.ElementId.ToLower() == component.ElementId.ToLower() && c.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                        if (dbComponent != null)
                        {
                            dbComponent.Name = component.Name;
                            dbComponent.Description = component.Description;
                            dbComponent.ComponentType = component.ComponentType;
                        }
                        else
                        {
                            var newDbComponent = new DB.Component
                            {
                                Name = component.Name,
                                Description = component.Description,
                                ElementId = component.ElementId,
                                ComponentType = component.ComponentType,
                                EnvironmentId = dbEnvironment.Id,
                                CreateDate = DateTime.UtcNow
                            };
                            dbContext.Components.Add(newDbComponent);
                        }
                    }
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);

                    // Update / Replace Actions
                    var dbComponents = await dbContext.Components.Where(s => s.EnvironmentId == dbEnvironment.Id).ToListAsync().ConfigureAwait(false);
                    foreach (var action in environment.Actions)
                    {
                        // Check all Component->Action assignments
                        foreach (var component in action.Components)
                        {
                            var dbComponent = dbComponents.FirstOrDefault(c => c.ElementId.ToLower() == component.ToLower() && c.EnvironmentId == dbEnvironment.Id);
                            if (dbComponent == null)
                            {
                                var newDbComponent = new DB.Component
                                {
                                    Name = component,
                                    Description = null,
                                    ElementId = component,
                                    ComponentType = "Component",
                                    EnvironmentId = dbEnvironment.Id,
                                    CreateDate = DateTime.UtcNow
                                };
                                dbContext.Components.Add(newDbComponent);
                                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }

                        // Update the dbComponents because new one might be added in the previous step
                        dbComponents = await dbContext.Components.Where(c => c.EnvironmentId == dbEnvironment.Id).ToListAsync().ConfigureAwait(false);

                        var newActionComponents = dbComponents.Where(c => action.Components.Any(ac => ac.ToLower() == c.ElementId.ToLower())).ToList();
                        var dbAction = await dbContext.Actions.Include(a => a.Components).FirstOrDefaultAsync(a => a.ElementId.ToLower() == action.ElementId.ToLower() && a.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                        if (dbAction != null)
                        {
                            dbAction.Name = action.Name;
                            dbAction.Description = action.Description;
                            if (replace == ReplaceFlag.False)
                            {
                                foreach (var component in newActionComponents)
                                {
                                    if (!dbAction.Components.Any(c => c.ElementId.Equals(component.ElementId, StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        dbAction.Components.Add(component);
                                    }
                                }
                            }
                            else if (replace == ReplaceFlag.True)
                            {
                                dbAction.Components = newActionComponents;
                            }
                        }
                        else
                        {
                            var newDbAction = new DB.Action
                            {
                                Name = action.Name,
                                Description = action.Description,
                                EnvironmentId = dbEnvironment.Id,
                                ElementId = action.ElementId,
                                Components = newActionComponents,
                                CreateDate = DateTime.UtcNow
                            };
                            dbContext.Actions.Add(newDbAction);
                        }
                    }
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);

                    // Update / Replace Services
                    var dbActions = await dbContext.Actions.Where(s => s.EnvironmentId == dbEnvironment.Id).ToListAsync().ConfigureAwait(false);
                    foreach (var service in environment.Services)
                    {
                        var newServiceActions = dbActions.Where(a => service.Actions.Any(sa => sa.ToLower() == a.ElementId.ToLower())).ToList();
                        var dbService = await dbContext.Services.Include(a => a.Actions).FirstOrDefaultAsync(s => s.ElementId.ToLower() == service.ElementId.ToLower() && s.EnvironmentId == dbEnvironment.Id).ConfigureAwait(false);
                        if (dbService != null)
                        {
                            dbService.Name = service.Name;
                            dbService.Description = service.Description;
                            if (replace == ReplaceFlag.False)
                            {
                                foreach (var action in newServiceActions)
                                {
                                    if (!dbService.Actions.Any(a => a.ElementId.Equals(action.ElementId, StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        dbService.Actions.Add(action);
                                    }
                                }
                            }
                            else if (replace == ReplaceFlag.True)
                            {
                                var oldServiceActions = dbService.Actions;
                                dbService.Actions = newServiceActions;

                                // Delete Action which are no longer assigned to the Service
                                foreach (var action in oldServiceActions)
                                {
                                    if (!newServiceActions.Any(a => a.ElementId.Equals(action.ElementId, StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        var dbAction = dbActions.FirstOrDefault(a => a.ElementId.ToLower() == action.ElementId.ToLower() && a.EnvironmentId == dbEnvironment.Id);
                                        if (dbAction != null)
                                        {
                                            dbContext.Actions.Remove(dbAction);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var newDbService = new DB.Service
                            {
                                Name = service.Name,
                                Description = service.Description,
                                EnvironmentId = dbEnvironment.Id,
                                EnvironmentRef = dbEnvironment.Id,
                                ElementId = service.ElementId,
                                Actions = newServiceActions,
                                CreateDate = DateTime.UtcNow
                            };
                            dbContext.Services.Add(newDbService);
                        }
                    }
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Notification Rules

        /// <inheritdoc />
        public async Task<List<GetNotificationRule>> GetNotificationRulesAsync(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                var notificationRules = new ConcurrentBag<GetNotificationRule>();
                AILogger.Log(SeverityLevel.Information, "GetNotificationRulesAsync started.");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbNotificationRules = await dbContext.NotificationConfigurations.Include(n => n.ComponentTypes).Include(n => n.States).ToListAsync(token).ConfigureAwait(false);
                    dbNotificationRules.ForEach(n => notificationRules.Add(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, n)));
                }
                return notificationRules.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetNotificationRule> GetNotificationRuleAsync(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetNotificationRuleAsync started. (NotificationRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbNotificationRule = await dbContext.NotificationConfigurations.Include(n => n.ComponentTypes).Include(n => n.States).FirstOrDefaultAsync(n => n.Id == id, token).ConfigureAwait(false);
                    if (dbNotificationRule == null)
                    {
                        var message = $"NotificationRule doesn't exist in the Database. (NotificationRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var notificationRule = ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, dbNotificationRule);
                    return notificationRule;
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetNotificationRule> AddNotificationRule(PostNotificationRule notificationRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddNotificationRule started. (Environment: '{notificationRule.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var states = await dbContext.States.ToListAsync(token).ConfigureAwait(false);
                    var types = await dbContext.ComponentTypes.ToListAsync(token).ConfigureAwait(false);

                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(notificationRule.EnvironmentSubscriptionId, dbContext);

                    var newNotificationRule = ProvidenceModelMapper.MapMdNotificationRuleToDbNotificationRule(dbEnvironments, states, types, notificationRule);

                    dbContext.NotificationConfigurations.Add(newNotificationRule);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbNotificationRule = await dbContext.NotificationConfigurations.FirstOrDefaultAsync(n => n.Id == newNotificationRule.Id, token).ConfigureAwait(false);
                    if (newDbNotificationRule == null)
                    {
                        var message = $"NotificationRule could not be created. (Environment: '{dbEnvironment.ElementId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, newDbNotificationRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newDbNotificationRule.Id, ChangeElementType.NotificationRule, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, newDbNotificationRule);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateNotificationRule(int id, PutNotificationRule notificationRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateNotificationRule started. (NotificationRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(notificationRule.EnvironmentSubscriptionId, dbContext);

                    var dbNotificationRule = await dbContext.NotificationConfigurations.Include(n => n.ComponentTypes).Include(n => n.States).FirstOrDefaultAsync(n => n.Id == id, token).ConfigureAwait(false);
                    if (dbNotificationRule == null)
                    {
                        var message = $"NotificationRule doesn't exist in the Database. (NotificationRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, dbNotificationRule));

                    var dbComponentTypes = await dbContext.ComponentTypes.ToListAsync(token).ConfigureAwait(false);
                    var dbStates = await dbContext.States.ToListAsync(token).ConfigureAwait(false);

                    dbNotificationRule.EmailAddresses = notificationRule.EmailAddresses;
                    dbNotificationRule.ComponentTypes = dbComponentTypes.Where(c => notificationRule.Levels.Any(l => l.ToLower() == c.Name.ToLower())).ToList();
                    dbNotificationRule.Environment = dbEnvironment.Id;
                    dbNotificationRule.IsActive = notificationRule.IsActive;
                    dbNotificationRule.States = dbStates.Where(s => notificationRule.States.Any(nrs => nrs.ToLower() == s.Name.ToLower())).ToList();
                    dbNotificationRule.NotificationInterval = notificationRule.NotificationInterval;

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, dbNotificationRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Update, id, ChangeElementType.NotificationRule, dbEnvironment, oldValue, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task<string> DeleteNotificationRule(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteNotificationRule started. (NotificationRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbNotificationRule = await dbContext.NotificationConfigurations.FirstOrDefaultAsync(n => n.Id == id, token).ConfigureAwait(false);
                    if (dbNotificationRule == null)
                    {
                        var message = $"NotificationRule doesn't exist in the Database. (NotificationRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Get the Environment the Rule was created for
                    var environmentSubscriptionId = (await dbContext.Environments.FirstOrDefaultAsync(e => e.Id == dbNotificationRule.Environment, token))?.ElementId;

                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbNotificationRuleToMdNotificationRule(dbEnvironments, dbNotificationRule));
                    dbContext.NotificationConfigurations.Remove(dbNotificationRule);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, id, ChangeElementType.NotificationRule, await dbContext.Environments.FirstOrDefaultAsync(e => e.Id == dbNotificationRule.Environment, token).ConfigureAwait(false), oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return environmentSubscriptionId;
                }
            }
        }

        #endregion

        #region State Increase Rules

        /// <inheritdoc />
        public async Task<GetStateIncreaseRule> GetStateIncreaseRule(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateIncreaseRule started. (StateIncreaseRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbStateIncreaseRule = await dbContext.StateIncreaseRules.FirstOrDefaultAsync(s => s.Id == id, token).ConfigureAwait(false);
                    if (dbStateIncreaseRule == null)
                    {
                        var message = $"StateIncreaseRule doesn't exist in the Database. (StateIncreaseRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var stateIncreaseRule = ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, dbStateIncreaseRule);
                    return stateIncreaseRule;
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetStateIncreaseRule>> GetStateIncreaseRules(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "GetStateIncreaseRules started.");
                var stateIncreaseRules = new ConcurrentBag<GetStateIncreaseRule>();
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbStateIncreaseRules = await dbContext.StateIncreaseRules.ToListAsync(token).ConfigureAwait(false);
                    dbStateIncreaseRules.ForEach(s => stateIncreaseRules.Add(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, s)));
                }
                return stateIncreaseRules.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<GetStateIncreaseRule> AddStateIncreaseRule(PostStateIncreaseRule stateIncreaseRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddStateIncreaseRule started. (Environment: '{stateIncreaseRule.EnvironmentSubscriptionId}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(stateIncreaseRule.EnvironmentSubscriptionId, dbContext);

                    var newStateIncreaseRule = ProvidenceModelMapper.MapMdStateIncreaseRuleToDbStateIncreaseRule(stateIncreaseRule);

                    dbContext.StateIncreaseRules.Add(newStateIncreaseRule);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbStateIncreaseRule = await dbContext.StateIncreaseRules.FirstOrDefaultAsync(n => n.Id == newStateIncreaseRule.Id, token).ConfigureAwait(false);
                    if (newDbStateIncreaseRule == null)
                    {
                        var message = $"NotificationRule could not be created. (Environment: '{stateIncreaseRule.EnvironmentSubscriptionId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, newDbStateIncreaseRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Add, newStateIncreaseRule.Id, ChangeElementType.IncreaseRule, dbEnvironment, JsonUtils.Empty, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, newDbStateIncreaseRule);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateStateIncreaseRule(int id, PutStateIncreaseRule stateIncreaseRule, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateStateIncreaseRule started. (StateIncreaseRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(stateIncreaseRule?.EnvironmentSubscriptionId, dbContext).ConfigureAwait(false);
                    var dbStateIncreaseRule = await dbContext.StateIncreaseRules.FirstOrDefaultAsync(s => s.Id == id, token).ConfigureAwait(false);
                    if (stateIncreaseRule == null || dbStateIncreaseRule == null)
                    {
                        var message = $"StateIncreaseRule doesn't exist in the Database. (StateIncreaseRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, dbStateIncreaseRule));

                    dbStateIncreaseRule.Name = stateIncreaseRule.Name;
                    dbStateIncreaseRule.Description = stateIncreaseRule.Description;
                    dbStateIncreaseRule.EnvironmentSubscriptionId = dbEnvironment.ElementId;
                    dbStateIncreaseRule.CheckId = stateIncreaseRule.CheckId;
                    dbStateIncreaseRule.AlertName = stateIncreaseRule.AlertName;
                    dbStateIncreaseRule.ComponentId = stateIncreaseRule.ComponentId;
                    dbStateIncreaseRule.TriggerTime = stateIncreaseRule.TriggerTime;
                    dbStateIncreaseRule.IsActive = stateIncreaseRule.IsActive;

                    // Create ChangeLog Entry
                    var newValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, dbStateIncreaseRule));
                    var changeLog = CreateChangeLog(ChangeOperation.Update, dbStateIncreaseRule.Id, ChangeElementType.IncreaseRule, dbEnvironment, oldValue, newValue);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteStateIncreaseRule(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteStateIncreaseRule started. (StateIncreaseRuleId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
                    var dbStateIncreaseRule = await dbContext.StateIncreaseRules.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbStateIncreaseRule == null)
                    {
                        var message = $"StateIncreaseRule doesn't exist in the Database. (StateIncreaseRuleId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    var oldValue = JsonConvert.SerializeObject(ProvidenceModelMapper.MapDbStateIncreaseRuleToMdStateIncreaseRule(dbEnvironments, dbStateIncreaseRule));
                    dbContext.StateIncreaseRules.Remove(dbStateIncreaseRule);

                    // Create ChangeLog Entry
                    var changeLog = CreateChangeLog(ChangeOperation.Delete, dbStateIncreaseRule.Id, ChangeElementType.IncreaseRule, await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId.ToLower() == dbStateIncreaseRule.EnvironmentSubscriptionId.ToLower(), token).ConfigureAwait(false), oldValue, JsonUtils.Empty);
                    dbContext.Changelogs.Add(changeLog);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Alert Comments

        /// <inheritdoc />
        public async Task<GetAlertComment> GetAlertComment(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAlertComment started. (AlertCommentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbAlertComment = await dbContext.AlertComments.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertComment == null)
                    {
                        var message = $"AlertComment doesn't exist in the Database. (AlertCommentId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    return ProvidenceModelMapper.MapDbAlertCommentToMdAlertComment(dbAlertComment);
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<GetAlertComment>> GetAlertComments(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "GetAlertComments started.");
                var alertComments = new ConcurrentBag<GetAlertComment>();
                using (var dbContext = GetContext())
                {
                    await dbContext.AlertComments.ForEachAsync(a => alertComments.Add(ProvidenceModelMapper.MapDbAlertCommentToMdAlertComment(a)), token).ConfigureAwait(false);
                }
                return alertComments.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<List<GetAlertComment>> GetAlertCommentsByRecordId(string recordId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetAlertCommentsByRecordId started. (RecordId: '{recordId}')");
                using (var dbContext = GetContext())
                {
                    var alertComments = new List<GetAlertComment>();

                    // Get all StateTransitions for the RecordId and select the one with the lowest Id
                    var dbStateTransitions = await dbContext.StateTransitions.Where(st => st.Guid.ToLower() == recordId.ToLower()).ToListAsync(token).ConfigureAwait(false);
                    if (!dbStateTransitions.Any())
                    {
                        var message = $"StateTransitions doesn't exist in the Database. (RecordId: '{recordId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    var smallestStateTransitionId = dbStateTransitions.Min(st => st.Id);

                    // Get all AlertComments for the StateTransitionId
                    await dbContext.AlertComments.Where(a => a.StateTransitionId == smallestStateTransitionId).ForEachAsync(n => alertComments.Add(ProvidenceModelMapper.MapDbAlertCommentToMdAlertComment(n)), token).ConfigureAwait(false);
                    return alertComments;
                }
            }
        }

        /// <inheritdoc />
        public async Task<GetAlertComment> AddAlertComment(PostAlertComment alertComment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddAlertComment started. (RecordId: '{alertComment.RecordId}')");
                using (var dbContext = GetContext())
                {
                    string message;
                    var newAlertComment = ProvidenceModelMapper.MapMdAlertCommentToDbAlertComment(alertComment);

                    // Get all StateTransitions for the RecordId
                    var dbStateTransitions = await dbContext.StateTransitions.Where(st => st.Guid.ToLower() == alertComment.RecordId.ToLower()).ToListAsync(token).ConfigureAwait(false);
                    if (!dbStateTransitions.Any())
                    {
                        message = $"StateTransitions doesn't exist in the Database. (RecordId: '{alertComment.RecordId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Update the StateTransitions 
                    foreach (var dbStateTransition in dbStateTransitions)
                    {
                        dbStateTransition.ProgressState = newAlertComment.State;
                    }

                    // Set StateTransitionId of the new Comment -> it always the lowest of all StateTransitionIds
                    var dbStateTransitionId = dbStateTransitions.Min(st => st.Id);
                    newAlertComment.StateTransitionId = dbStateTransitionId;

                    dbContext.AlertComments.Add(newAlertComment);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbAlertComment = await dbContext.AlertComments.FirstOrDefaultAsync(a => a.Id == newAlertComment.Id, token).ConfigureAwait(false);
                    if (newDbAlertComment == null)
                    {
                        message = $"AlertComment could not be created. (RecordId: '{alertComment.RecordId}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                    }
                    return ProvidenceModelMapper.MapDbAlertCommentToMdAlertComment(newDbAlertComment);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdateAlertComment(int id, PutAlertComment alertComment, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateAlertComment started. (AlertCommentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbAlertComment = await dbContext.AlertComments.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertComment == null)
                    {
                        var message = $"AlertComment doesn't exist in the Database. (AlertCommentId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Update all affected StateTransitions
                    var dbStateTransitionMentionedInAlertComment = await dbContext.StateTransitions.FirstOrDefaultAsync(st => st.Id == dbAlertComment.StateTransitionId, token).ConfigureAwait(false);
                    var dbStateTransitions = await dbContext.StateTransitions.Where(st => st.Guid.ToLower() == dbStateTransitionMentionedInAlertComment.Guid.ToLower()).ToListAsync(token).ConfigureAwait(false);
                    foreach (var dbStateTransition in dbStateTransitions)
                    {
                        dbStateTransition.ProgressState = (int?)alertComment.State;
                    }

                    dbAlertComment.Comment = alertComment.Comment;
                    dbAlertComment.User = alertComment.User;
                    dbAlertComment.State = (int)alertComment.State;
                    dbAlertComment.Timestamp = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteAlertComment(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteAlertComment started. (AlertCommentId: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbAlertComment = await dbContext.AlertComments.FirstOrDefaultAsync(a => a.Id == id, token).ConfigureAwait(false);
                    if (dbAlertComment == null)
                    {
                        var message = $"AlertComment doesn't exist in the Database. (AlertCommentId: '{id}')";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }

                    // Get all affected StateTransitions
                    var dbStateTransitionMentionedInAlertComment = await dbContext.StateTransitions.FirstOrDefaultAsync(st => st.Id == dbAlertComment.StateTransitionId, token).ConfigureAwait(false);
                    var dbStateTransitions = await dbContext.StateTransitions.Where(st => st.Guid.ToLower() == dbStateTransitionMentionedInAlertComment.Guid.ToLower()).ToListAsync(token).ConfigureAwait(false);

                    // Get all comment for the specific StateTransitionId
                    var dbAlertComments = await dbContext.AlertComments.Where(a => a.StateTransitionId == dbAlertComment.StateTransitionId).OrderBy(a => a.Timestamp).ToListAsync(token).ConfigureAwait(false);

                    // Check if the newest Comment was deleted: If yes update the stateTransition to the last known progress state
                    if (dbAlertComments.Count() > 1 && dbAlertComments.Last().Equals(dbAlertComment))
                    {
                        foreach (var dbStateTransition in dbStateTransitions)
                        {
                            dbStateTransition.ProgressState = dbAlertComments[dbAlertComments.Count() - 2].State;
                        }
                    }
                    else if (dbAlertComments.Count() <= 1)
                    {
                        foreach (var dbStateTransition in dbStateTransitions)
                        {
                            // No more comments available for the StateTransitionId
                            dbStateTransition.ProgressState = (int)ProgressState.None;
                        }
                    }
                    dbContext.AlertComments.Remove(dbAlertComment);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region SLAs

        /// <inheritdoc />
        public async Task<List<StateTransitionHistory>> GetStateTransitionHistoriesBetweenDates(int environmentId, string elementId, DateTime startDate, DateTime endDate, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetStateTransitionHistories started. (StartDate: '{startDate}', EndDate: '{endDate}', ElementId: '{elementId}', Environment: '{environmentId}')");

                var stateTransitionHistories = new ConcurrentBag<StateTransitionHistory>();
                using (var dbContext = GetContext())
                {
                    var states = await dbContext.States.ToListAsync(token).ConfigureAwait(false);
                    var types = await dbContext.ComponentTypes.ToListAsync(token).ConfigureAwait(false);
                    var dbStateTransitionHistories = await dbContext.StateTransitionHistories       //TODO: Create DB Procedure for this like in StateTransitionHistory
                        .Where(s => s.EnvironmentId == environmentId && s.ElementId.ToLower() == elementId.ToLower())
                        .Where(s =>
                            (startDate <= s.StartDate.Value && s.StartDate.Value <= endDate) ||  //startDate between start and end
                            (startDate <= s.EndDate.Value && s.EndDate.Value <= endDate) ||      //endDate between start and end
                            (s.StartDate <= startDate && endDate <= s.EndDate))                  //startDate before start and endDate after end          
                        .ToListAsync(token).ConfigureAwait(false);
                    dbStateTransitionHistories.ForEach(s => stateTransitionHistories.Add(ProvidenceModelMapper.MapDbStateTransitionHistoryToMdStateTransitionHistory(s, states, types)));
                }
                return stateTransitionHistories.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<DateTime> GetElementCreationDate(int environmentId, string elementId, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetElementCreationDate started. (ElementId: '{elementId}')");
                using (var dbContext = GetContext())
                {
                    var environment = await dbContext.Environments.FirstOrDefaultAsync(a => a.Id == environmentId && a.ElementId.ToLower() == elementId.ToLower(), token);
                    if (environment != null)
                    {
                        return environment.CreateDate;
                    }
                    var action = await dbContext.Actions.FirstOrDefaultAsync(a => a.EnvironmentId == environmentId && a.ElementId.ToLower() == elementId.ToLower(), token);
                    if (action != null)
                    {
                        return action.CreateDate;
                    }
                    var service = await dbContext.Services.FirstOrDefaultAsync(a => a.EnvironmentId == environmentId && a.ElementId.ToLower() == elementId.ToLower(), token);
                    if (service != null)
                    {
                        return service.CreateDate;
                    }
                    var component = await dbContext.Components.FirstOrDefaultAsync(a => a.EnvironmentId == environmentId && a.ElementId.ToLower() == elementId.ToLower(), token);
                    if (component != null)
                    {
                        return component.CreateDate;
                    }
                    // CreationDate for the element not found
                    throw new ProvidenceException($"CreationDate doesn't exist in the Database. (ElementId: '{elementId}')", HttpStatusCode.NotFound);
                }
            }
        }

        #endregion

        /// <inheritdoc />
        public async Task CleanEnvironment(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"CleanEnvironment started. (Environment: '{environmentSubscriptionId})");
                using (var dbContext = GetContext())
                {
                    var dbEnvironment = await GetDatabaseEnvironmentBySubscriptionId(environmentSubscriptionId, dbContext).ConfigureAwait(false);
                    await dbContext.CleanEnvironment(dbEnvironment.Id).ConfigureAwait(false);
                }
            }
        }

        #region Private Methods

        private static async Task<List<DB.Check>> GetEntityChecks(IList<string> checks, int environmentId, DB.MonitoringDB dbContext)
        {
            var entityChecks = new ConcurrentBag<DB.Check>();
            var dbChecks = await dbContext.Checks.Where(c => c.EnvironmentId == environmentId).ToListAsync().ConfigureAwait(false);
            if (checks != null && checks.Any())
            {
                foreach (var check in checks.Distinct())
                {
                    var dbCheck = dbChecks.FirstOrDefault(c => c.ElementId.Equals(check, StringComparison.OrdinalIgnoreCase));
                    if (dbCheck == null)
                    {
                        var message = $"Check doesn't exist in the Database. (ElementId: '{check}' )";
                        AILogger.Log(SeverityLevel.Warning, message);
                        throw new ProvidenceException(message, HttpStatusCode.NotFound);
                    }
                    entityChecks.Add(dbCheck);
                }
            }
            return entityChecks.ToList();
        }

        private static async Task<DB.Environment> GetDatabaseEnvironmentBySubscriptionId(string environmentSubscriptionId, DB.MonitoringDB dbContext)
        {
            AILogger.Log(SeverityLevel.Information, $"GetDatabaseEnvironmentBySubscriptionId started. (Environment: '{environmentSubscriptionId}')");
            var dbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.ElementId.ToLower() == environmentSubscriptionId.ToLower()).ConfigureAwait(false);
            if (dbEnvironment == null)
            {
                var message = $"Environment doesn't exist in the Database. (Environment: '{environmentSubscriptionId}')";
                AILogger.Log(SeverityLevel.Warning, message);
                throw new ProvidenceException(message, HttpStatusCode.NotFound);
            }
            return dbEnvironment;
        }

        private static async Task<DB.Environment> GetDatabaseEnvironmentByName(string environmentName, DB.MonitoringDB dbContext)
        {
            AILogger.Log(SeverityLevel.Information, $"GetDatabaseEnvironmentByName started. (Environment: '{environmentName}')");
            var dbEnvironment = await dbContext.Environments.FirstOrDefaultAsync(e => e.Name.ToLower() == environmentName.ToLower()).ConfigureAwait(false);
            if (dbEnvironment == null)
            {
                var message = $"Environment doesn't exist in the Database. (Environment: '{environmentName}')";
                AILogger.Log(SeverityLevel.Warning, message);
                throw new ProvidenceException(message, HttpStatusCode.NotFound);
            }
            return dbEnvironment;
        }

        private static async Task<List<GetDeployment>> AddRecurringDeployments(int parentId, ICollection<PostDeployment> childDeployments, DB.MonitoringDB dbContext, CancellationToken token)
        {
            AILogger.Log(SeverityLevel.Information, $"AddRecurringDeployments started. (ParentId: '{parentId}', Count: '{childDeployments.Count}')");
            var dbEnvironments = await dbContext.Environments.ToListAsync(token).ConfigureAwait(false);
            foreach (var childDeployment in childDeployments)
            {
                childDeployment.ParentId = parentId;

                var mappedDeployment = ProvidenceModelMapper.MapMdDeploymentToDbDeployment(dbEnvironments, childDeployment);
                dbContext.Deployments.Add(mappedDeployment);
            }
            await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

            var dbDeployments = await dbContext.Deployments.Where(d => d.ParentId == parentId).ToListAsync(token).ConfigureAwait(false);
            if (!dbDeployments.Any())
            {
                var message = $"Creating recurring Deployment failed. (ParentId: '{parentId}', Count: '{childDeployments.Count}')";
                AILogger.Log(SeverityLevel.Warning, message);
                throw new ProvidenceException(message, HttpStatusCode.NotFound);
            }
            var recurringDeployments = new List<GetDeployment>();
            dbDeployments.ForEach(d => recurringDeployments.Add(ProvidenceModelMapper.MapDbDeploymentToMdDeployment(dbEnvironments, d)));
            return recurringDeployments;
        }

        #endregion
    }
}