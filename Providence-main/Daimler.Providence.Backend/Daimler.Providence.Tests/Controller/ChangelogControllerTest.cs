using Daimler.Providence.Service.Models.ChangeLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ChangeLogControllerTest
    {
        #region Private Members

        private Mock<IMasterdataManager> _businessLogic;
        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IMasterdataManager>();
        }

        #endregion

        #region GetChangeLogsAsync Tests

        [TestMethod]
        public async Task GetChangeLogAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChangeLogAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateValidChangeLogs().First()));

            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ChangeLogId}={TestParameters.ValidId}");

            // Perform Method to test
            var result = await controller.GetChangeLogsAsync(new CancellationToken());
            TestHelper.AssertContentRequest(result);
        }
        
        [TestMethod]
        public async Task GetChangeLogsAsync_OK()
        {
            // Create controller with Mock -> with elementId param
            _businessLogic.Setup(mock => mock.GetChangeLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateValidChangeLogs()));
            
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ElementId}={TestParameters.ValidId}");
            
                // Perform Method to test
            var result = await controller.GetChangeLogsAsync(new CancellationToken());

            var changeLogs = TestHelper.AssertContentRequestList(result, 2);

            // Create controller with Mock -> with environmentName param
            _businessLogic.Setup(mock => mock.GetChangeLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateValidChangeLogs()));

            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.EnvironmentName}={TestParameters.EnvironmentName}");
            
              // Perform Method to test
            result = await controller.GetChangeLogsAsync(new CancellationToken());
            TestHelper.AssertContentRequestList(result, 2);

            // Create controller with Mock -> without elementId and environmentName param
            _businessLogic.Setup(mock => mock.GetChangeLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateValidChangeLogs()));

            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}");
           
            // Perform Method to test
            result = await controller.GetChangeLogsAsync(new CancellationToken());
            result.ShouldNotBe(null);
            TestHelper.AssertContentRequestList(result, 2);
        }

        [TestMethod]
        public async Task GetChangeLogsAsync_BadRequest()
        {
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.FutureTime}&{RequestParameters.EndDate}={TestParameters.PastTime}&{RequestParameters.ChangeLogId}={TestParameters.ValidId}");
            
           // Perform Method to test -> BadRequest on startDate >= endDate
            var result = await controller.GetChangeLogsAsync(new CancellationToken());
            result.ShouldNotBe(null);
            TestHelper.AssertBadRequest(result);

            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ChangeLogId}={TestParameters.InvalidId}");
             // Perform Method to test -> BadRequest on changeLogId <= 0
            result = await controller.GetChangeLogsAsync(new CancellationToken());
            TestHelper.AssertBadRequest(result);

            // Create controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ElementId}={TestParameters.InvalidId}");

            // Perform Method to test -> BadRequest on elementId <= 0
            result = await controller.GetChangeLogsAsync(new CancellationToken());
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetChangeLogsAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChangeLogAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });
            _businessLogic.Setup(mock => mock.GetChangeLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ChangeLogId}={TestParameters.ValidId}");
            try
            {
                // Perform Method to test
                await controller.GetChangeLogsAsync(new CancellationToken());
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ElementId}={TestParameters.ValidId}");
            try
            {
                // Perform Method to test
                await controller.GetChangeLogsAsync(new CancellationToken());
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetChangeLogsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChangeLogAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();
            _businessLogic.Setup(mock => mock.GetChangeLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ChangeLogId}={TestParameters.ValidId}");

            try
            {
                // Perform Method to test
                await controller.GetChangeLogsAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ElementId}={TestParameters.ValidId}");
            try
            {
                // Perform Method to test
                await controller.GetChangeLogsAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private ChangeLogController SetupController(IMasterdataManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            httpContext.Request.QueryString = new QueryString(queryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new ChangeLogController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }


        private static List<GetChangeLog> CreateValidChangeLogs()
        {
            var changeLogs = new List<GetChangeLog>
            {
                new GetChangeLog
                {
                    Id = 1,
                    EnvironmentName = TestParameters.EnvironmentName,
                    ElementId = TestParameters.ValidId,
                    ElementType = "Deployment",
                    ChangeDate = TestParameters.PastTime,
                    Username = TestParameters.User,
                    Operation = "Create",
                    ValueOld = null,
                    ValueNew = null,
                    Diff = null
                },
                new GetChangeLog
                {
                    Id = 2,
                    EnvironmentName = TestParameters.EnvironmentName,
                    ElementId = TestParameters.ValidId,
                    ElementType = "Deployment",
                    ChangeDate = TestParameters.PastTime,
                    Username = TestParameters.User,
                    Operation = "Create",
                    ValueOld = null,
                    ValueNew = null,
                    Diff = null
                }
            };
            return changeLogs;
        }

        #endregion
    }
}
