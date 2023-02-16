using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.StateTransition;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    class MockEnvironmentManager : IEnvironmentManager
    {
        public MockEnvironmentManager()
        {
            ReceivedAlertMessages = new List<AlertMessage>();
        }

        public Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task<Environment> GetEnvironment(string environmentName)
        {
            return null;
        }

        public Task<StateManagerContent> GetStateManagerContent(string environmentName)
        {
            return null;
        }

        public Task<List<Environment>> GetEnvironments()
        {
            return Task.FromResult(new List<Environment>());
        }

        public Task<List<string>> GetEnvironmentNames()
        {
            return Task.FromResult(new List<string>());
        }

        public Task SendTreeUpdate(string environmentName)
        {
            throw new NotImplementedException();
        }

        public Task SendTreeDeletion(string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public StateTransition GetStateTransitionById(int id)
        {
            return new StateTransition();
        }

        public Dictionary<string, List<StateTransition>> GetStateTransitionHistory(string environmentName, string elementId, int numberOfLevels, bool includeChecks, DateTime startDate,
            DateTime endDate, bool allColumns)
        {
            return new Dictionary<string, List<StateTransition>>();
        }

        public Task<List<GetDeployment>> GetDeploymentHistoryAsync(string subscriptionId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeployment> GetLastDeploymentAsync(string subscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeployment> GetDeploymentAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetDeployment>> GetDeploymentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> AddDeploymentAsync(PostDeployment deployment)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDeploymentAsync(PutDeployment deployment, int id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeploymentAsync(int id, string subscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task CheckCurrentAndFutureDeployments()
        {
            throw new NotImplementedException();
        }

        public Task AddFutureDeployments(IList<GetDeployment> deployments)
        {
            throw new NotImplementedException();
        }
        public Task UpdateFutureDeployments(IList<GetDeployment> deployments)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFutureDeployments(int id, string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task CheckEmailNotificationStates()
        {
            throw new NotImplementedException();
        }

        public Task CheckStateIncreaseRules()
        {
            throw new NotImplementedException();
        }

        public Task DeactivateNotificationRule(int id, string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task DeactivateStateIncreaseRule(int id, string environmentSubscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task RefreshEnvironment(string environmentName)
        {
            return Task.CompletedTask;
        }

        public Task RefreshAllEnvironments()
        {
            return Task.CompletedTask;
        }

        public Task ResetEnvironment(string environmentName)
        {
            return Task.CompletedTask;
        }

        public Task ResetAllEnvironments()
        {
            return Task.CompletedTask;
        }

        public Task CreateStateManager(string environmentName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateStateManager(string environmentName)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStateManager(string environmentName)
        {
            throw new NotImplementedException();
        }

        public Task HandleAlerts(AlertMessage[] alertMessages)
        {
            ReceivedAlertMessages.AddRange(alertMessages);
            return Task.CompletedTask;
        }

        public Task CheckHeartbeatAllEnvironments()
        {
            return Task.CompletedTask;
        }

        public Task DisposeEnvironments()
        {
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, List<StateTransition>>> GetStateTransitionHistoryAsync(string environmentName, bool includeChecks, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<StateTransition>> GetStateTransitionHistoryByElementIdAsync(string environmentName, string elementId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public void SaveCachedHistoryToDatabase()
        {
            throw new NotImplementedException();
        }

        public List<AlertMessage> ReceivedAlertMessages { get; set; }
    }
}
