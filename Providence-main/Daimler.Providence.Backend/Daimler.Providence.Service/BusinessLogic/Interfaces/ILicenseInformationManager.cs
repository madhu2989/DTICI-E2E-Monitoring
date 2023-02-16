using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="LicenseInformationManager"/> class.
    /// </summary>
    public interface ILicenseInformationManager
    {
        /// <summary>
        /// Method for retrieving all license information for the project.
        /// </summary>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<List<LicenseInformation>> GetLicenseInformationAsync(CancellationToken token);
    }
}