using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Tests.BusinessLogic
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StateManagerTest
    {
        #region Private Members

        private const string AppSettingsFile = "appsettings.json";
        private Mock<IStorageAbstraction> _dataAccessLayer;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _dataAccessLayer = new Mock<IStorageAbstraction>();

            var builder = new ConfigurationBuilder()
                .AddJsonFile(AppSettingsFile, false, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            ProvidenceConfigurationManager.SetConfiguration(configuration);
        }

        #endregion

        #region Tests

        #region Intiliaization Tests

        [TestMethod]
        public void Initialization_Successful()
        {
            // Setup Mock
            SetupValidMock();

            var exceptionThrown = false;
            try
            {
                // Create BusinessLogic with Mock
                ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeFalse();
        }

        [TestMethod]
        public void Initialization_Failed()
        {
            var exceptionThrown = false;
            try
            {
                // Create BusinessLogic with Mock
                ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            exceptionThrown.ShouldBeTrue();
        }

        #endregion

        #region GetCurrentEnvironmentState Tests

        [TestMethod]
        public void GetCurrentEnvironmentState_Successful()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.Name.ShouldBe(TestParameters.EnvironmentName);
        }

        #endregion

        #region GetCurrentStateManagerContent Tests

        [TestMethod]
        public void GetCurrentStateManagerContent_Successful()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            var stateManagerContent = stateManager.GetCurrentStateManagerContent();
            stateManagerContent.EnvironmentName.ShouldBe(TestParameters.EnvironmentName);
        }

        #endregion

        #region GetQueuedAlertMessages Tests

        [TestMethod]
        public void GetQueuedAlertMessages_Successful()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            var queuedAlertMessages =stateManager.GetQueuedAlertMessages();
            queuedAlertMessages.ShouldNotBeNull();
        }

        #endregion

        #region RefreshEnvironment Tests

        [TestMethod]
        public async Task RefreshEnvironment_NoNewEnvironmentName()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.Name.ShouldBe(TestParameters.EnvironmentName);

            await stateManager.RefreshEnvironment().ConfigureAwait(false);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.Name.ShouldBe(TestParameters.EnvironmentName);
        }


        [TestMethod]
        public async Task RefreshEnvironment_NewEnvironmentName()
        {
            // Setup Mock
            SetupValidMock();

            // After the refresh the environmentTree should be updated
            _dataAccessLayer.SetupSequence(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateSimpleEnvironmentTree(TestParameters.EnvironmentName)))
                .Returns(Task.FromResult(CreateSimpleEnvironmentTree(TestParameters.EnvironmentName2)));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.Name.ShouldBe(TestParameters.EnvironmentName);

            await stateManager.RefreshEnvironment(TestParameters.EnvironmentName2).ConfigureAwait(false);
            
            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.Name.ShouldBe(TestParameters.EnvironmentName2);
        }

        #endregion

        #region ResetFrequencyChecks Tests

        [TestMethod]
        public async Task ResetFrequencyChecks_ResetAlertGenerated()
        {
            // Setup Mock
            SetupValidMock();

            // No stateTransitions to reset found
            _dataAccessLayer.Setup(mock => mock.GetChecksToReset(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<StateTransition> {
                    new StateTransition {
                        Frequency = 5,
                        SourceTimestamp = TestParameters.PastTime,
                        CheckId = TestParameters.CheckId,
                        ElementId = TestParameters.CheckElementId,
                        AlertName = TestParameters.AlertName

                }}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 1);
        }

        [TestMethod]
        public async Task ResetFrequencyChecks_ResetAlertNotGeneratedMultipleTimes()
        {
            // Setup Mock
            SetupValidMock();

            // No stateTransitions to reset found
            _dataAccessLayer.Setup(mock => mock.GetChecksToReset(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<StateTransition> {
                    new StateTransition {
                        Frequency = 5,
                        SourceTimestamp = TestParameters.PastTime,
                        CheckId = TestParameters.CheckId,
                        ElementId = TestParameters.ElementId,
                        AlertName = TestParameters.AlertName,
                        TriggeredByElementId = TestParameters.ElementId

                }}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            // Perform reset multiple times -> there should be only one alert in the queue
            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);
            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 1);
        }
     
        [TestMethod]
        public async Task ResetFrequencyChecks_InvalidFrequency()
        {
            // Setup Mock
            SetupValidMock();

            // No stateTransitions to reset found
            _dataAccessLayer.Setup(mock => mock.GetChecksToReset(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<StateTransition> {
                    new StateTransition {
                        Frequency = -1,
                        SourceTimestamp = TestParameters.CurrentTime,
                        CheckId = TestParameters.CheckId,
                        ElementId = TestParameters.ComponentElementId,
                        AlertName = TestParameters.AlertName
                }}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform Method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            // Trigger a reset of the environment
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            await stateManager.ResetFrequencyChecks().ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            // Check state after reset was executed
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);
        }

        #endregion

        #region HandleAlerts Tests

        [TestMethod]
        public async Task HandleAlerts_AlertEnqueued()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            // Perform reset multiple times -> there should be only one alert in the queue
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ServiceElementId
            };
            await stateManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 1);
        }

        [TestMethod]
        public async Task HandleAlerts_AlertIgnored()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetCurrentAlertIgnoreRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetAlertIgnoreRule> {
                    new GetAlertIgnoreRule {
                        Name = TestParameters.Name,
                        CreationDate = TestParameters.CurrentTime,
                        EnvironmentName = TestParameters.EnvironmentName,
                        EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                        ExpirationDate = TestParameters.FutureTime,
                        IgnoreCondition = new AlertIgnoreCondition
                        {
                            AlertName = TestParameters.AlertName,
                            CheckId = TestParameters.CheckId,
                            ComponentId = TestParameters.ServiceElementId
                        }
                }}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            // Perform reset multiple times -> there should be only one alert in the queue
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ServiceElementId
            };
            await stateManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);
        }

        [TestMethod]
        public async Task HandleAlerts_AlertIncreasedToError()
        {
            // Setup Mock
            SetupValidMock();

            // One Element in State Error which was increased from Warning to Error
            var stateDictionary = new Dictionary<string, List<StateTransition>>
            {
                {
                    TestParameters.ElementId,
                    new List<StateTransition> {
                        new StateTransition {
                             Frequency = 0,
                             SourceTimestamp = TestParameters.PastTime,
                             Description = TestParameters.IncreasedAlertDescription,
                             CheckId = TestParameters.CheckId,
                             ElementId = TestParameters.ServiceElementId,
                             AlertName = TestParameters.AlertName,
                             TriggeredByElementId = TestParameters.ServiceElementId,
                             TriggeredByAlertName = TestParameters.AlertName,
                             TriggeredByCheckId = TestParameters.CheckId,
                             ComponentType = TestParameters.ComponentType,
                             State = State.Error
                        }
                    }
                }
            };
            _dataAccessLayer.Setup(mock => mock.GetCurrentStates(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(stateDictionary));

            // On the db there is one active StateIncreaseRule
            _dataAccessLayer.Setup(mock => mock.GetActiveStateIncreaseRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetStateIncreaseRule> {
                    new GetStateIncreaseRule {
                        Name = TestParameters.Name,
                        Description = TestParameters.Description,
                        AlertName = TestParameters.AlertName,
                        CheckId = TestParameters.CheckId,
                        ComponentId = TestParameters.ServiceElementId,
                        EnvironmentName = TestParameters.EnvironmentName,
                        EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                        IsActive = true,
                        TriggerTime = 0
                }}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 0);

            // Perform method to test
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ServiceElementId
            };
            await stateManager.HandleAlerts(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetQueuedAlertMessages().ConfigureAwait(false), 1);

            var queuedAlertMessages = await stateManager.GetQueuedAlertMessages().ConfigureAwait(false);
            queuedAlertMessages[0].State.ShouldBe(State.Error); // State should be increased from Warning to Error
        }

        #endregion

        #region HandleAlertsInternal Tests

        [TestMethod]
        public async Task HandleAlertsInternal_HandleHeartbeatAlert_OldAlert()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.HeartbeatCheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ServiceElementId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.LastHeartBeat.ShouldBe(DateTime.MinValue);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_HandleHeartbeatAlert_NewAlert()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.HeartbeatCheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ServiceElementId,
                SourceTimestamp = TestParameters.CurrentTime
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.LastHeartBeat.ShouldBe(alertMessage.SourceTimestamp);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_InvalidCheckId()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = "Invalid",
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_RedToGreen()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId              
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Ok);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_RedToYellow()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false); 
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 3);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Warning);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_YellowToGreen()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 3);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Warning);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Ok);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_YellowToRed()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 3);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Warning);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_GreenToYellow()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Warning to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Warning,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 3);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Warning);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Warning);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_GreenToRed()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_ComplexTree_MultipleComponents_GreenToRedToGreen()
        {
            // Setup Mock
            SetupValidMock();

            var environmentTree = CreateComplexEnvironmentTree(TestParameters.EnvironmentName);
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(environmentTree));

            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironment(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>
                {
                        new EnvironmentElement{ElementId = TestParameters.EnvironmentSubscriptionId},
                        new EnvironmentElement{ElementId = TestParameters.ServiceElementId},
                        new EnvironmentElement{ElementId = TestParameters.ActionElementId},
                        new EnvironmentElement{ElementId = TestParameters.ActionElementId2},
                        new EnvironmentElement{ElementId = TestParameters.ComponentElementId},
                        new EnvironmentElement{ElementId = TestParameters.ComponentElementId2},
                        new EnvironmentElement{ElementId = TestParameters.CheckId},
                        new EnvironmentElement{ElementId = TestParameters.CheckId2}
                }));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentChecks(It.IsAny<string>()))
                .Returns(Task.FromResult(new Dictionary<string, Check> { { TestParameters.CheckId, CreateEnvironmentCheck(TestParameters.CheckId)}, { TestParameters.CheckId2, CreateEnvironmentCheck(TestParameters.CheckId2)}}));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 5);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 7);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[0].State.State.ShouldBe(State.Error);

            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 7);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[0].State.State.ShouldBe(State.Ok);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_ComplexTree_MultipleChecks_GreenToRedToGreen()
        {
            // Setup Mock
            SetupValidMock();

            var environmentTree = CreateComplexEnvironmentTree(TestParameters.EnvironmentName);
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(environmentTree));

            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironment(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new List<EnvironmentElement>
            {
                    new EnvironmentElement{ElementId = TestParameters.EnvironmentSubscriptionId},
                    new EnvironmentElement{ElementId = TestParameters.ServiceElementId},
                    new EnvironmentElement{ElementId = TestParameters.ActionElementId},
                    new EnvironmentElement{ElementId = TestParameters.ActionElementId2},
                    new EnvironmentElement{ElementId = TestParameters.ComponentElementId},
                    new EnvironmentElement{ElementId = TestParameters.ComponentElementId2},
                    new EnvironmentElement{ElementId = TestParameters.CheckId},
                    new EnvironmentElement{ElementId = TestParameters.CheckId2}
            }));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentChecks(It.IsAny<string>()))
                .Returns(Task.FromResult(new Dictionary<string, Check> { { TestParameters.CheckId, CreateEnvironmentCheck(TestParameters.CheckId) }, { TestParameters.CheckId2, CreateEnvironmentCheck(TestParameters.CheckId2) } }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system for first check
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId2,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.State.ShouldBe(State.Error);

            // Send Error to the system for second check
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId2,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId2,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 8);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.State.ShouldBe(State.Error);

            // Send Ok to the system for first check
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId2,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 8);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[1].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.State.ShouldBe(State.Error);

            // Send Ok to the system for second check
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId2,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId2,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);

            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 8);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[1].Components[1].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[0]?.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.ShouldNotBeNull();
            environmentState.Services[0]?.Actions[1]?.Components[1]?.Checks[1]?.State.State.ShouldBe(State.Ok);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_MultipleAlerts()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            // Send Error to the system
            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            var stateTransitions = await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            stateTransitions[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(1);
            stateTransitions[TestParameters.ServiceElementId].Count.ShouldBe(1);
            stateTransitions[TestParameters.ActionElementId].Count.ShouldBe(1);
            stateTransitions[TestParameters.ComponentElementId].Count.ShouldBe(1);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Ok);

            stateTransitions = await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            stateTransitions[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(2);
            stateTransitions[TestParameters.ServiceElementId].Count.ShouldBe(2);
            stateTransitions[TestParameters.ActionElementId].Count.ShouldBe(2);
            stateTransitions[TestParameters.ComponentElementId].Count.ShouldBe(2);

            // Send Error to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.ComponentElementId,
                SourceTimestamp = TestParameters.FutureTime.AddDays(1),
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false), 6);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[0]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[0].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            stateTransitions = await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            stateTransitions[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(3);
            stateTransitions[TestParameters.ServiceElementId].Count.ShouldBe(3);
            stateTransitions[TestParameters.ActionElementId].Count.ShouldBe(3);
            stateTransitions[TestParameters.ComponentElementId].Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task HandleAlertsInternal_OrphanElement()
        {
            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var stateManagerContent = stateManager.GetCurrentStateManagerContent();
            stateManagerContent.ShouldNotBeNull();
            stateManagerContent.EnvironmentTree.Services.Count.ShouldBe(1);
            stateManagerContent.ErrorStates.Count.ShouldBe(0);
            stateManagerContent.AllowedElementIds.Count.ShouldBe(5);

            var alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Error,
                ComponentId = TestParameters.OrphanElementId,
                SourceTimestamp = TestParameters.CurrentTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().EnvironmentTree.Services, 2);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 4);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().AllowedElementIds, 8);

            var environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Error);
            environmentState.Services[1]?.State.ShouldNotBeNull();
            environmentState.Services[1].State.State.ShouldBe(State.Error);
            environmentState.Services[1]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[1].Actions[0].State.State.ShouldBe(State.Error);
            environmentState.Services[1]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[1].Actions[0].Components[0].State.State.ShouldBe(State.Error);

            // Send Ok to the system
            alertMessage = new AlertMessage
            {
                AlertName = TestParameters.AlertName,
                CheckId = TestParameters.CheckId,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                ComponentId = TestParameters.OrphanElementId,
                SourceTimestamp = TestParameters.FutureTime,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            await stateManager.HandleAlertsInternal(new[] { alertMessage }).ConfigureAwait(false);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().EnvironmentTree.Services, 2);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().ErrorStates, 0);
            Awaiter.Await().AtMost(5).UntilProperty("Count", stateManager.GetCurrentStateManagerContent().AllowedElementIds, 8);

            environmentState = stateManager.GetCurrentEnvironmentState();
            environmentState.ShouldNotBeNull();
            environmentState.State.ShouldNotBeNull();
            environmentState.State.State.ShouldBe(State.Ok);
            environmentState.Services[1]?.State.ShouldNotBeNull();
            environmentState.Services[1].State.State.ShouldBe(State.Ok);
            environmentState.Services[1]?.Actions[0]?.State.ShouldNotBeNull();
            environmentState.Services[1].Actions[0].State.State.ShouldBe(State.Ok);
            environmentState.Services[1]?.Actions[0]?.Components[0]?.State.ShouldNotBeNull();
            environmentState.Services[1].Actions[0].Components[0].State.State.ShouldBe(State.Ok);
        }

        #endregion

        #region GetStateTransitionHistory Tests

        [TestMethod]
        public async Task GetStateTransitionHistory_CachedData_IncludeChecks()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime}}}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            history[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(1);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once());

        }

        [TestMethod]
        public async Task GetStateTransitionHistory_CachedData_ExcludeChecks()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime } }}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistory(false, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            history[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(0);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task GetStateTransitionHistory_DatabaseData_IncludeChecks()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime}}}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistory(true, TestParameters.PastTime.AddDays(-10), TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            history[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(1);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task GetStateTransitionHistory_DatabaseData_ExcludeChecks()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime } }}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistory(false, TestParameters.PastTime.AddDays(-10), TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            history[TestParameters.EnvironmentSubscriptionId].Count.ShouldBe(0);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        #endregion

        #region

        [TestMethod]
        public async Task GetStateTransitionHistoryByElementId_CachedData()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime}}}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistoryByElementId(TestParameters.EnvironmentSubscriptionId, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task GetStateTransitionHistoryByElementId_DatabaseData()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime}}}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistoryByElementId(TestParameters.EnvironmentSubscriptionId, TestParameters.PastTime.AddDays(-10), TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task GetStateTransitionHistoryByElementId_UnknownElementId()
        {
            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>
                {
                    {TestParameters.EnvironmentSubscriptionId, new List<StateTransition>{new StateTransition{ComponentType = "3", SourceTimestamp = TestParameters.CurrentTime}}}
                }));

            // Create BusinessLogic with Mock
            var stateManager = ManagerBuilder.CreateStateManager(_dataAccessLayer.Object, TestParameters.EnvironmentName);

            // Perform method to test
            var history = await stateManager.GetStateTransitionHistoryByElementId(TestParameters.EnvironmentSubscriptionId2, TestParameters.PastTime, TestParameters.FutureTime).ConfigureAwait(false);
            history.ShouldNotBeNull();
            history.Count.ShouldBe(0);
            _dataAccessLayer.Verify(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        #endregion

        #region Private Methods

        // Method used to Setup Mock for creating a valid StateManager
        // Overwrite the Setups in your TestMethod to get other results.
        private void SetupValidMock()
        {
            _dataAccessLayer.Setup(mock => mock.GetEnvironmentTree(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateSimpleEnvironmentTree(TestParameters.EnvironmentName)));

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
                        }}));

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
            
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistory(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Dictionary<string, List<StateTransition>>()));

            _dataAccessLayer.Setup(mock => mock.GetActiveStateIncreaseRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetStateIncreaseRule>()));

            _dataAccessLayer.Setup(mock => mock.GetActiveNotificationRulesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetNotificationRule>()));

            _dataAccessLayer.Setup(mock => mock.GetEnvironmentChecks(It.IsAny<string>()))
                .Returns(Task.FromResult(new Dictionary<string, Check> { { TestParameters.CheckId, CreateEnvironmentCheck(TestParameters.CheckId) } }));

            _dataAccessLayer.Setup(mock => mock.GetCurrentAlertIgnoreRules(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetAlertIgnoreRule>()));
        }

        private static Environment CreateSimpleEnvironmentTree(string environmentName)
        {
            var environmentTree = new Environment
            {
                Id = TestParameters.ValidId,
                Name = environmentName,
                Description = TestParameters.Description,
                ElementId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true,
                LastHeartBeat = TestParameters.PastTime,
                LogSystemState = State.Error,
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
                        Actions = new List<Service.Models.EnvironmentTree.Action>
                        {
                            new Service.Models.EnvironmentTree.Action
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
                                        ComponentType = TestParameters.ComponentType,
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

        private static Environment CreateComplexEnvironmentTree(string environmentName)
        {
            var environmentTree = new Environment
            {
                Id = TestParameters.ValidId,
                Name = environmentName,
                Description = TestParameters.Description,
                ElementId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true,
                LastHeartBeat = TestParameters.PastTime,
                LogSystemState = State.Error,
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
                        Actions = new List<Service.Models.EnvironmentTree.Action>
                        {
                            new Service.Models.EnvironmentTree.Action
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
                                        ComponentType = TestParameters.ComponentType,
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
                            },
                            new Service.Models.EnvironmentTree.Action
                            {
                                Id = TestParameters.ValidId,
                                Name = TestParameters.Name,
                                Description = TestParameters.Description,
                                ElementId = TestParameters.ActionElementId2,
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
                                        ComponentType = TestParameters.ComponentType,
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
                                    },
                                    new Component
                                    {
                                        Id = TestParameters.ValidId,
                                        Name = TestParameters.Name,
                                        Description = TestParameters.Description,
                                        ElementId = TestParameters.ComponentElementId2,
                                        CreateDate = TestParameters.PastTime,
                                        State = new StateTransition(),
                                        ComponentType = TestParameters.ComponentType,
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
                                            },
                                            new Check
                                            {
                                                Id = TestParameters.ValidId,
                                                Name = TestParameters.Name,
                                                Description = TestParameters.Description,
                                                ElementId = TestParameters.CheckId2,
                                                CreateDate = TestParameters.PastTime,
                                                Frequency =  TestParameters.Frequency,
                                                State = new StateTransition(),
                                            }
                                        },

                                    }
                                }
                            }
                        }
                    }
                }
            };
            return environmentTree;
        }

        private static Check CreateEnvironmentCheck(string elementId)
        {
            var environmentCheck = new Check
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = elementId,
                CreateDate = TestParameters.PastTime,
                Frequency = TestParameters.Frequency,
                State = new StateTransition()
            };
            return environmentCheck;
        }

        #endregion
    }
}