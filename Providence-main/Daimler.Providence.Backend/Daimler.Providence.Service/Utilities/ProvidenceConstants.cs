namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class which contains all constant values used in this project.
    /// </summary>
    public static class ProvidenceConstants
    {
        /// <summary>
        ///  The description for the "ResetToGreen" Alert Message.
        /// </summary>
        public static string ResetDescription { get; } = "Reset To Green";

        /// <summary>
        /// The CheckId used for all "Heartbeat" Alert Messages.
        /// </summary>
        public static string HeartbeatCheckId { get; } = "HeartbeatAlert";

        /// <summary>
        /// The time threshold after that a negative heartbeat state is send to the clients.
        /// </summary>
        public static int HeartbeatThreshold { get; } = 15;

        /// <summary>
        /// The name used for the "Unassigned Components" Service.
        /// </summary>
        public static string UnassignedServiceName { get; } = "UnassignedComponents";

        /// <summary>
        /// The name used for the "Unassigned Components" Action.
        /// </summary>
        public static string UnassignedActionName { get; } = "UnassignedComponentsAction";

        /// <summary>
        /// The ElementType of a Check.
        /// </summary>
        public static string ElementTypeCheck { get; } = "Check";

        /// <summary>
        /// The TimeInterval after which an already executed Deployment is removed from cache.
        /// </summary>
        public static int CurrentDeploymentDeletionInterval { get; } = 900;

        /// <summary>
        /// Name used by the Service to send Email Notifications.
        /// </summary>
        public static string EmailNotificationSenderName { get; } = "CSG-E2E_Monitoring_Crew";

        /// <summary>
        /// Address used by the Service to send Email Notifications.
        /// </summary>
        public static string EmailNotificationSenderAddress { get; } = "csg-e2e_monitoring_crew@daimler.com";

        /// <summary>
        /// Address of the Mail Server used by the Service to send Email Notifications.
        /// </summary>
        public static string EmailNotificationMailServerAddress { get; } = "mailhost.apac.bg.corpintra.net";

        /// <summary>
        /// Port of the Mail Server used by the Service to send Email Notifications.
        /// </summary>
        public static int EmailNotificationMailServerPort { get; } = 25;

        /// <summary>
        /// Section containing the app settings parameters.
        /// </summary>
        public static string AppSettingsSection { get; } = "AppSettings";

        /// <summary>
        /// Section containing the connection strings.
        /// </summary>
        public static string ConnectionStringSection { get; } = "ConnectionStrings";

        /// <summary>
        /// Name of the Blob Storage Container used for storing calculated SLA data.
        /// </summary>
        public static string SlaBlobStorageContainerName { get; } = "calculated-sla-data";

        /// <summary>
        /// Name of the ConsumerGroup used for reading data from the Eventhub.
        /// </summary>
       // public static string EventHubConsumerGroupName { get; } = "monitoringservice";
        public static string EventHubConsumerGroupName { get; } = "alertreceiver";
    }
}