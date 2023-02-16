using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    class MockAlertManager : IAlertManager
    {
        public List<AlertMessage> ReceivedAlertMessages { get; set; }

        public MockAlertManager()
        {
            ReceivedAlertMessages = new List<AlertMessage>();
        }

        public Task HandleAlert(AlertMessage alertMessage)
        {
            ReceivedAlertMessages.Add(alertMessage);
            return Task.CompletedTask;
        }

        public Task<List<AlertMessage>> GetQueuedAlertMessagesAsync(string environmentSubscriptionId)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleAlerts(AlertMessage[] alertMessages)
        {
            ReceivedAlertMessages.AddRange(alertMessages);
            return Task.CompletedTask;
        }
    }
}
