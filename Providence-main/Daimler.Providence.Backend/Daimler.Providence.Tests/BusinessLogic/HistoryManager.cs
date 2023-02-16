using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.SignalR;

namespace Daimler.Providence.Service.BusinessLogic
{
    public class HistoryManager
    {
        #region Private Members

        // Connection for SignalR to UI
        private readonly ClientRepository _clientRepository;
       
        private readonly IStorageAbstraction _storageAbstraction;
        private string _environmentName;

        // List holding all history entries for the last 3 days
        private Dictionary<string, List<StateTransition>> _history = new Dictionary<string, List<StateTransition>>();
        
        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public HistoryManager(ClientRepository clientRepository, IStorageAbstraction storageAbstraction, string environmentName)
        {
            _clientRepository = clientRepository;
            _storageAbstraction = storageAbstraction;
            _environmentName = environmentName;
            Task.Run(LoadHistoryForEnvironment).Wait();
        }

        #endregion

        #region Public Methods


        #endregion

        #region Private Methods

        private async Task LoadHistoryForEnvironment()
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-72);

            _history = await _storageAbstraction.GetStateTransitionHistory(_environmentName, startDate, endDate, CancellationToken.None)
                .ConfigureAwait((false));
        }

        #endregion
    }
}
