using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Database;
using Daimler.Providence.Service.Models.SLA;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for <see cref="SlaCalculationManager"/> class.
    /// </summary>
    public interface ISlaCalculationManager
    {
        /// <summary>
        /// Method for calculating the SLA for each Element in the Environment for a given timeFrame.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique Element of the existing Environment the SLA shall be calculated for.</param>
        /// <param name="startDate">The Date which determines the start of the SLA calculation.</param>
        /// <param name="endDate">The Date which determines the end of the SLA calculation.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<SlaBlobRecord> CalculateSlaAsync(string environmentSubscriptionId, DateTime startDate, DateTime endDate, CancellationToken token);

        /// <summary>
        /// Method for retrieving the raw SLA data for each Element in the Environment for a given timeFrame.
        /// </summary>
        /// <param name="environmentSubscriptionId">The unique ElementId of the existing Environment the raw SLA data shall be calculated for.</param>
        /// <param name="elementId">The unique ElementId which belongs to the Element the raw SLA data shall be retrieved for.</param>
        /// <param name="start">The Date which determines the start of the raw SLA data retrieval.</param>
        /// <param name="end">The Date which determines the end of the raw SLA data retrieval.</param>
        /// <param name="token">The token used to cancel backend operations.</param>
        Task<Dictionary<string, SlaDataRaw>> GetRawSlaDataAsync(string environmentSubscriptionId, string elementId, DateTime start, DateTime end, CancellationToken token);
    }
}