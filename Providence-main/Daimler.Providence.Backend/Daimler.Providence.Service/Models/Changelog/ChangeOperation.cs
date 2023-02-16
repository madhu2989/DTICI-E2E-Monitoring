namespace Daimler.Providence.Service.Models.ChangeLog
{
    /// <summary>
    /// Enum which defines the different ChangeOperation types.
    /// </summary>
    public enum ChangeOperation
    {
        /// <summary>
        /// ChangeOperation which indicates that an element was added.
        /// </summary>
        Add = 1,

        /// <summary>
        /// ChangeOperation which indicates that an element was updated.
        /// </summary>
        Update = 2,

        /// <summary>
        /// ChangeOperation which indicates that an element was deleted.
        /// </summary>
        Delete = 3
    }
}