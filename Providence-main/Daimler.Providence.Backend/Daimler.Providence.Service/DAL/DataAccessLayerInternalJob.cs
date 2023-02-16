using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Database;

namespace Daimler.Providence.Service.DAL
{
    public partial class DataAccessLayer
    {
        #region Internal Job

        /// <inheritdoc />
        public async Task<InternalJob> GetInternalJob(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetInternalJob started. (Id: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbInternalJob = await dbContext.InternalJobs.FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);
                    if (dbInternalJob == null)
                    {
                        throw new ProvidenceException($"Internal Job doesn't exist in the Database. (Id: '{id}')", HttpStatusCode.NotFound);
                    }
                    return dbInternalJob;
                }
            }
        }

        /// <inheritdoc />
        public async Task<List<InternalJob>> GetInternalJobs(CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"GetInternalJobs started.");
                using (var dbContext = GetContext())
                {
                    var dbInternalJobs = await dbContext.InternalJobs.ToListAsync(token).ConfigureAwait(false);
                    return dbInternalJobs;
                }
            }
        }

        /// <inheritdoc />
        public async Task<InternalJob> AddInternalJob(InternalJob internalJob, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"AddInternalJob started. (Type: '{internalJob.Type}')");
                using (var dbContext = GetContext())
                {
                    dbContext.InternalJobs.Add(internalJob);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);

                    var newDbInternalJob = await dbContext.InternalJobs.FirstOrDefaultAsync(d => d.Id == internalJob.Id, token).ConfigureAwait(false);
                    if (newDbInternalJob == null)
                    {
                        throw new ProvidenceException($"InternalJob could not be created. (Type: '{internalJob.Type}')", HttpStatusCode.NotFound);
                    }
                    return newDbInternalJob;
                }
            }
        }

        /// <inheritdoc />
        public async Task<InternalJob> UpdateInternalJob(InternalJob internalJob, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"UpdateInternalJob started. (Id: '{internalJob.Id}')");
                using (var dbContext = GetContext())
                {
                    var dbInternalJob = await dbContext.InternalJobs.FirstOrDefaultAsync(c => c.Id == internalJob.Id).ConfigureAwait(false);
                    if (dbInternalJob == null)
                    {
                        throw new ProvidenceException($"Internal Job doesn't exist in the Database. (Id: '{internalJob.Id}')", HttpStatusCode.NotFound);
                    }
                    dbInternalJob.State = internalJob.State;
                    dbInternalJob.StateInformation = internalJob.StateInformation;
                    dbInternalJob.FileName = internalJob.FileName;
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                    return dbInternalJob;
                }
            }
        }

        /// <inheritdoc />
        public async Task DeleteInternalJob(int id, CancellationToken token)
        {
            using (new ElapsedTimeLogger())
            {
                AILogger.Log(SeverityLevel.Information, $"DeleteInternalJob started. (Id: '{id}')");
                using (var dbContext = GetContext())
                {
                    var dbInternalJob = await dbContext.InternalJobs.FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);
                    if (dbInternalJob == null)
                    {
                        throw new ProvidenceException($"Internal Job doesn't exist in the Database. (Id: '{id}')", HttpStatusCode.NotFound);
                    }
                    dbContext.InternalJobs.Remove(dbInternalJob);
                    await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}