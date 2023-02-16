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
    public class ResetControllerTest
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
        public async Task ResetComponentsTreeAsyncTest_OneEnvironment()
        {
            // Create Controller with Mock
            var controller = new ResetController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.ResetEnvironmentTreeAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(response);
        }

        [TestMethod]
        public async Task ResetComponentsTreeAsyncTest_AllEnvironments()
        {
            // Create Controller with Mock
            var controller = new ResetController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.ResetEnvironmentTreeAsync(CancellationToken.None).ConfigureAwait(false);

            // Perform Tests
            TestHelper.AssertNoContentRequest(response);
        }

        [TestMethod]
        public async Task ResetComponentsTreeAsyncTest_BadRequestOnException()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.ResetAllEnvironments())
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new ResetController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.ResetEnvironmentTreeAsync(CancellationToken.None).ConfigureAwait(false);
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
