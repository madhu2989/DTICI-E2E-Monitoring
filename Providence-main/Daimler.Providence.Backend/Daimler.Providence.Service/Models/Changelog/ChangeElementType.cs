namespace Daimler.Providence.Service.Models.ChangeLog
{
    /// <summary>
    /// Enum which defines the different Element types.
    /// </summary>
    public enum ChangeElementType
    {
        /// <summary>
        /// Type of the changed element is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The changed element was an Environment.
        /// </summary>
        Environment = 1,

        /// <summary>
        /// The changed element was an environment.
        /// </summary>
        Check = 2,

        /// <summary>
        /// The changed element was Deployment.
        /// </summary>
        Deployment = 3,

        /// <summary>
        /// The changed element was a recurring Deployment.
        /// </summary>
        DeploymentRecurring = 4,

        /// <summary>
        /// The changed element was a AlertIgnoreRule.
        /// </summary>
        AlertIgnore = 5,

        /// <summary>
        /// The changed element was a NotificationRule.
        /// </summary>
        NotificationRule = 6,

        /// <summary>
        /// The changed element was an IncreaseRule.
        /// </summary>
        IncreaseRule = 7,

        /// <summary>
        /// The changed element was a Configuration.
        /// </summary>
        Configuration = 8
    }
}