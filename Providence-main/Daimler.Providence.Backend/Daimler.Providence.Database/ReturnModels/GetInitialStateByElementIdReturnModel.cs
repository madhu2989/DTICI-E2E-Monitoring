using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class GetInitialStateByElementIdReturnModel
    {
        public int Id { get; set; }
        public int? Environment { get; set; }
        public System.DateTime? SourceTimestamp { get; set; }
        public int? State { get; set; }
        public int? ComponentType { get; set; }
        public System.DateTime? GeneratedTimestamp { get; set; }
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
        public string ComponentTypeName { get; set; }
        public string EnvironmentName { get; set; }
        public string StateName { get; set; }
        public string TriggerName { get; set; }
    }
}
