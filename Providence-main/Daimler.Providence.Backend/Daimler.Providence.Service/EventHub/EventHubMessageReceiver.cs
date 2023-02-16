using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.EventHub
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class EventHubMessageReceiver : EventProcessorFactory
    {
        #region Private Members

        private readonly IAlertManager _alertManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EventHubMessageReceiver(IAlertManager alertManager) : base()
        {
            _alertManager = alertManager;
        }

        /// <summary>
        /// catches any exceptions occuring in the event hub.
        /// </summary>
        /// <param name="errorArgs"></param>
        /// <returns></returns>
        public override Task ProcessError(ProcessErrorEventArgs errorArgs)
        {
            AILogger.Log(SeverityLevel.Error, $"error received :: {errorArgs.Exception.ToString()}");
            Console.WriteLine($"error received :: {errorArgs.Exception.Message}");
            return Task.CompletedTask;
        }
        /// <summary>
        /// Processes the events coming in from the event hub
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public override async Task ProcessEventAsync(ProcessEventArgs eventArgs)
        {
            //Console.WriteLine($"Event Received from partition {eventArgs.Partition.PartitionId} :: {eventArgs.Data.EventBody.ToString()}");
            AILogger.Log(SeverityLevel.Information, $"EventHubMessageReceiver is received events from partition : '{eventArgs.Partition.PartitionId}");

            try
            {
                //eventArgs.Partition.
                if (eventArgs.Data != null)
                {
                    AILogger.Log(SeverityLevel.Information, $"EventHubMessageReceiver received event : '{eventArgs.Data.EventBody.ToString()}");
                    var alertMessages = ParseAlertMessage(eventArgs.Data);
                    await _alertManager.HandleAlerts(alertMessages.ToArray());
                    await eventArgs.UpdateCheckpointAsync();
                }
                else
                {
                    Console.WriteLine($"The event data is empty for the partition:: { eventArgs.Partition.PartitionId}");
                }
            }
            catch (Exception ex)
            {
                AILogger.Log(SeverityLevel.Error, "Error caught in the processor events receiver: " + ex.Message, exception: ex);
            }
        }

        #endregion

        #region Public Methods 
        protected static List<AlertMessage> ParseAlertMessage(EventData eventData)
        {
            try
            {
                List<AlertMessage> alertMessages = new List<AlertMessage>();
                var message = Encoding.UTF8.GetString(eventData.EventBody.ToArray());
                var alertMessage = JsonConvert.DeserializeObject<AlertMessage>(message);
                alertMessages.Add(alertMessage);
                return alertMessages;
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, $"Parsing message from EventHub failed. Reason: '{e.Message}'.", exception: e);
            }
            return null;
        }
        #endregion
    }
}