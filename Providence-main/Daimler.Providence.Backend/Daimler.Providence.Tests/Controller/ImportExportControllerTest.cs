using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ImportExport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using Action = Daimler.Providence.Service.Models.ImportExport.Action;
using Environment = Daimler.Providence.Service.Models.ImportExport.Environment;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ImportExportControllerTest
    {
        #region Private Members

        private Mock<IImportExportManager> _businessLogic;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IImportExportManager>();
        }

        #endregion
       
        #region ExportEnvironmentAsync Tests

        [TestMethod]
        public async Task ExportEnvironmentAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.ExportEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateEnvironment()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}");
           
            // Perform Method to test
            var result = await controller.ExportEnvironmentAsync(CancellationToken.None);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task ExportEnvironmentAsync_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.ExportEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateEnvironment()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}=");
          
            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            var result = await controller.ExportEnvironmentAsync(CancellationToken.None);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task ExportEnvironmentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.ExportEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}");
         
            try
            {
                // Perform Method to test
                await controller.ExportEnvironmentAsync(CancellationToken.None);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task ExportEnvironmentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.ExportEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}");
         
            try
            {
                // Perform Method to test
                await controller.ExportEnvironmentAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region ImportEnvironmentAsnyc Tests

        [TestMethod]
        public async Task ImportEnvironmentAsync_OK()
        {
            var newEnvironment = CreateEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.ImportEnvironmentAsync(It.IsAny<Environment>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReplaceFlag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<string>>()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.InstanceName}={TestParameters.EnvironmentName}&{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}&{RequestParameters.Replace}={ReplaceFlag.True}");

            // Perform Method to test
            var result = await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task ImportEnvironmentAsync_BadRequest()
        {
            var newEnvironment = CreateEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.ImportEnvironmentAsync(It.IsAny<Environment>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReplaceFlag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<string>>()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.InstanceName}=&{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}&{RequestParameters.Replace}={ReplaceFlag.True}");
        
            // Perform Method to test -> BadRequest on instanceName = ""
            var result = await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            TestHelper.AssertBadRequest(result);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.InstanceName}={TestParameters.EnvironmentName}&{RequestParameters.EnvironmentSubscriptionId}=&{RequestParameters.Replace}={ReplaceFlag.True}");
         
            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            TestHelper.AssertBadRequest(result);

            ///// Error on validation
            
            _businessLogic.Setup(mock => mock.ImportEnvironmentAsync(It.IsAny<Environment>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReplaceFlag>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<string>>{{TestParameters.ActionElementId, new List<string>{"Error on validation."}}}));

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.InstanceName}={TestParameters.EnvironmentName}&{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}&{RequestParameters.Replace}={ReplaceFlag.True}");

            // Perform Method to test -> BadRequest validation error
            result = await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            TestHelper.AssertBadRequest(result);

        }

        [TestMethod]
        public async Task ImportEnvironmentAsync_NotFound()
        {
            var newEnvironment = CreateEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.ImportEnvironmentAsync(It.IsAny<Environment>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReplaceFlag>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}");
         
            try
            {
                // Perform Method to test
                await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task ImportEnvironmentAsync_InternalServerError()
        {
            var newEnvironment = CreateEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.ImportEnvironmentAsync(It.IsAny<Environment>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReplaceFlag>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.EnvironmentSubscriptionId}={TestParameters.EnvironmentSubscriptionId}");

            try
            {
                // Perform Method to test
                await controller.ImportEnvironmentAsync(CancellationToken.None, newEnvironment);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion
        
        #region Private Methods

        private ImportExportController SetupController(IImportExportManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            httpContext.Request.QueryString = new QueryString(queryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new ImportExportController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }

        private static Environment CreateEnvironment()
        {
            var exportEnvironment = new Environment
            {
                Services = new List<Service.Models.ImportExport.Service>(),
                Actions = new List<Action>(),
                Components = new List<Component>(),
                Checks = new List<Check>()
            };
            return exportEnvironment;
        }

        #endregion
    }
}