using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Manager for managing all Providence configurations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProvidenceConfigurationManager
    {
        private static readonly IDictionary<string, object> Settings = new Dictionary<string, object>();
        private static readonly IDictionary<string, string> ConnectionStrings = new Dictionary<string, string>();
    
        private static IConfiguration _configuration { get; set; }

        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Properties

        /// <summary>
        /// The environment the app service is running on.
        /// </summary>
        public static string Environment => GetConfigurationValue("Environment", "");

        /// <summary>
        /// The environment the app service is running on.
        /// </summary>
        public static string Region => GetConfigurationValue("Region", "");

        /// <summary>
        /// The id of the AAD Tenant the app service is registered in.
        /// </summary>
        public static string TenantId => GetConfigurationValue("TenantId", "");

        /// <summary>
        /// The isntance of the authority the app service is registered in.
        /// </summary>
        public static string Instance => GetConfigurationValue("AuthBaseUrl", "");

        /// <summary>
        /// The id of the application registered in the AAD.
        /// </summary>
        public static string EnterpriseApplicationAppId => GetConfigurationValue("EnterpriseApplication-AppId", "");

        /// <summary>
        /// The flag which indicates whether the "RefreshJob" is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunAutoRefresh => GetConfigurationValue("RunAutoRefresh", false);

        /// <summary>
        /// The time interval after which the "RefreshJob" shall be executed.
        /// </summary>
        public static int AutoRefreshJobIntervalInSeconds => GetConfigurationValue("AutoRefreshJobIntervalInSeconds", 300);

        /// <summary>
        /// The flag which indicates whether the "ResetJob" is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunAutoReset => GetConfigurationValue("RunAutoReset", false);

        /// <summary>
        /// The time interval after which the "ResetJob" shall be executed.
        /// </summary>
        public static int AutoResetJobIntervalInSeconds => GetConfigurationValue("AutoResetJobIntervalInSeconds", 300);

        /// <summary>
        /// The flag which indicates whether the "DeleteExpiredStateTransitions" job is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunDeleteExpiredStateTransitions => GetConfigurationValue("RunDeleteExpiredStateTransitions", false);

        /// <summary>
        /// The flag which indicates whether the "DeleteUnassignedComponents" job is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunDeleteUnassignedComponents => GetConfigurationValue("RunDeleteUnassignedComponents", false);

        /// <summary>
        /// The flag which indicates whether the "DeleteExpiredInternalJobs" job is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunDeleteExpiredInternalJobs => GetConfigurationValue("RunDeleteExpiredInternalJobs", false);

        /// <summary>
        /// The flag which indicates whether the "DeleteExpiredDeployments" job is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunDeleteExpiredDeployments => GetConfigurationValue("RunDeleteExpiredDeployments", false);

        /// <summary>
        /// The flag which indicates whether the "DeleteExpiredChangeLogs" job is enabled (true) or not (null or false).
        /// </summary>
        public static bool RunDeleteExpiredChangeLogs => GetConfigurationValue("RunDeleteExpiredChangeLogs", false);

        /// <summary>
        /// The time in weeks after which "expired" entries from the database shall be deleted.
        /// </summary>
        public static int CutOffTimeRangeInWeeks => GetConfigurationValue("CutOffTimeRangeInWeeks", 8);

        /// <summary>
        /// The flag which indicates whether the logging of elapsed time is enabled (true) or not (null or false).
        /// </summary>
        public static bool LogElapsedTime => GetConfigurationValue("LogElapsedTime", false);

        /// <summary>
        /// Minimal value for elapsed time in seconds, which will be logged.
        /// All values less that this value will not be logged.
        /// If this value is not set, all messages will be logged.
        /// </summary>
        public static int MinElapsedTime => GetConfigurationValue("MaxElapsedTimeInMinutes", 60 * 60);

        /// <summary>
        /// The time interval after which the "EmailNotificationJob" shall be executed.
        /// </summary>
        public static int EmailNotificationJobIntervalInSeconds => GetConfigurationValue("EmailNotificationJobIntervalInSeconds", 60);

        /// <summary>
        /// The time interval after which the "StateIncreaseJob" shall be executed.
        /// </summary>
        public static int StateIncreaseJobIntervalInSeconds => GetConfigurationValue("StateIncreaseJobIntervalInSeconds", 300);

        /// <summary>
        /// The time interval after which the "UpdateDeploymentsJob" shall be executed.
        /// </summary>
        public static int UpdateDeploymentsJobIntervalInSeconds => GetConfigurationValue("UpdateDeploymentsJobIntervalInSeconds", 600);

        /// <summary>
        /// The username which is used to send notification via Daimler Relay Service.
        /// </summary>
        public static string DaimlerRelayUsername => GetConfigurationValue("DaimlerRelayUsername", "");

        /// <summary>
        /// The password which is used to send notification via Daimler Relay Service.
        /// </summary>
        public static string DaimlerRelayPassword => GetConfigurationValue("DaimlerRelayPassword", "");

        /// <summary>
        /// The flag which indicates whether to process EventHub messages or not.
        /// </summary>
        public static bool EnableEventHubReader => GetConfigurationValue("EnableEventHubReader", false);

        /// <summary>
        /// The key which is used for logging into ApplicationInsights.
        /// </summary>
        public static string ApplicationInsightsInstrumentationKey => GetConfigurationValue("ApplicationInsightsInstrumentationKey", "");

        /// <summary>
        /// The flag which indicates whether the logging of sql queries is enabled (true) or not (null or false).
        /// </summary>
        public static bool LogSqlQuery => GetConfigurationValue("LogSqlQuery", false);

        /// <summary>
        /// Managed identity client id
        /// </summary>
        public static string ManagedIdentity => GetConfigurationValue("ManagedIdentity", "");

        /// <summary>
        /// Get eventhub name
        /// </summary>
        public static string EventHubName => GetConfigurationValue("EventHubName", "");

        /// <summary>
        /// Get storage path
        /// </summary>
        public static string StorageUrlPath => GetConfigurationValue("StorageUrlPath", "");

        /// <summary>
        /// 
        /// </summary>
        public static string EventHubQualifiedNameSpace => GetConfigurationValue("EventHubQualifiedNameSpace", "");
        /// <summary>
        /// app insight connection string
        /// </summary>
        public static string ApplicationInsightConnectionString => GetConfigurationValue("AppInsightsConnectionString", "");

        #endregion

        #region Connection Strings

        /// <summary>
        /// Connection String used for EventHub connection.
        /// </summary>
        public static string EventHubConnectionString => GetConnectionString("EventHubConnectionString");

        /// <summary>
        /// Connection String used for StorageAccount connection.
        /// </summary>
        public static string StorageAccountConnectionString => GetConnectionString("StorageAccountConnectionString");

        /// <summary>
        /// Connection String used for Database connection.
        /// </summary>
        public static string DatabaseConnectionString => GetConnectionString("DatabaseConnectionString");

        #endregion

        #region Private Methodes

        private static T GetConfigurationValue<T>(string key, T defaultValue)
        {

            if (!Settings.ContainsKey(key))
            {
                object value = defaultValue;
                var type = typeof(T);
                if (type == typeof(bool))
                {

                    if (bool.TryParse(_configuration.GetSection(ProvidenceConstants.AppSettingsSection)[key], out bool boolValue))
                    {
                        value = boolValue;
                    }
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(_configuration.GetSection(ProvidenceConstants.AppSettingsSection)[key], out int intValue))
                    {
                        value = intValue;
                    }
                }
                else if (type == typeof(string))
                {
                    value = _configuration.GetSection(ProvidenceConstants.AppSettingsSection)[key];
                }
                Settings.Add(key, value);
            }
            return (T)Settings[key];
        }

        private static string GetConnectionString(string key)
        {
            if (!ConnectionStrings.ContainsKey(key))
            {
                var value = _configuration.GetSection(ProvidenceConstants.ConnectionStringSection)[key];
                ConnectionStrings.Add(key, value);
            }
            return ConnectionStrings[key];
        }

        #endregion
    }
}