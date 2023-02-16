using System;
using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Daimler.Providence.Tests.BusinessLogic
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AlertManagerTest
    {
        #region Private Members

        private MockEnvironmentManager _environmentManagerMock;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            // Create Mock
            _environmentManagerMock = new MockEnvironmentManager();
        }

        #endregion

        #region Tests

        [TestMethod]
        public void HandleAlertTest()
        {
            var alertManager = ManagerBuilder.CreateAlertManager(_environmentManagerMock);

            // Test Message
            var alertMessage = new AlertMessage
            {
                RecordId = Guid.NewGuid()
            };

            // Perform Method to test
            alertManager.HandleAlerts(new [] {alertMessage});

            // Perform Tests
            _environmentManagerMock.ReceivedAlertMessages.ShouldNotBeNull();
            _environmentManagerMock.ReceivedAlertMessages.Count.ShouldBe(1);
        }

        [TestMethod]
        public void HandleAlertsTest()
        {
            var alertManager = ManagerBuilder.CreateAlertManager(_environmentManagerMock);

            // Test Messages
            var alertMessage = new AlertMessage
            {
                RecordId = Guid.NewGuid()
            };
            var alertMessage2 = new AlertMessage
            {
                RecordId = Guid.NewGuid()
            };

            _environmentManagerMock.ReceivedAlertMessages.Clear();

            // Perform Method to test
            alertManager.HandleAlerts(new[] { alertMessage, alertMessage2 });

            // Perform Tests
            _environmentManagerMock.ReceivedAlertMessages.ShouldNotBeNull();
            _environmentManagerMock.ReceivedAlertMessages.Count.ShouldBe(2);
        }

        #endregion
    }
}
