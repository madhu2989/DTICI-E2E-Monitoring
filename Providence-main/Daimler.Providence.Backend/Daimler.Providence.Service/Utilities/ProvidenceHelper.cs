using System;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class which contains general Helper-Methods for this Solution.
    /// </summary>
    public static class ProvidenceHelper
    {
        /// <summary>
        /// Method used for comparing two string with each other.
        /// </summary>
        /// <param name="string1">First string to be compared.</param>
        /// <param name="string2">Second string to be compared.</param>
        public static bool CompareString(string string1, string string2)
        {
            return string.IsNullOrEmpty(string1) && string.IsNullOrEmpty(string2) ||
                   !string.IsNullOrEmpty(string1) && !string.IsNullOrEmpty(string2) && string1.Equals(string2, StringComparison.OrdinalIgnoreCase);
        }
    }
}