using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models.ImportExport;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="ImportExportManager"/> class.
    /// </summary>
    public interface IImportExportManager
    {
        /// <summary>
        /// Method for importing an <see cref="Environment"/>.
        /// </summary>
        /// <param name="environment">The <see cref="Environment"/> to be imported.</param>
        /// <param name="environmentName">The name of the existing Environment to be updated.</param>
        /// <param name="environmentSubscriptionId">The unique ElementId of the existing Environment to be updated.</param>
        /// <param name="replace">The flag containing information about the Environment's replacement mode.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Dictionary<string, List<string>>> ImportEnvironmentAsync(Environment environment, string environmentName, string environmentSubscriptionId, ReplaceFlag replace, CancellationToken token);

        /// <summary>
        /// Method for exporting an <see cref="Environment"/>.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the existing Environment to be exported.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Environment> ExportEnvironmentAsync(string environmentSubscriptionId, CancellationToken token);
    }
}