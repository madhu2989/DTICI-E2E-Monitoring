using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateTransition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class HistoryControllerTest
    {
        #region Private Members

        private Mock<IMasterdataManager> _masterDataManager;
        private Mock<IEnvironmentManager> _environmentManager;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _masterDataManager = new Mock<IMasterdataManager>();
            _environmentManager = new Mock<IEnvironmentManager>();
        }

        #endregion

        private HistoryController SetupController(IEnvironmentManager _environmentManager, IMasterdataManager _masterDataManager, string queryString)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            httpContext.Request.QueryString = new QueryString(queryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new HistoryController(_environmentManager, _masterDataManager)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }

        #region GetStateTransitionsHistoryAsync Tests
        [TestMethod]
        public async Task GetStateTransitionsHistoryAsync_OK()
        {
            // Setup Mock
            _environmentManager.Setup(mock => mock.GetStateTransitionHistoryAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(CreateStateTransitionHistory());
            _environmentManager.Setup(mock => mock.GetStateTransitionHistoryByElementIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(CreateStateTransitionHistoryByElementId());

            // Create Controller with Mock -> Without ElementId
            var controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.IncludeChecks}=true");
          
            // Perform Method to test
            var result = await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            // Create Controller with Mock -> Without ElementId
            controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.ElementId}={TestParameters.ElementId}");

          
            // Perform Method to test -> With ElementId
            result = await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            

            var history = (List<StateTransition>)TestHelper.AssertContentRequestType(result, typeof(List<StateTransition>));

            
            history.ShouldNotBeNull();
            history.Count.ShouldBe(1);
            history[0].ElementId.ShouldBe(TestParameters.ElementId);
        
            // Create Controller with Mock -> Extended Model
           
            controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.ExtendedModel}=true");


            // Perform Method to test
            result = await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            
            var extendedHistory = (Dictionary<string, List<StateTransition>>)TestHelper.AssertContentRequestType(result, typeof(Dictionary<string, List<StateTransition>>));


            extendedHistory.ShouldNotBeNull();
            extendedHistory.Count.ShouldBe(1);
            extendedHistory[TestParameters.EnvironmentSubscriptionId][0].ElementId.ShouldBe(TestParameters.ElementId);
        }
        
        [TestMethod]
        public async Task GetStateTransitionsHistoryAsync_BadRequest()
        {
            // Setup Mock
            _environmentManager.Setup(mock => mock.GetStateTransitionHistoryAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(CreateStateTransitionHistory());

            // Create Controller with Mock
            var controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.CurrentTime}&{RequestParameters.EndDate}={TestParameters.PastTime}&{RequestParameters.ExtendedModel}=true" );

         
            // Perform Method to test -> BadRequest on startDate >= endDate
            var result = await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);


            // Create Controller with Mock
            controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.ExtendedModel}=true" );

        
            // Perform Method to test -> BadRequest on environmentName = ""
            result = await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, "").ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }
       
        [TestMethod]
        public async Task GetStateTransitionsHistoryAsync_NotFound()
        {
            // Setup Mock
            _environmentManager.Setup(mock => mock.GetStateTransitionHistoryAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.ExtendedModel}=true");

          
            try
            {
                // Perform Method to test
                await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
               pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetStateTransitionsHistoryAsync_InternalServerError()
        {
            // Setup Mock
            _environmentManager.Setup(mock => mock.GetStateTransitionHistoryAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_environmentManager.Object, _masterDataManager.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.CurrentTime}&{RequestParameters.ExtendedModel}=true");

         
            try
            {
                // Perform Method to test
                await controller.GetStateTransitionsHistoryAsync(CancellationToken.None, TestParameters.EnvironmentName).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetStateTransitionByIdAsync Tests

        [TestMethod]
        public async Task GetStateTransitionByIdAsync_OK()
        {
            // Setup Mock
            _masterDataManager.Setup(x => x.GetStateTransitionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateStateTransition()));

            // Create Controller with Mock
            
            var controller = SetupController(_environmentManager.Object, _masterDataManager.Object, "");

            // Perform Method to test
            var result = await controller.GetStateTransitionByIdAsync(CancellationToken.None, 1).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);


            string response = TestHelper.AssertContentRequestString(result);


            var stateTransition = JsonConvert.DeserializeObject<StateTransition>(response);
                       
            stateTransition.ShouldNotBeNull();
            stateTransition.ElementId.ShouldBe(TestParameters.ElementId);
        }
        
        [TestMethod]
        public async Task GetStateTransitionByIdAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new HistoryController(_environmentManager.Object, _masterDataManager.Object);

            // Perform Method to test -> BadRequest on id <= 0
            var result = await controller.GetStateTransitionByIdAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetStateTransitionByIdAsync_NotFound()
        {
            // Setup Mock
            _masterDataManager.Setup(x => x.GetStateTransitionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new HistoryController(_environmentManager.Object, _masterDataManager.Object);

            try
            {
                // Perform Method to test
                await controller.GetStateTransitionByIdAsync(CancellationToken.None, 1).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetStateTransitionByIdAsync_InternalServerError()
        {
            // Setup Mock
            _masterDataManager.Setup(x => x.GetStateTransitionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new HistoryController(_environmentManager.Object, _masterDataManager.Object);

            try
            {
                // Perform Method to test
                await controller.GetStateTransitionByIdAsync(CancellationToken.None, 1).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }
         
        #endregion
       
        #region Private Methods

        private static Dictionary<string, List<StateTransition>> CreateStateTransitionHistory()
                {
                    return new Dictionary<string, List<StateTransition>>
                    {
                        {
                            TestParameters.EnvironmentSubscriptionId,
                            new List<StateTransition>
                            {
                                new StateTransition
                                {
                                    Id = TestParameters.ValidId,
                                    AlertName = TestParameters.Name,
                                    CheckId = TestParameters.CheckId,
                                    ComponentType = "5",
                                    CustomField1 = "",
                                    CustomField2 = "",
                                    CustomField3 = "",
                                    CustomField4 = "",
                                    CustomField5 = "",
                                    Description = TestParameters.Description,
                                    ElementId = TestParameters.ElementId,
                                    EnvironmentName = TestParameters.EnvironmentName,
                                    SourceTimestamp = TestParameters.PastTime,
                                    Frequency = TestParameters.Frequency,
                                    RecordId = TestParameters.ValidRecordId,
                                    State = State.Ok,
                                    TimeGenerated = TestParameters.PastTime,
                                    TriggerName = "",
                                    TriggeredByAlertName = "",
                                    TriggeredByCheckId = "",
                                    TriggeredByElementId = ""
                                }
                            }
                        }
                    };
                }

        private static List<StateTransition> CreateStateTransitionHistoryByElementId()
        {
            return new List<StateTransition>
            {
                new StateTransition
                {
                    Id = TestParameters.ValidId,
                    AlertName = TestParameters.Name,
                    CheckId = TestParameters.CheckId,
                    ComponentType = "5",
                    CustomField1 = "",
                    CustomField2 = "",
                    CustomField3 = "",
                    CustomField4 = "",
                    CustomField5 = "",
                    Description = TestParameters.Description,
                    ElementId = TestParameters.ElementId,
                    EnvironmentName = TestParameters.EnvironmentName,
                    SourceTimestamp = TestParameters.PastTime,
                    Frequency = TestParameters.Frequency,
                    RecordId = TestParameters.ValidRecordId,
                    State = State.Ok,
                    TimeGenerated = TestParameters.PastTime,
                    TriggerName = "",
                    TriggeredByAlertName = "",
                    TriggeredByCheckId = "",
                    TriggeredByElementId = ""
                }
            };
        }

        private static StateTransition CreateStateTransition()
        {
            return new StateTransition
            {
                Id = TestParameters.ValidId,
                AlertName = TestParameters.Name,
                CheckId = TestParameters.CheckId,
                ComponentType = "5",
                CustomField1 = "",
                CustomField2 = "",
                CustomField3 = "",
                CustomField4 = "",
                CustomField5 = "",
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentName = TestParameters.EnvironmentName,
                SourceTimestamp = TestParameters.PastTime,
                Frequency = TestParameters.Frequency,
                RecordId = TestParameters.ValidRecordId,
                State = State.Ok,
                TimeGenerated = TestParameters.PastTime,
                TriggerName = "",
                TriggeredByAlertName = "",
                TriggeredByCheckId = "",
                TriggeredByElementId = ""
            };
        }

        #endregion
    }
}
