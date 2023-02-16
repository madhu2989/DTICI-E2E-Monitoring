using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.SignalR;

namespace Daimler.Providence.Service.SignalR
{
    // [Authorize]
    public class DeviceEventHub : Hub 
    {
        #region Private Members

        private readonly ClientRepository ClientRepository;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public DeviceEventHub(ClientRepository repository)
        {
            ClientRepository = repository;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            RegisterClient();
        }

        /// <inheritdoc />
        public override Task OnDisconnectedAsync(Exception ex)
        {
            // remove client
            UnregisterClient();
            return base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// Method for registering a SignalR Client
        /// </summary>
        public bool RegisterClient()
        {
            return ClientRepository.RegisterClient(Context.ConnectionId);
        }

        /// <summary>
        /// Method for unregistering a SignalR Client
        /// </summary>
        public bool UnregisterClient()
        {
            return ClientRepository.UnregisterClient(Context.ConnectionId);
        }

        /// <summary>
        /// Method for updating the Heartbeats in the UI.
        /// </summary>
        public void SendHeartbeat(HeartbeatMsg msg)
        {
            AILogger.Log(SeverityLevel.Information, "SendHeartbeat started");

            using (new ElapsedTimeLogger())
            {
                //update heartsbeats
                Clients.All.SendAsync("updateHeartbeat", msg);
            }
        }

        /// <summary>
        /// Method for updating the StateTransitions in the UI.
        /// </summary>
        public void SendStateTransitions(List<StateTransition> stateTransitions)
        {
            AILogger.Log(SeverityLevel.Information, "SendStateTransitions started");

            using (new ElapsedTimeLogger())
            {
                //update state transitions
                Clients.All.SendAsync("updateStateTransitions", stateTransitions);
            }
        }

        /// <summary>
        /// Method for updating the Deployment Windows in the UI.
        /// </summary>
        public void SendDeploymentWindows(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendDeploymentWindows started. (Environment: '{environmentName}')");
                //update deployment Windows
                Clients.All.SendAsync("updateDeploymentWindows", environmentName);
            }
        }

        /// <summary>
        /// Method for sending the tree updates to the clients.
        /// </summary>
        /// <param name="environmentName">The unique Name of the Environment which was deleted.</param>
        public void SendTreeUpdate(string environmentName)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"SendTreeUpdate started. (Environment: '{environmentName}')");
                //send notification about a tree update
                Clients.All.SendAsync("updateTree", environmentName);
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
                //send notification about a tree update
                Clients.All.SendAsync("deleteTree", environmentSubscriptionId);
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
                Clients.All.SendAsync("internalJobUpdated", internalJob);
            }
        }

        #endregion
    }
}