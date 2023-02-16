using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Azure.Identity;
using Daimler.Providence.Service.Utilities;
using Azure.Storage.Blobs;
using System;
using System.Linq;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {

        private static int counter = 1;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Get()
        {
            return Ok("Good");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [NonAction]
        [AllowAnonymous]    
        [Route("/verifymiauth")]
        public IActionResult VerifyAzureManagedIdentityReadiness()
        {
            try
            {
                var userAssignedCred = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = ProvidenceConfigurationManager.ManagedIdentity
                });

                string storageBasePath = ProvidenceConfigurationManager.StorageUrlPath;
                BlobServiceClient _blobServiceClient = new BlobServiceClient(new Uri(storageBasePath), userAssignedCred);
                var containers = _blobServiceClient.GetBlobContainers().AsPages().ToList();
                return Ok();
            }
            catch (Exception ex)
            {
                counter++;
                return BadRequest(ex);
            }

        }
    }
}
