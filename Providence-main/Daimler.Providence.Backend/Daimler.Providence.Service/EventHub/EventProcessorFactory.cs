using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.Threading;

namespace Daimler.Providence.Service.EventHub
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public abstract class EventProcessorFactory
    {
        #region Public Members

        /// <summary>
        /// Flag which indicates whether a connection was established or not.
        /// </summary>
        public bool ConnectionEstablished { get; set; }

        #endregion

        #region Private Memebers
        private EventProcessorClient processorClient;
        private BlobContainerClient containerClient;
        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EventProcessorFactory()
        {
            string userAssignedIdentity = ProvidenceConfigurationManager.ManagedIdentity;
            //string userAssignedIdentity = "2706c202-b9f3-4901-8294-d473e835e758";
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = userAssignedIdentity
            });
            AILogger.Log(SeverityLevel.Information, "connecting blob connection and event hub using userAssignedIdentity :: " +userAssignedIdentity);


            string containerPath = ProvidenceConfigurationManager.StorageUrlPath + "/" + ProvidenceConfigurationManager.EventHubName;
            AILogger.Log(SeverityLevel.Information, "Container path :: " +containerPath);


            containerClient = new BlobContainerClient(new Uri(containerPath), credential);
            processorClient = new EventProcessorClient(containerClient, ProvidenceConstants.EventHubConsumerGroupName, ProvidenceConfigurationManager.EventHubQualifiedNameSpace, ProvidenceConfigurationManager.EventHubName, credential);

            // Registers the Event Processor Host and starts receiving messages
            processorClient.ProcessEventAsync+=ProcessEventAsync;
            processorClient.ProcessErrorAsync+=ProcessError;

            ConnectionEstablished = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// starts the event processing client.
        /// </summary>
        /// <returns></returns>
        public async Task StartEventListenerAsync()
        {
            await processorClient.StartProcessingAsync();
            AILogger.Log(SeverityLevel.Information, "Processing started...");
            Console.WriteLine("Processing started.");
        }


        /// <summary>
        /// parent method captures the errors that occur during event processing
        /// </summary>
        /// <param name="errorArgs"></param>
        /// <returns></returns>
        public abstract Task ProcessError(ProcessErrorEventArgs errorArgs);

        /// <summary>
        /// parent methhod registered to receive messages
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public abstract Task ProcessEventAsync(ProcessEventArgs eventArgs);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task StopEventHubListenerAsync()
        {
            AILogger.Log(SeverityLevel.Information, "StopEventHubListenerAsync.");
            try
            {
                if (processorClient != null)
                {
                    await processorClient.StopProcessingAsync();
                }
            }
            catch (Exception e)
            {
                AILogger.Log(SeverityLevel.Error, e.Message, exception: e);
            }
        }
        #endregion Public Methods
    }
}