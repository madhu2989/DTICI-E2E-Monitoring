namespace Daimler.Providence.Service.Models.ImportExport
{
    /// <summary>
    /// Flag that indicates how an environment should be treated when importing data.
    /// </summary>
    public enum ReplaceFlag
    {
        /// <summary>
        /// The environment defined in the payload just updates the existing one.
        /// </summary>
        False = 0,

        /// <summary>
        /// The environment defined in the payload overwrites the existing one.
        /// </summary>
        True = 1,

        /// <summary>
        /// The existing environment is completely deleted and a new one is created based on the environment defined payload.
        /// </summary>
        All = 2
    }
}