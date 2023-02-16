using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("StateTransitionHistory", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public partial class StateTransitionHistory
    {
        public int Id { get; set; }

        public string ElementId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ComponentType { get; set; }
        public virtual ComponentType ComponentType_ComponentType { get; set; }

        public int EnvironmentId { get; set; }
        public virtual Environment Environment_Environment { get; set; }

        public int State { get; set; }
        public virtual State State_State { get; set; }
    }
}
