using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Configuration;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.SLA;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.BusinessLogic
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SlaCalculationManagerTest
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

        #region CalculateSlaAsync - DefaultCase: +++{++xxxxxx++}+++

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_DefaultCase()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error in 120Min  
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(75);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_DefaultCase()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error and 30Min Warning in 120Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(50);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 1: xxx{xxx++++++++}

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase1()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 12:15 until 13:15
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 12, 15, 0), new DateTime(2020, 1, 1, 13, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error in 60Min 
            result.ShouldNotBe(null);
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(75);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase1()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:45 until 13:15
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 45, 0), new DateTime(2020, 1, 1, 13, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error and 15Min Warning in 0.9Min
            result.ShouldNotBe(null);
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(50);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 2: {++++++++xxx}xxx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase2()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:15 until 12:15
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(75);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase2()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:45 until 12:45
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 45, 0), new DateTime(2020, 1, 1, 12, 45, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Warning in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(25);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 3: xxx{xxx+++++xxx}xxx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase3()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 11:00 until 11:30
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 0, 0), EndDate = new DateTime(2020, 1, 1, 11, 30, 0), State = State.Error, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:15 until 12:15
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(50);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase3()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:00 until 11:30
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 0, 0), EndDate = new DateTime(2020, 1, 1, 11, 30, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:15 until 12:15
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error and 15Min Warning in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(50);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 4: ---{----++++xxx}xxx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase4()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 11:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1, 11, 0, 0)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:00 until 12:15 -> Interval is 135Min but because of CreationDate at 11:00 the Interval should be only 60Min
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(75);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase4()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 11:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1, 11, 0, 0)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:00 until 12:15 -> Interval is 135Min but because of CreationDate at 11:00 the Interval should be only 60Min
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error and 30Min Warning in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(25);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 5: ---{----++++xxx}...

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase5()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 11:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1, 11, 0, 0)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 12:00 until null (ongoing)
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = null, State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:00 until 12:15 -> Interval is 135Min but because of CreationDate at 11:00 the Interval should be only 60Min
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(75);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase5()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 11:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1, 11, 0, 0)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until null (ongoing)
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = null, State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:00 until 12:15 -> Interval is 135Min but because of CreationDate at 11:00 the Interval should be only 60Min
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 15, 0), new DateTime(2020, 1, 1, 12, 15, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 15Min Error and 30Min Warning in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(25);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 6: xxx{xxxxxxxxxx}xxx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase6()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 11:00 until 13:00
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 00, 0), EndDate = new DateTime(2020, 1, 1, 13, 00, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 12:00 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 12, 0, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error in 120Min  
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(0);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase6()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:00 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 13:00
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 00, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 13, 00, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 30Min Error and 30Min Warning in 120Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Error);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(0);
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 7.1 (Element created after interval): ---{----------}-xx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase7_1()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 03/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 3)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // No StateTransitions in the defined interval
            var stateTransitionHistories = new List<StateTransitionHistory>();
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 12:00 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 12, 0, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> No SLA Data in 30Min  
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].ShouldBeNull();
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase7_1()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 03/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 3)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // No StateTransitions in the defined interval
            var stateTransitionHistories = new List<StateTransitionHistory>();
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 11:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 11, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> No SLA Data in 60Min
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].ShouldBeNull();
        }

        #endregion

        #region CalculateSlaAsync - SpecialCase 7.2 (No StateTransitions in interval): ---{----------}-xx

        [TestMethod]
        public async Task CalculateSlaAsync_OnlyError_SpecialCase7_2()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // No StateTransitions in the defined interval

            var stateTransitionHistories = new List<StateTransitionHistory>();
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 0Min Error in 120Min  
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Ok);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(100);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_SpecialCase7_2()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // No StateTransitions in the defined interval
            var stateTransitionHistories = new List<StateTransitionHistory>();
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            // Perform Tests -> 0Min Warning and 0Min Error in 120Min  
            result.ShouldNotBe(null);
            result.SlaDataPerElement[TestParameters.ElementId].Level.ShouldBe(SlaLevel.Ok);
            result.SlaDataPerElement[TestParameters.ElementId].Value.ShouldBe(100);
        }

        #endregion

        #region CalculateSlaAsync - History

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>{environment}));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            // There was no Error or Warning on the 02/01/2020
            // There was an Warning on the 03/01/2020 from 05:00 until 17:00
            // There was an Error on the 04/01/2020 from 22:00 until 23:30
            var stateTransitionHistories_01 = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            var stateTransitionHistories_02 = new List<StateTransitionHistory>();
            var stateTransitionHistories_03 = new List<StateTransitionHistory>
            {
                new StateTransitionHistory { EnvironmentId = 1, StartDate = new DateTime(2020, 1, 3, 5, 0, 0), EndDate = new DateTime(2020, 1, 3, 17, 0, 0), State = State.Warning, ElementId = TestParameters.ElementId },
            };
            var stateTransitionHistories_04 = new List<StateTransitionHistory>
            {
                new StateTransitionHistory { EnvironmentId = 1, StartDate = new DateTime(2020, 1, 4, 22, 0, 0), EndDate = new DateTime(2020, 1, 4, 23, 30, 0), State = State.Error, ElementId = TestParameters.ElementId }
            };

            _dataAccessLayer.SetupSequence(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(stateTransitionHistories_01))
                .Returns(Task.FromResult(stateTransitionHistories_01))
                .Returns(Task.FromResult(stateTransitionHistories_02))
                .Returns(Task.FromResult(stateTransitionHistories_03))
                .Returns(Task.FromResult(stateTransitionHistories_04));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 31/12/2019 00:00 until 04/01/2020 23:00 
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2019, 12, 31, 0, 0, 0), new DateTime(2020, 1, 4, 23, 00, 0), CancellationToken.None).ConfigureAwait(false);

            result.ShouldNotBe(null);
            result.SlaDataPerElementPerDay[TestParameters.ElementId].Length.ShouldBe(5);

            // Perform Tests -> 0Min Error in 1440Min  
            var element = result.SlaDataPerElementPerDay[TestParameters.ElementId][0];
            element.ShouldBe(null);

            // Perform Tests -> 30Min Error in 1440Min  
            element = result.SlaDataPerElementPerDay[TestParameters.ElementId][1];
            element.Value.ShouldBe(97.92);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][1].Level.ShouldBe(SlaLevel.Ok);

            // Perform Tests -> 0Min Error in 1440Min  
            element = result.SlaDataPerElementPerDay[TestParameters.ElementId][2];
            element.Value.ShouldBe(100);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][2].Level.ShouldBe(SlaLevel.Ok);

            // Perform Tests -> 720Min Warning in 1440Min  
            element = result.SlaDataPerElementPerDay[TestParameters.ElementId][3];
            element.Value.ShouldBe(50);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][3].Level.ShouldBe(SlaLevel.Error);

            // Perform Tests -> 60Min Error in 1380Min  
            element = result.SlaDataPerElementPerDay[TestParameters.ElementId][4];
            element.Value.ShouldBe(95.65);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][4].Level.ShouldBe(SlaLevel.Ok);
        }

        [TestMethod]
        public async Task CalculateSlaAsync_ErrorAndWarning_VeryShortPeriod()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Error on the 01/01/2020 from 23:00 until 23:30
            // There was an Error on the 02/01/2020 from 00:30 until 12:30
            var stateTransitionHistories_01 = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 23, 0, 0), EndDate = new DateTime(2020, 1, 1, 23, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            var stateTransitionHistories_02 = new List<StateTransitionHistory>
            {
                new StateTransitionHistory { EnvironmentId = 1, StartDate = new DateTime(2020, 1, 2, 0, 30, 0), EndDate = new DateTime(2020, 1, 2, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId },
            };

            _dataAccessLayer.SetupSequence(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(stateTransitionHistories_01))
                .Returns(Task.FromResult(stateTransitionHistories_01))
                .Returns(Task.FromResult(stateTransitionHistories_02));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 23:00 until 02/01/2020 01:00 
            var result = await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 23, 0, 0), new DateTime(2020, 1, 2, 1, 0, 0), CancellationToken.None).ConfigureAwait(false);

            result.ShouldNotBe(null);
            result.SlaDataPerElementPerDay[TestParameters.ElementId].Length.ShouldBe(2);

            // Perform Tests -> 30Min Error in 60Min  
            var value = result.SlaDataPerElementPerDay[TestParameters.ElementId][0].Value;
            value.ShouldBe(49.99);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][0].Level.ShouldBe(SlaLevel.Error);

            // Perform Tests -> 30Min Error in 60Min  
            value = result.SlaDataPerElementPerDay[TestParameters.ElementId][1].Value;
            value.ShouldBe(50);
            result.SlaDataPerElementPerDay[TestParameters.ElementId][1].Level.ShouldBe(SlaLevel.Error);
        }

        #endregion

        #region CalculateSlaAsync - Exception Cases

        [TestMethod]
        public async Task CalculateSlaAsync_NotFound_Environment()
        {
            // Environment was created on the 01/01/2020 at 00:00:00 was not found
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task CalculateSlaAsync_NotFound_CreationDate()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment>{ environment }));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task CalculateSlaAsync_InternalServerError()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironments(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetEnvironment> { environment }));

            // All Configurations are set correctly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // There was an Warning on the 01/01/2020 from 11:00 until unknown EndDate
            // There was an Error on the 01/01/2020 from 12:00 until 13:00
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 00, 0), EndDate = null, State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 13, 00, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task CalculateSlaAsync_InternalServerError_Configurations()
        {
            // warningThreshold set incorrectly
            var warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = null };
            var errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            // Mo Configurations available
            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }

            // errorThreshold set incorrectly
            warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = null };
            includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            // Mo Configurations available
            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }

            // includeWarnings set incorrectly
            warningThreshold = new GetConfiguration { Key = "SLA_Warning_Threshold", Value = "0.9" };
            errorThreshold = new GetConfiguration { Key = "SLA_Error_Threshold", Value = "0.8" };
            includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = null };

            // Mo Configurations available
            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(warningThreshold))
                .Returns(Task.FromResult(errorThreshold))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.CalculateSlaAsync(TestParameters.EnvironmentSubscriptionId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region GetRawSlaDataAsync

        [TestMethod]
        public async Task GetRawSlaDataAsync_OnlyError()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(environment));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "false" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));


            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);

            // Perform Tests -> 30Min Error in 120Min
            result[TestParameters.ElementId].CalculatedValue.ShouldBe(75);
            result[TestParameters.ElementId].DownTime.ShouldBe(new TimeSpan(0, 0, 30, 0));
            result[TestParameters.ElementId].IncludeWarnings.ShouldBeFalse();
            result[TestParameters.ElementId].RawData.Count.ShouldBe(2);
            result[TestParameters.ElementId].TimeInterval.ShouldBe(new TimeSpan(0, 2, 0, 0));
            result[TestParameters.ElementId].UpTime.ShouldBe(new TimeSpan(0, 1, 30, 0));
        }

        [TestMethod]
        public async Task GetRawSlaDataAsync_ErrorAndWarning()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(environment));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // All Configurations are set correctly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // There was an Warning on the 01/01/2020 from 11:30 until 12:00
            // There was an Error on the 01/01/2020 from 12:00 until 12:30
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 30, 0), EndDate = new DateTime(2020, 1, 1, 12, 00, 0), State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 12, 30, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(stateTransitionHistories));


            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            // Get SLA on the 01/01/2020 from 10:30 until 12:30
            var result = await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);

            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);

            // Perform Tests -> 30Min Error and 30Min Warning in 120Min
            result[TestParameters.ElementId].CalculatedValue.ShouldBe(50);
            result[TestParameters.ElementId].DownTime.ShouldBe(new TimeSpan(0, 1, 0, 0));
            result[TestParameters.ElementId].IncludeWarnings.ShouldBeTrue();
            result[TestParameters.ElementId].RawData.Count.ShouldBe(2);
            result[TestParameters.ElementId].TimeInterval.ShouldBe(new TimeSpan(0, 2, 0, 0));
            result[TestParameters.ElementId].UpTime.ShouldBe(new TimeSpan(0, 1, 0, 0));
        }

        #endregion

        #region GetRawSlaDataAsync - Exception Cases

        [TestMethod]
        public async Task GetRawSlaDataAsync_NotFound_Environment()
        {
            // Environment was created on the 01/01/2020 at 00:00:00 was not found
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // All Configurations are set correctly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetRawSlaDataAsync_NotFound_CreationDate()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(environment));

            // All Configurations are set correctly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetRawSlaDataAsync_InternalServerError()
        {
            // Environment was created on the 01/01/2020 at 00:00:00
            var environment = new GetEnvironment { Id = 1, CreateDate = new DateTime(2020, 1, 1), SubscriptionId = TestParameters.EnvironmentSubscriptionId };
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(environment));

            // All Configurations are set correctly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = "true" };

            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // The Element with TestParameters.ElementId "TestParameters.ElementId" was created on the 01/01/2020 at 00:00:00
            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>{new EnvironmentElement
                {
                    ElementId = TestParameters.ElementId,
                    EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                    ElementType = "Component",
                    CreationDate =  new DateTime(2020, 1, 1)
                }}));

            // There was an Warning on the 01/01/2020 from 11:00 until unknown EndDate
            // There was an Error on the 01/01/2020 from 12:00 until 13:00
            var stateTransitionHistories = new List<StateTransitionHistory>
            {
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 11, 00, 0), EndDate = null, State = State.Warning, ElementId = TestParameters.ElementId},
                new StateTransitionHistory{EnvironmentId = 1, StartDate= new DateTime(2020, 1, 1, 12, 0, 0), EndDate = new DateTime(2020, 1, 1, 13, 00, 0), State = State.Error, ElementId = TestParameters.ElementId}
            };
            _dataAccessLayer.Setup(mock => mock.GetStateTransitionHistoriesBetweenDates(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(stateTransitionHistories));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task GetRawSlaDataAsync_InternalServerError_Configurations()
        {
            // warningThreshold set incorrectly
            var includeWarnings = new GetConfiguration { Key = "SLA_Include_Warnings", Value = null };

            // Mo Configurations available
            _dataAccessLayer.SetupSequence(mock => mock.GetConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(includeWarnings));

            // Create BusinessLogic with Mock
            var slaCalculationManager = ManagerBuilder.CreateSlaCalculationManager(_dataAccessLayer.Object);

            try
            {
                // Perform Method to test
                await slaCalculationManager.GetRawSlaDataAsync(TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId, new DateTime(2020, 1, 1, 10, 30, 0), new DateTime(2020, 1, 1, 12, 30, 0), CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.InternalServerError);
            }
        }

        #endregion
    }
}
