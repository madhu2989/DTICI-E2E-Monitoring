using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Class containing JSON Helper methods.
    /// </summary>
    public class JsonUtils
    {
        /// <summary>
        /// Empty json string.
        /// </summary>
        public const string Empty = "{}";

        /// <summary>
        /// Method for comparing two JSON strings.
        /// </summary>
        public static string GetDiff(string json1, string json2)
        {
            var jdp = new JsonDiffPatch();
            JToken diff = jdp.Diff(json1 ?? Empty, json2 ?? Empty);
            return diff.ToString();
        }
    }
}