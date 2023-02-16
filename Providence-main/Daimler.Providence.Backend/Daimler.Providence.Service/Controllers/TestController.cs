using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Daimler.Providence.Service.EventHub;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Storage;
//using Microsoft.Azure.Storage.Blob;
using Microsoft.Data.SqlClient;
using Swashbuckle.AspNetCore.Annotations;

namespace Daimler.Providence.Service.Controllers
{
    /// <summary>
    /// Controller which provides endpoints for testing the service.
    /// </summary>
    [Authorize]
    [Route("api/test")]
    [TypeFilter(typeof(ProvidenceExceptionFilterAttribute))]
    public class TestController : ControllerBase
    {
        #region Private Members

        private readonly EventHubMessageReceiver _eventProcessorFactory;

        // Parameters used for test connections
        private static string _databaseConnectionString;
        private static string _storageAccountConnectionString;
        private static string _slaBlobContainerName = "calculated-sla-data";

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TestController(EventHubMessageReceiver eventProcessorFactory)
        {
            _eventProcessorFactory = eventProcessorFactory;
            _databaseConnectionString = ProvidenceConfigurationManager.DatabaseConnectionString;
            _storageAccountConnectionString = ProvidenceConfigurationManager.StorageAccountConnectionString;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Endpoind for testing the EvnetHub Connection.
        /// </summary>
        [HttpGet]
        [Route("service", Name = "TestService")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Test availability of the Service", Type = typeof(string))]
        [ExcludeFromCodeCoverage]
        public IActionResult TestService()
        {
            AILogger.Log(SeverityLevel.Information, "[GET] Test Service availability called.");
            const string responseMessage = "Successfully tested Service availability.";
            return ResponseBuilder.CreateResponse(HttpStatusCode.OK, null, SeverityLevel.Information, responseMessage);
        }


        /// <summary>
        /// Endpoind for testing the EvnetHub Connection.
        /// </summary>
        [HttpGet]
        [Route("eventhub", Name = "TestEventHub")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Test availability of Eventhub", Type = typeof(string))]
        [ExcludeFromCodeCoverage]
        public IActionResult TestEventHub()
        {
            AILogger.Log(SeverityLevel.Information, "[GET] Test Eventhub availability called.");
            var result = _eventProcessorFactory.ConnectionEstablished;
            if (result)
            {
                const string responseMessage = "Successfully tested EventHub availability.";
                return ResponseBuilder.CreateResponse(HttpStatusCode.OK, null, SeverityLevel.Information, responseMessage);
            }
            var message = $"Testing Eventhub availability failed. Reason: Connection to Eventhub could not be established.";
            throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Endpoind for testing the Database Connection.
        /// </summary>
        [HttpGet]
        [Route("database", Name = "TestDatabase")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Test availability of Database", Type = typeof(string))]
        [ExcludeFromCodeCoverage]
        public IActionResult TestDatabase()
        {
            AILogger.Log(SeverityLevel.Information, "[GET] Test Database availability called.");

            object testData = null;
            var sqlConnection = new SqlConnection(_databaseConnectionString);
            try
            {
                sqlConnection.Open();

                var command = new SqlCommand("SELECT * FROM ComponentType", sqlConnection);
                var reader = command.ExecuteReader();
                if (reader.HasRows && reader.Read())
                {
                    testData = reader.GetValue(1);
                }
                sqlConnection.Close();
                command.Dispose();

                if (testData != null)
                {
                    const string responseMessage = "Successfully tested Database availability.";
                    return ResponseBuilder.CreateResponse(HttpStatusCode.OK, null, SeverityLevel.Information, responseMessage);
                }
                else
                {
                    var message = $"Testing Database availability failed. Reason: Testdata could not be retrieved from Database.";
                    AILogger.Log(SeverityLevel.Warning, message);
                    throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
                }
            }
            catch (SqlException)
            {
                var message = $"Testing Database availability failed. Reason: Connection to Database could not be established.";
                AILogger.Log(SeverityLevel.Error, message);
                throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Endpoind for testing the StorageAccount Connection.
        /// </summary>
        [HttpGet]
        [Route("storageaccount", Name = "TestStorageAccount")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Test availability of StorageAccount", Type = typeof(string))]
        [ExcludeFromCodeCoverage]
        public IActionResult TestStorageAccount()
        {
            AILogger.Log(SeverityLevel.Information, "[GET] Test StorageAccount availability called.");

            // Retrieve storage accounts from connection string.
            //var storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionString);
            //if (storageAccount != null)
            //{
            //    // Create the blob client.
            //    var slaBlobClient = storageAccount.CreateCloudBlobClient();

            //    // Retrieve reference to the recordings container/directory.
            //    var container = slaBlobClient.GetContainerReference(_slaBlobContainerName);
            //    if (container != null)
            //    {
            //        const string responseMessage = "Successfully tested StorageAccount availability.";
            //        return ResponseBuilder.CreateResponse(HttpStatusCode.OK, null, SeverityLevel.Information, responseMessage);
            //    }
            //    else
            //    {
            //        var message = $"Testing StorageAccount availability failed. Reason: BlobContainer could not be retrieved from StorageAccount.";
            //        AILogger.Log(SeverityLevel.Error, message);
            //        throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
            //    }
            //}
            //else
            //{
            //    var message = $"Testing StorageAccount availability failed. Reason: Connection to StorageAccount could not be established.";
            //    AILogger.Log(SeverityLevel.Error, message);
            //    throw new ProvidenceException(message, HttpStatusCode.InternalServerError);
            //}
            return ResponseBuilder.CreateResponse(HttpStatusCode.Gone, null, SeverityLevel.Information, "Test case not available");
        }

        #endregion
    }
}