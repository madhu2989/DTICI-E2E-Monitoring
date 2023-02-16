using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Serialization;

namespace Daimler.Providence.Service.Utilities
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class ProvidenceContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <inheritdoc />
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);
            contract.DictionaryKeyResolver = propertyName => propertyName;
            return contract;
        }
    }
}