using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("AlertComment", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class AlertComment
    {
        public int Id { get; set; }

        public string User { get; set; }

        public string Comment { get; set; }

        public int State { get; set; }

        public DateTime Timestamp { get; set; }

        public int StateTransitionId { get; set; }
        public virtual StateTransition StateTransition { get; set; }
    }
}
