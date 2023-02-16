using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for healthChecks and cleanUp of Environments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MaintenanceBusinessLogic : IMaintenanceBusinessLogic
    {
        #region Private Members

        private readonly IStorageAbstraction _storageAbstraction;

        #endregion

        #region Constrcutor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public MaintenanceBusinessLogic(IStorageAbstraction storageAbstraction)
        {
            _storageAbstraction = storageAbstraction;
        }

        #endregion

        /// <inheritdoc />
        public Dictionary<string, string> GetAllThreads()
        {
            var result = new Dictionary<string, string>();
            try
            {
                var runningThreads = Process.GetCurrentProcess().Threads;

                result.Add("Threads.Count", runningThreads.Count.ToString());
                for (var i = 0; i < runningThreads.Count; i++)
                {

                    var threadInfo = $"Id: {runningThreads[i].Id} \n" +
                                     $"StartTime: {runningThreads[i].StartTime} \n " +
                                     $"ThreadState: {runningThreads[i].ThreadState} \n  " +
                                     $"TotalProcessorTime: {runningThreads[i].TotalProcessorTime} \n ";

                    if (runningThreads[i].ThreadState == System.Diagnostics.ThreadState.Wait)
                    {
                        threadInfo += $"WaitReason: {runningThreads[i].WaitReason}\n ";
                    }
                    threadInfo += "==============================";
                    result.Add((i + 1).ToString(), threadInfo);
                }
            }
            catch (Exception e)
            {
                result.Clear();
                result.Add("Error", e.Message);
            }
            return result;
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetMemoryConsumption()
        {
            var result = new Dictionary<string, string>
            {
                {"Private Memory", (Process.GetCurrentProcess().PrivateMemorySize64 / 1024).ToString()},
                {"Physical memory usage", (Process.GetCurrentProcess().WorkingSet64 / 1024).ToString()},
                {"Paged system memory size", (Process.GetCurrentProcess().PagedSystemMemorySize64 / 1024).ToString()},
                {"Paged memory size", (Process.GetCurrentProcess().PagedMemorySize64 / 1024).ToString()}
            };
            return result;
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetCpuConsumption()
        {
            var result = new Dictionary<string, string>
            {
                {"User processor time", (Process.GetCurrentProcess().UserProcessorTime).ToString()},
                {"Privileged processor time", (Process.GetCurrentProcess().PrivilegedProcessorTime).ToString()},
                {"Total processor time", (Process.GetCurrentProcess().TotalProcessorTime).ToString()}
            };
            return result;
        }

        /// <inheritdoc />
        public async Task DeleteExpiredStateTransitions(DateTime cutOffDate, CancellationToken token)
        {
            await _storageAbstraction.DeleteExpiredStateTransitions(cutOffDate, token);
        }

        /// <inheritdoc />
        public async Task DeleteExpiredDeployments(DateTime cutOffDate, CancellationToken token)
        {
            await _storageAbstraction.DeleteExpiredDeployments(cutOffDate, token);
        }

        /// <inheritdoc />
        public async Task DeleteExpiredChangeLogs(DateTime cutOffDate, CancellationToken token)
        {
            await _storageAbstraction.DeleteExpiredChangeLogs(cutOffDate, token);
        }

        /// <inheritdoc />
        public async Task DeleteUnassignedComponents(DateTime cutOffDate, CancellationToken token)
        {
            await _storageAbstraction.DeleteUnassignedComponents(cutOffDate, token);
        }
    }
}