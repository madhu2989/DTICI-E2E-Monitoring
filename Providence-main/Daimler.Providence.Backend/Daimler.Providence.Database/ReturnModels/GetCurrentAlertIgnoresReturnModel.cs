using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class GetCurrentAlertIgnoresReturnModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime ExpirationDate { get; set; }
        public string IgnoreCondition { get; set; }
        public string EnvironmentSubscriptionId { get; set; }
    }
}