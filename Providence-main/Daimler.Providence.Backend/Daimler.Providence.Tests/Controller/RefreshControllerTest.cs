using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RefreshControllerTest
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

        #endregion
        
        #region Tests

        [TestMethod]
        public async Task RefreshComponentTreeAsync_NoContent()
        {
            // Create Controller with Mock
            var controller = new RefreshController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.RefreshEnvironmentTreeAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(response);
        }

        [TestMethod]
        public async Task RefreshComponentTreesAsync_NoContent()
        {
            // Create Controller with Mock
            var controller = new RefreshController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.RefreshEnvironmentTreeAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(response);
        }

        [TestMethod]
        public async Task RefreshComponentsTreeAsync_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.RefreshAllEnvironments()).Throws<Exception>();

            // Create Controller with Mock
            var controller = new RefreshController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.RefreshEnvironmentTreeAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        
    }
}
