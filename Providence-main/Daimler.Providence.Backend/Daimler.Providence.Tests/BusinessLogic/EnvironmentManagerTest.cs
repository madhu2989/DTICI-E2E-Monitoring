using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using Action = Daimler.Providence.Service.Models.EnvironmentTree.Action;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Tests.BusinessLogic
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EnvironmentManagerTest
    {
        #region Private Members

        private Mock<IStorageAbstraction> _dataAccessLayer;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _dataAccessLayer = new Mock<IStorageAbstraction>();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();

            var Configuration = builder.Build();
            ProvidenceConfigurationManager.SetConfiguration(Configuration);
        }

        #endregion

        #region HandleAlerts Tests

        [TestMethod]
        public async Task HandleAlerts_HandledForKnownEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var alertMessage = CreateAlertMessage();
            await businessLogic.HandleAlerts(new[] { alertMessage });

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        [TestMethod]
        public async Task HandleAlerts_NotHandledForUnknownEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty list
            // -> No Environments found on DB and therefore no StateManager is created
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>()));

            // Mock returns empty list
            // -> No Environments found on DB
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentSubscriptionIds(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<string>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var alertMessage = CreateAlertMessage();
            await businessLogic.HandleAlerts(new[] { alertMessage });

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.ShouldBeNull();
        }

        [TestMethod]
        public async Task HandleAlerts_HandledForUnknownEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty list
            // -> No Environments found on DB and therefore no StateManager is created
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var alertMessage = CreateAlertMessage();
            await businessLogic.HandleAlerts(new[] { alertMessage });

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        #endregion

        #region GetEnvironment Tests

        [TestMethod]
        public async Task GetEnvironment_ExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        [TestMethod]
        public async Task GetEnvironment_UnknownEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty string
            // -> No Environment with provided EnvironmentName exists on DB and therefore no StateManager is found
            _dataAccessLayer.Setup(mock => mock.GetSubscriptionIdByEnvironmentName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(string.Empty));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var result = await businessLogic.GetEnvironment("Unknown");
            result.ShouldBe(null);
        }

        #endregion

        #region GetEnvironments Tests

        [TestMethod]
        public async Task GetEnvironments_ExistingEnvironments()
        {
            // Setup Mock 
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var result = await businessLogic.GetEnvironments();
            result[0].Services.Count.ShouldBe(1);
            result[0].Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result[0].Services[0].Actions.Count.ShouldBe(1);
            result[0].Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result[0].Services[0].Actions[0].Components.Count.ShouldBe(1);
            result[0].Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result[0].Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result[0].Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        [TestMethod]
        public async Task GetEnvironments_NoExistingEnvironments()
        {
            // Setup Mock 
            SetupValidMock();

            // Mock returns empty list
            // -> No Environments found on DB and therefore no StateManager is created
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var result = await businessLogic.GetEnvironments();
            result.Count.ShouldBe(0);
        }

        #endregion

        #region GetStateManagerContent Tests

        [TestMethod]
        public async Task GetStateManagerContent_ExistingEnvironment()
        {
            // Setup Mock 
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            var result = await businessLogic.GetStateManagerContent(TestParameters.EnvironmentSubscriptionId);
            result.ShouldNotBeNull();
            result.EnvironmentName.ShouldBe(TestParameters.EnvironmentName);
            result.AllowedElementIds.Count.ShouldBe(5);
            result.EnvironmentChecks.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task GetStateManagerContent_UnknownEnvironment()
        {
            // Setup Mock 
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.GetStateManagerContent("Unknown");
            }
            catch (ProvidenceException pe)
            {
                exceptionThrown = true;
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
            exceptionThrown.ShouldBeTrue();
        }

        #endregion

        #region RefreshEnvironment Tests

        [TestMethod]
        public async Task RefreshEnvironment_ExistingEnvironment()
        {
            // Setup Mock 
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.RefreshEnvironment(TestParameters.EnvironmentSubscriptionId);
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        [TestMethod]
        public async Task RefreshEnvironment_UnknownEnvironment()
        {
            // Setup Mock 
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.RefreshEnvironment("Unknown");
            }
            catch (ProvidenceException pe)
            {
                exceptionThrown = true;
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
            exceptionThrown.ShouldBeTrue();
        }

        [TestMethod]
        public async Task RefreshEnvironment_NoExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty list
            // -> Environment on DB was deleted after StateManager was created
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentSubscriptionIds(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<string>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.RefreshEnvironment(TestParameters.EnvironmentSubscriptionId);
            }
            catch (ProvidenceException pe)
            {
                exceptionThrown = true;
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
            exceptionThrown.ShouldBeTrue();
        }

        #endregion

        #region RefreshAllEnvironments Tests

        [TestMethod]
        public async Task RefreshAllEnvironments_ExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.RefreshAllEnvironments();
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        [TestMethod]
        public async Task RefreshAllEnvironments_NoExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty list
            // -> Environment on DB was deleted after StateManager was created
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentSubscriptionIds(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<string>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.RefreshAllEnvironments();
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        #endregion

        #region ResetEnvironment Tests

        [TestMethod]
        public async Task ResetEnvironment_ExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.ResetEnvironment(TestParameters.EnvironmentSubscriptionId);
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        [TestMethod]
        public async Task ResetEnvironment_UnknownEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.ResetEnvironment("Unknown");
            }
            catch (ProvidenceException pe)
            {
                exceptionThrown = true;
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
            exceptionThrown.ShouldBeTrue();
        }

        #endregion

        #region ResetAllEnvironments Tests

        [TestMethod]
        public async Task ResetAllEnvironments_ExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.ResetAllEnvironments();
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        [TestMethod]
        public async Task ResetAllEnvironments_NoExistingEnvironment()
        {
            // Setup Mock -> Environment on DB was deleted after StateManager was created
            SetupValidMock();

            // Mock returns empty list
            // -> Environment on DB was deleted after StateManager was created
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentSubscriptionIds(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<string>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var exceptionThrown = false;
            try
            {
                // Perform Method to test
                await businessLogic.ResetAllEnvironments();
            }
            catch (ProvidenceException)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        #endregion

        #region CreateStateManager Tests

        [TestMethod]
        public async Task CreateStateManager_CreatedForNewEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns empty list
            // -> No Environments found on DB and therefore no StateManager is created
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            await businessLogic.CreateStateManager(TestParameters.EnvironmentSubscriptionId);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        [TestMethod]
        public async Task CreateStateManager_NotCreatedForUnknownEnvironment()
        {
            // Setup Mock -> No Environments in DB when initializing EnvironmentManager
            SetupValidMock();

            // Mock returns empty list
            // -> No Environments found on DB and therefore no StateManager is created
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>()));

            // Mock returns empty string
            // -> No Environment with provided EnvironmentName exists on DB and therefore no StateManager is found
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentNameBySubscriptionId(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(string.Empty));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            // Perform Method to test
            await businessLogic.CreateStateManager(TestParameters.EnvironmentSubscriptionId);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.ShouldBeNull();
        }

        #endregion

        #region UpdateStateManager Tests

        [TestMethod]
        public async Task UpdateStateManager_UpdatedForExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Mock returns the whole EnvironmentTree the first time and an updated one the second time
            // -> StateManager for whole Tree is created and then the StateManager is updated used the updated Tree
            _dataAccessLayer.SetupSequence(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateEnvironmentTree()))
                .Returns(Task.FromResult(CreateUpdatedEnvironmentTree()));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);

            // Perform Method to test
            await businessLogic.UpdateStateManager(TestParameters.EnvironmentSubscriptionId);

            result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(0);
        }

        [TestMethod]
        public async Task UpdateStateManager_NotUpdatedForUnknownEnvironment()
        {
            // Setup Mock -> No Environments in DB when initializing EnvironmentManager
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);

            // Perform Method to test
            await businessLogic.UpdateStateManager("Unknown");

            result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        #endregion

        #region DeleteStateManager Tests

        [TestMethod]
        public async Task DeleteStateManager_DeletedForExistingEnvironment()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);

            // Perform Method to test
            await businessLogic.DeleteStateManager(TestParameters.EnvironmentSubscriptionId);

            result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.ShouldBeNull();
        }

        [TestMethod]
        public async Task DeleteStateManager_NotDeletedForUnknownEnvironment()
        {
            // Setup Mock -> No Environments in DB when initializing EnvironmentManager
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);

            // Perform Method to test
            await businessLogic.DeleteStateManager("Unknown");

            result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);
        }

        #endregion

        #region DisposeEnvironments Tests

        [TestMethod]
        public async Task DisposeEnvironments_DeletedEnvironments()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateEnvironmentManager(_dataAccessLayer.Object);

            var result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.Services.Count.ShouldBe(1);
            result.Services[0].ElementId.ShouldBe(TestParameters.ServiceElementId);
            result.Services[0].Actions.Count.ShouldBe(1);
            result.Services[0].Actions[0].ElementId.ShouldBe(TestParameters.ActionElementId);
            result.Services[0].Actions[0].Components.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].ElementId.ShouldBe(TestParameters.ComponentElementId);
            result.Services[0].Actions[0].Components[0].Checks.Count.ShouldBe(1);
            result.Services[0].Actions[0].Components[0].Checks[0].ElementId.ShouldBe(TestParameters.CheckId);

            // Perform Method to test
            await businessLogic.DisposeEnvironments();

            result = await businessLogic.GetEnvironment(TestParameters.EnvironmentName);
            result.ShouldBeNull();
        }

        #endregion

        #region Private Methods

        // Method used to Setup Mock for creating a valid StateManager
        // Overwrite the Setups in your TestMethod to get other results.
        private void SetupValidMock()
        {
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { CreateGetEnvironment() }));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateEnvironmentTree()));

            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironment(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>
                {
                    new EnvironmentElement{ElementId = TestParameters.EnvironmentSubscriptionId},
                    new EnvironmentElement{ElementId = TestParameters.ServiceElementId},
                    new EnvironmentElement{ElementId = TestParameters.ActionElementId},
                    new EnvironmentElement{ElementId = TestParameters.ComponentElementId},
                    new EnvironmentElement{ElementId = TestParameters.CheckId}
                }));

            _dataAccessLayer.Setup(mock => mock.GetCurrentDeployments(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetDeployment>{
                        new GetDeployment
                        {
                            Id = TestParameters.ValidId,
                            EnvironmentName = TestParameters.EnvironmentName,
                            EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                            Description = TestParameters.Description,
                            ShortDescription = TestParameters.ShortDescription,
                            CloseReason = TestParameters.CloseReason,
                            StartDate = TestParameters.PastTime,
                            EndDate = TestParameters.FutureTime,
                            Length = (int)TestParameters.FutureTime.Subtract(TestParameters.PastTime).TotalMilliseconds
                        }}
                ));

            _dataAccessLayer.Setup(mock => mock.GetFutureDeployments(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetDeployment>
                {
                    new GetDeployment
                    {
                        Id = TestParameters.ValidId,
                        EnvironmentName = TestParameters.EnvironmentName,
                        EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                        Description = TestParameters.Description,
                        ShortDescription = TestParameters.ShortDescription,
                        CloseReason = TestParameters.CloseReason,
                        StartDate = TestParameters.FutureTime,
                        EndDate = TestParameters.FutureTime.AddDays(1),
                        Length = (int)TestParameters.FutureTime.Subtract(TestParameters.PastTime).TotalMilliseconds
                    }
                }));

            _dataAccessLayer.Setup(mock => mock.GetCurrentStates(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>()));

            _dataAccessLayer.Setup(mock => mock.GetActiveStateIncreaseRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetStateIncreaseRule>()));

            _dataAccessLayer.Setup(mock => mock.GetActiveNotificationRulesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetNotificationRule>()));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentChecks(It.IsAny<string>()))
                .Returns(Task.FromResult(new Dictionary<string, Check> { { TestParameters.CheckId, CreateEnvironmentCheck() } }));

            _dataAccessLayer.Setup(mock => mock.GetSubscriptionIdByEnvironmentName(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(TestParameters.EnvironmentSubscriptionId));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentNameBySubscriptionId(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(TestParameters.EnvironmentName));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentSubscriptionIds(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<string> { TestParameters.EnvironmentSubscriptionId }));

            _dataAccessLayer.Setup(mock => mock.GetCurrentAlertIgnoreRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetAlertIgnoreRule>()));

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>()));
        }

        private static GetEnvironment CreateGetEnvironment()
        {
            var getEnvironment = new GetEnvironment
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true,
                Checks = new List<string>(),
                Services = new List<string> { TestParameters.ServiceElementId }
            };
            return getEnvironment;
        }

        private static Environment CreateEnvironmentTree()
        {
            var environmentTree = new Environment
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                ElementId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true,
                LastHeartBeat = TestParameters.PastTime,
                LogSystemState = State.Ok,
                State = new StateTransition(),
                Checks = new List<Check>(),
                Services = new List<Service.Models.EnvironmentTree.Service>
                {
                    new Service.Models.EnvironmentTree.Service
                    {
                        Id = TestParameters.ValidId,
                        Name = TestParameters.Name,
                        Description = TestParameters.Description,
                        ElementId = TestParameters.ServiceElementId,
                        CreateDate = TestParameters.PastTime,
                        State = new StateTransition(),
                        Checks = new List<Check>(),
                        Actions = new List<Action>
                        {
                            new Action
                            {
                                Id = TestParameters.ValidId,
                                Name = TestParameters.Name,
                                Description = TestParameters.Description,
                                ElementId = TestParameters.ActionElementId,
                                CreateDate = TestParameters.PastTime,
                                State = new StateTransition(),
                                Checks = new List<Check>(),
                                Components = new List< Component>
                                {
                                    new Component
                                    {
                                        Id = TestParameters.ValidId,
                                        Name = TestParameters.Name,
                                        Description = TestParameters.Description,
                                        ElementId = TestParameters.ComponentElementId,
                                        CreateDate = TestParameters.PastTime,
                                        State = new StateTransition(),
                                        Checks = new List<Check>
                                        {
                                            new Check
                                            {
                                                Id = TestParameters.ValidId,
                                                Name = TestParameters.Name,
                                                Description = TestParameters.Description,
                                                ElementId = TestParameters.CheckId,
                                                CreateDate = TestParameters.PastTime,
                                                Frequency =  TestParameters.Frequency,
                                                State = new StateTransition(),
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            return environmentTree;
        }

        private static Environment CreateUpdatedEnvironmentTree()
        {
            var environmentTree = new Environment
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                ElementId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true,
                LastHeartBeat = TestParameters.PastTime,
                LogSystemState = State.Ok,
                State = new StateTransition(),
                Checks = new List<Check>(),
                Services = new List<Service.Models.EnvironmentTree.Service>
                {
                    new Service.Models.EnvironmentTree.Service
                    {
                        Id = TestParameters.ValidId,
                        Name = TestParameters.Name,
                        Description = TestParameters.Description,
                        ElementId = TestParameters.ServiceElementId,
                        CreateDate = TestParameters.PastTime,
                        State = new StateTransition(),
                        Checks = new List<Check>(),
                        Actions = new List<Action>()
                    }
                }
            };
            return environmentTree;
        }

        private static Check CreateEnvironmentCheck()
        {
            var environmentCheck = new Check
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.CheckId,
                CreateDate = TestParameters.PastTime,
                Frequency = TestParameters.Frequency,
                State = new StateTransition()
            };
            return environmentCheck;
        }

        private static AlertMessage CreateAlertMessage()
        {
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.Name,
                RecordId = TestParameters.ValidRecordId,
                CheckId = TestParameters.CheckId,
                Description = TestParameters.Description,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId,
                SourceTimestamp = TestParameters.PastTime,
                TimeGenerated = TestParameters.PastTime,
                State = State.Error
            };
            return alertMessage;
        }

        #endregion
    }
}
