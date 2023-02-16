using System.Threading.Tasks;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Service.BusinessLogic.Interfaces
{
    /// <summary>
    /// Interface for Alert processing classes.
    /// </summary>
    public interface IAlertReceiver
    {
        /// <summary>
        /// Method for handling an array of <see cref="AlertMessage"/>s.
        /// </summary>
        /// <param name="alertMessages">The <see cref="AlertMessage"/>s which shall be handled.</param>
        Task HandleAlerts(AlertMessage[] alertMessages);
    }
}