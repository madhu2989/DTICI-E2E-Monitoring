using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateTransition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AlertControllerTest
    {
        #region Private Members

        private Mock<IAlertManager> _businessLogic;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IAlertManager>();
        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task PostAlertAsyncTest()
        {
            var alertMessage = CreateAlertMessage();

            // Create Controller with Mock
            var controller = new AlertController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.PostAlertAsync(alertMessage).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(response);
        }

        [TestMethod]
        public async Task PostAlertAsyncTest_BadRequest()
        {
            var alertMessage = CreateAlertMessage();

            // Create Controller with Mock
            var controller = new AlertController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on invalid CheckId
            alertMessage.CheckId = null;
            var response = await controller.PostAlertAsync(alertMessage).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);

            // Perform Method to test -> BadRequest on invalid alert
            response = await controller.PostAlertAsync(null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task PostAlertAsyncTest_InternalServerError()
        {
            var alertMessage = CreateAlertMessage();

            // Setup Mock
            _businessLogic.Setup(mock => mock.HandleAlerts(It.IsAny<AlertMessage[]>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.PostAlertAsync(alertMessage).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        [TestMethod]
        public async Task GetQueuedAlertsAsyncTest()
        {
            // Create Controller with Mock
            _businessLogic.Setup(mock => mock.GetQueuedAlertMessagesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<AlertMessage>()));

            var controller = new AlertController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.GetQueuedAlertsAsync(TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        #endregion

        #region Private Methods

        private static AlertMessage CreateAlertMessage()
        {
            var alertMessage = new AlertMessage
            {
                RecordId = new Guid("00000000-0000-0000-0000-000000000000"),
                SourceTimestamp = DateTime.MinValue,
                TimeGenerated = DateTime.MinValue,
                ComponentId = TestParameters.ComponentElementId,
                CheckId = TestParameters.CheckId,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId,
                AlertName = TestParameters.AlertName,
                State = State.Ok
            };
            return alertMessage;
        }

        #endregion
    }
}