using System;
using System.Collections.Generic;

namespace ProvidenceFuncAppPayload
{
    public class PayloadOut
    {
        public string AlertName { get; set; }
        public string CheckId { get; set; }
        public string ComponentId { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }
        public string Description { get; set; }
        public string RecordId { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public string State { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime TimeGenerated { get; set; }
    }
}
