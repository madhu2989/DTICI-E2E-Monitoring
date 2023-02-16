using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("StateIncreaseRules", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public partial class StateIncreaseRules
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CheckId { get; set; }

        public string AlertName { get; set; }

        public string ComponentId { get; set; }

        public int TriggerTime { get; set; }

        public bool IsActive { get; set; }

        public string EnvironmentSubscriptionId { get; set; }
        public virtual Environment EnvironmentSubscription { get; set; }
    }
}
