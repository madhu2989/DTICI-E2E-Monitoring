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
    public class LicenseInformationControllerTest
    {
        #region Private Members

        private Mock<ILicenseInformationManager> _businessLogic;
        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<ILicenseInformationManager>();
        }

        #endregion

        #region GetLicenseInformationAsync Tests

        [TestMethod]
        public async Task GetLicenseInformationAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetLicenseInformationAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<LicenseInformation>{new LicenseInformation()}));

            var controller = SetupController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetLicenseInformationAsync(new CancellationToken());
            TestHelper.AssertContentRequest(result);
        }

        [TestMethod]
        public async Task GetLicenseInformationAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetLicenseInformationAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(new List<LicenseInformation>()));


            // Create controller with Mock
            var controller = SetupController(_businessLogic.Object);
            try
            {
                // Perform Method to test
                await controller.GetLicenseInformationAsync(new CancellationToken());
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetLicenseInformationAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetLicenseInformationAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();
          
            var controller = SetupController(_businessLogic.Object);
            try
            {
                // Perform Method to test
                await controller.GetLicenseInformationAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private LicenseInformationController SetupController(ILicenseInformationManager businessLogic)
        {
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            var controller = new LicenseInformationController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }
      
        #endregion
    }
}
