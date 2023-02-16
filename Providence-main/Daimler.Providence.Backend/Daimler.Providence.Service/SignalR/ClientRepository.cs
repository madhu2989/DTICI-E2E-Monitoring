using System;
using System.Collections.Generic;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.SignalR;

namespace Daimler.Providence.Service.SignalR
{
    /// <summary>
    /// Class for holding the ids for SignalR connections.
    /// </summary>
    public sealed class ClientRepository
    {
        #region Private Members

        private IHubContext<DeviceEventHub> _hubContext;
        private readonly List<string> _registeredClients = new List<string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ClientRepository(IHubContext<DeviceEventHub> context)
        {
            _hubContext = context;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method for sending a heartbeat message to the clients.
        /// </summary>
        /// <param name="msg"></param>
        public void SendHeartbeatToRegisteredClients(HeartbeatMsg msg)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "SendHeartbeatToRegisteredClients started.");

                if (msg == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("updateHeartbeat", msg);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method for sending a list of state transitions to the clients.
        /// </summary>
        /// <param name="transitions"></param>
        public void SendStateTransitionsToRegisteredClients(List<StateTransition> transitions)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, "SendStateTransitionsToRegisteredClients started.");

                if (transitions == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("updateStateTransitions", transitions);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method for sending the subscriptionId of an environment which deployment has changed to the clients.
        /// </summary>
        /// <param name="environmentName"></param>
        public void SendDeploymentWindowChangedToRegisteredClients(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendDeploymentWindowChangedToRegisteredClients started. (Environment: '{environmentName}')");

                if (environmentName == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("updateDeploymentWindows", environmentName);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method for sending the tree updates to the clients.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment which was updated.</param>
        public void SendTreeUpdate(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendTreeUpdate started. (Environment: '{environmentName}')");

                if (environmentName == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("updateTree", environmentName);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method for sending the tree deletion to the clients.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the Environment which was deleted.</param>
        public void SendTreeDeletion(string environmentSubscriptionId)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendTreeDeletion started. (Environment: '{environmentSubscriptionId}')");

                if (environmentSubscriptionId == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("deleteTree", environmentSubscriptionId);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method used for notifying the UI about an updated Internal Job.
        /// </summary>
        public void SendInternalJobUpdated(GetInternalJob internalJob)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendInternalJobUpdated started. (Id: '{internalJob.Id}')");

                if (internalJob == null) return;
                foreach (var connectionId in _registeredClients)
                {
                    try
                    {
                        _hubContext.Clients.Client(connectionId).SendAsync("internalJobUpdated", internalJob);
                    }
                    catch (Exception e)
                    {
                        AILogger.Log(SeverityLevel.Error, $"Error occurred on notifying client. (ConnectionId: '{connectionId}')", string.Empty, string.Empty, e);
                    }
                }
            }
        }

        /// <summary>
        /// Method for registering a client.
        /// </summary>
        /// <param name="connectionId"></param>
        public bool RegisterClient(string connectionId)
        {
            AILogger.Log(SeverityLevel.Information, $"RegisterClient started. (ConnectionId: '{connectionId}')");

            // Modify Client Map
            if (_registeredClients.Contains(connectionId)) return false;
            _registeredClients.Add(connectionId);
            return true;
        }

        /// <summary>
        /// Method for unregistering a client.
        /// </summary>
        /// <param name="connectionId"></param>
        public bool UnregisterClient(string connectionId)
        {
            AILogger.Log(SeverityLevel.Information, $"UnRegisterClient started. (ConnectionId: '{connectionId}')");

            // Modify Client Map
            if (!_registeredClients.Contains(connectionId)) return false;
            _registeredClients.Remove(connectionId);
            return true;
        }

        /// <summary>
        /// Method for checking whether a client is registered or not.
        /// </summary>
        /// <param name="connectionId"></param>
        public bool IsRegisteredClient(string connectionId)
        {
            return _registeredClients.Contains(connectionId);
        }

        #endregion
    }
}