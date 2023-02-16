using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Daimler.Providence.Service.EventHub;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Tests.Mocks;
using Azure.Messaging.EventHubs;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daimler.Providence.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EventHubProcessorTest
    {
        [TestMethod]
        public void TestValidMessages()
        {
            var alertManager = new MockAlertManager();
            var messageReceiver = new EventHubMessageReceiver(alertManager);

            var message1 = new AlertMessage
            {
                CheckId = "testCheck1"
            };
            var message2 = new AlertMessage
            {
                CheckId = "testCheck2"
            };

            var eventData1 = new EventData(Encoding.UTF8.GetBytes(message1.ToString()));
            //var eventData2 = new EventData(Encoding.UTF8.GetBytes(message2.ToString()));

            //ProcessEventArgs processEventArgs = new ProcessEventArgs(
            //messageReceiver./*ProcessEventAsync*/(null, new List<EventData> { eventData1, eventData2 });

            //Assert.AreEqual(2, alertManager.ReceivedAlertMessages.Count);
            //Assert.AreEqual(message1.CheckId, alertManager.ReceivedAlertMessages[0].CheckId);
            //Assert.AreEqual(message2.CheckId, alertManager.ReceivedAlertMessages[1].CheckId);
        }

        [TestMethod]
        public void TestInvalidMessage()
        {
            var alertManager = new MockAlertManager();
            var messageReceiver = new EventHubMessageReceiver(alertManager);

            var message1 = "{someinvalidstuff}";

            var eventData1 = new EventData(Encoding.UTF8.GetBytes(message1));

            //messageReceiver.ProcessEventsAsync(null, new List<EventData> { eventData1 });

            //Assert.AreEqual(0, alertManager.ReceivedAlertMessages.Count);
        }

        [TestMethod]
        public void TestJsonMessage()
        {
            var alertJson = @"{
	            'TimeGenerated': '2018-06-05T12:43:41.793Z',
	            'AlertName': '_Test generic LA alert',
	            'CheckID': 'GenericLogAnalyticsQueryAlert',
	            'ComponentID': 'MyResourceIDmodified',
	            'CustomField1': 'SearchQuery : AdvanceAlert_CL\n| where TimeGenerated > ago(5m) \n',
	            'CustomField2': 'LinkToSearchResults : https://cdc24ae1-3e28-441b-ba7a-5dd90d7df664.portal.mms.microsoft.com/#Workspace/search/index?_timeInterval.intervalEnd=2018-06-05T12%3a43%3a31.0000000Z&_timeInterval.intervalDuration=300&q=AdvanceAlert_CL%0A%7C%20where%20TimeGenerated%20%3E%20ago%285m%29%20%0A',
	            'CustomField3': 'AlertRuleDescription : ResourceID#MyResourceIDmodified#Description#Testdescription',
	            'CustomField4': '',
	            'CustomField5': '',
	            'Description': 'Testdescription',
	            'RecordID': '98c79542-526d-403e-a266-5dd0650b5361',
	            'SubscriptionID': '4025db3d-bb6f-4de1-8485-36a843f29882',
	            'sourceTimestamp': '2018-06-05T12:43:31Z',
	            'State': 'WARNING'
            }";

            var alertManager = new MockAlertManager();
            var messageReceiver = new EventHubMessageReceiver(alertManager);

            var eventData1 = new EventData(Encoding.UTF8.GetBytes(alertJson));

            //messageReceiver.ProcessEventsAsync(null, new List<EventData> { eventData1 });

            //Assert.AreEqual(1, alertManager.ReceivedAlertMessages.Count);

            var alertMessage = alertManager.ReceivedAlertMessages[0];

            //Assert.AreEqual(State.Warning, alertMessage.State);
        }

    }
}