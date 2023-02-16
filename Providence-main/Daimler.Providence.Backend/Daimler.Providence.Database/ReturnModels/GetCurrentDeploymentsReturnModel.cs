using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class GetCurrentDeploymentsReturnModel
    {
        public int Id { get; set; }
        public int EnvironmentId { get; set; }
        public string ElementIds { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string CloseReason { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public int? ParentId { get; set; }
        public string RepeatInformation { get; set; }
        public string EnvironmentName { get; set; }
        public string EnvironmentSubscriptionId { get; set; }
    }
}
