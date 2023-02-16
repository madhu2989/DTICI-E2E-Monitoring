using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Serialization;

namespace Daimler.Providence.Service.SignalR
{
    // See: https://www.it-swarm.dev/de/serialization/signalr-kamelhuelle-verwenden/1052084583/

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class SignalRContractResolver : IContractResolver
    {
        #region Private Members

        private readonly Assembly _assembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SignalRContractResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _assembly = typeof(HubCallerContext).Assembly;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public JsonContract ResolveContract(Type type)
        {
            return type.Assembly.Equals(_assembly) ? _defaultContractSerializer.ResolveContract(type) : _camelCaseContractResolver.ResolveContract(type);
        }

        #endregion
    }
}