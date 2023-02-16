using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("StateTransition", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class StateTransition
    {
        public StateTransition()
        {
            AlertComment = new HashSet<AlertComment>();
        }

        public int Id { get; set; }

        public DateTime? SourceTimestamp { get; set; }

        public DateTime? GeneratedTimestamp { get; set; }

        public string Description { get; set; }

        public string Customfield1 { get; set; }

        public string Customfield2 { get; set; }

        public string Customfield3 { get; set; }

        public string Customfield4 { get; set; }

        public string Customfield5 { get; set; }

        public string CheckId { get; set; }

        public string Guid { get; set; }

        public string ElementId { get; set; }

        public string AlertName { get; set; }

        public string TriggeredByCheckId { get; set; }

        public string TriggeredByElementId { get; set; }

        public string TriggeredByAlertName { get; set; }

        public int? ProgressState { get; set; }

        public virtual ICollection<AlertComment> AlertComment { get; set; }

        public int? ComponentType { get; set; }
        public virtual ComponentType ComponentType_ComponentType { get; set; }
        public int? Environment { get; set; }
        public virtual Environment Environment_Environment { get; set; }
        public int? State { get; set; }
        public virtual State State_State { get; set; }
    }
}
