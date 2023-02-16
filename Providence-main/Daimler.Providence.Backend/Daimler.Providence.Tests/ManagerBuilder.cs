using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;

namespace Daimler.Providence.Tests
{
    /// <summary>
    /// This methods can be later adapted to create real managers. This is used only for compiling.
    /// </summary>
    public class ManagerBuilder
    {
        public static IStateManager CreateStateManager(IStorageAbstraction storageAbstraction, string environmentName)
        {
            IStateManager stateManager = new StateManager(storageAbstraction, environmentName, new Service.SignalR.ClientRepository(null));
            stateManager.InitializeStateManager();
            return stateManager;
        }

        public static IAlertManager CreateAlertManager(IEnvironmentManager environmentManager)
        {
            IAlertManager alertManager = new AlertManager(environmentManager);
            return alertManager;
        }

        public static IEnvironmentManager CreateEnvironmentManager(IStorageAbstraction storageAbstraction)
        {
            IEnvironmentManager environmentManager = new EnvironmentManager(storageAbstraction, new Service.SignalR.ClientRepository(null));
            return environmentManager;
        }

        public static IImportExportManager CreateImportExportManager(IStorageAbstraction storageAbstraction, IEnvironmentManager environmentManager)
        {
            IImportExportManager manager = new ImportExportManager(storageAbstraction, environmentManager);
            return manager;
        }

        public static ISlaCalculationManager CreateSlaCalculationManager(IStorageAbstraction storageAbstraction)
        {
           ISlaCalculationManager manager = new SlaCalculationManager(storageAbstraction);
           return manager;
        }
    }
}
