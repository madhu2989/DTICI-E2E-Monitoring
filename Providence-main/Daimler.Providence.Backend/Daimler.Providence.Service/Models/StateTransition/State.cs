using System.Runtime.Serialization;

namespace Daimler.Providence.Service.Models.StateTransition
{
    /// <summary>
    /// Enum which defines the different States an Element can have.
    /// </summary>
    [DataContract]
    public enum State
    {
        [EnumMember(Value = "OK")]
        Ok = 1,
        [EnumMember(Value = "WARNING")]
        Warning = 2,
        [EnumMember(Value = "ERROR")]
        Error = 3
    }
}