using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EnvironmentControllerTest
    {
        #region Private Members

        private Mock<IEnvironmentManager> _businessLogic;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IEnvironmentManager>();
        }

        private EnvironmentController SetupController(IEnvironmentManager businessLogic, string queryString)
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

            var controller = new EnvironmentController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }

        #endregion

        #region GetEnvironmentTreeAsync Tests

        [TestMethod]
        public async Task GetEnvironmentTreeAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetEnvironment(It.IsAny<string>())).ReturnsAsync(new Environment());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.Showdemo}=true");
           
            // Perform Method to test
            var response = await controller.GetEnvironmentTreeAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetEnvironmentTreesAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetEnvironments()).ReturnsAsync(new List<Environment>());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.Showdemo}=true");

            // Perform Method to test
            var response = await controller.GetEnvironmentTreeAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetEnvironmentTreeAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetEnvironment(It.IsAny<string>())).ReturnsAsync((Environment)null);

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.Showdemo}=true");
            // Perform Method to test
            var response = await controller.GetEnvironmentTreeAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertNotFoundRequest(response);
        }

        #endregion

        #region GetStateManagerContentAsync Tests

        [TestMethod]
        public async Task GetStateManagerContentAsyncAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetStateManagerContent(It.IsAny<string>()))
                .ReturnsAsync(new StateManagerContent());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            var response = await controller.GetStateManagerContentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetStateManagerContentAsyncAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetStateManagerContent(It.IsAny<string>()))
                .ReturnsAsync((StateManagerContent)null);

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");
            // Perform Method to test
            var response = await controller.GetStateManagerContentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertNotFoundRequest(response);
        }

        [TestMethod]
        public async Task GetStateManagerContentAsyncAsync_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetStateManagerContent(It.IsAny<string>()))
                .ReturnsAsync((StateManagerContent)null);

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            var response = await controller.GetStateManagerContentAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
        }

        #endregion
    }
}