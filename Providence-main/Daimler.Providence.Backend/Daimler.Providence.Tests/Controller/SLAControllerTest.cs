using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models.SLA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SlaControllerTest
    {
        #region Private Members

        private Mock<ISlaCalculationManager> _businessLogic;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<ISlaCalculationManager>();
        }

        #endregion

        #region GetRawSla Tests
        
        [TestMethod]
        public async Task GetRawSlaAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetRawSlaDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateRawSlaDictionary()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}" );

            // Perform Method to test
            var result = await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}&{RequestParameters.ElementId}={TestParameters.ElementId}");
          
            // Perform Method to test -> OK on valid ElementId
            result = await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ElementId}");

            // Perform Method to test -> OK on valid ElementId and no StartDate and EndDate
            result = await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetRawSlaAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}");
          
            // Perform Method to test -> BadRequest on environmentSubscriptionId = null
            var result = await controller.GetRawSlaAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.FutureTime}&{RequestParameters.EndDate}={TestParameters.PastTime}");
          
            // Perform Method to test -> BadRequest on startDate >= endDate
            result = await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetRawSlaAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetRawSlaDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.FutureTime}&{RequestParameters.EndDate}={TestParameters.PastTime}");
          
            try
            {
                // Perform Method to test
                await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetRawSlaAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetRawSlaDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.FutureTime}&{RequestParameters.EndDate}={TestParameters.PastTime}");

            try
            {
                // Perform Method to test
                await controller.GetRawSlaAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion
        
        #region Private Methods

        private static Dictionary<string, SlaDataRaw> CreateRawSlaDictionary()
        {
            var slaDictionary = new Dictionary<string, SlaDataRaw>
            {
                { "Element1", new SlaDataRaw
                    {
                        CalculatedValue = 100,
                        IncludeWarnings = true,
                        UpTime = TestParameters.FutureTime-TestParameters.PastTime,
                        DownTime = new TimeSpan(0),
                        RawData = new List<StateTransitionHistory>{ new StateTransitionHistory()}
                    }
                },
                { "Element2", null }
            };
            return slaDictionary;
        }

        private SlaController SetupController(ISlaCalculationManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext();
            if (!string.IsNullOrEmpty(queryString))
            {
                httpContext.Request.QueryString = new QueryString(queryString);
            }
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new SlaController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }

        #endregion
    }
}
