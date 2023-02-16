using System;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class GetAllElementsWithEnvironmentIdReturnModel
    {
        public string ElementId { get; set; }
        public string EnvironmentSubscriptionId { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Type { get; set; }
    }
}
