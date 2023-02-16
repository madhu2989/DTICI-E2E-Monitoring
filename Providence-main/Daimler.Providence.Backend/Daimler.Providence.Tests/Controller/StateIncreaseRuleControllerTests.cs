using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateIncreaseRule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StateIncreaseRuleControllerTests
    {
        #region Private Members

        private Mock<IMasterdataManager> _businessLogic;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IMasterdataManager>();
        }

        #endregion
        
        #region GetStateIncreaseRuleAsync Tests

        [TestMethod]
        public async Task GetStateIncreaseRuleAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetStateIncreaseRule()));

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetStateIncreaseRuleAsyncTest_BadRequest()
        {
            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on id <= 0
            var result = await controller.GetStateIncreaseRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetStateIncreaseRuleAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetStateIncreaseRuleAsync(CancellationToken.None, 1).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetStateIncreaseRuleAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetStateIncreaseRulesAsync Tests

        [TestMethod]
        
        public async Task GetStateIncreaseRulesAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetStateIncreaseRulesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetStateIncreaseRule> { CreateGetStateIncreaseRule() }));

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);
            // FIXME: Test dies here
            var result = await controller.GetStateIncreaseRulesAsync(CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertOkRequest(result);

            // BusinessLogic returns an empty list
            _businessLogic.Setup(mock => mock.GetStateIncreaseRulesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetStateIncreaseRule>()));

            controller = new StateIncreaseRuleController(_businessLogic.Object);

            result = await controller.GetStateIncreaseRulesAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetStateIncreaseRulesAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetStateIncreaseRulesAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetStateIncreaseRulesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddStateIncreaseRuleAsync Tests

        [TestMethod]
        public async Task AddStateIncreaseRuleAsyncTest_Created()
        {
            var newStateIncreaseRule = CreatePostStateIncreaseRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddStateIncreaseRuleAsync(It.IsAny<PostStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetStateIncreaseRule()));

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddStateIncreaseRuleAsyncTest_BadRequest()
        {
            var newStateIncreaseRule = CreatePostStateIncreaseRule();

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on stateIncreaseRule = null
            var result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on stateIncreaseRule.CheckId = ""
            newStateIncreaseRule.CheckId = "";
            result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.CheckId = TestParameters.CheckId;

            // Perform Method to test -> BadRequest on stateIncreaseRule.TriggerTime <= 0
            newStateIncreaseRule.TriggerTime = -1;
            result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.TriggerTime = 10;

            // Perform Method to test -> BadRequest on stateIncreaseRule.EnvironmentSubscriptionId = ""
            newStateIncreaseRule.EnvironmentSubscriptionId = "";
            result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on tateIncreaseRule.ComponentId = ""
            newStateIncreaseRule.ComponentId = "";
            result = await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddStateIncreaseRuleAsyncTest_NotFound()
        {
            var newStateIncreaseRule = CreatePostStateIncreaseRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddStateIncreaseRuleAsync(It.IsAny<PostStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddStateIncreaseRuleAsyncTest_InternalServerError()
        {
            var newStateIncreaseRule = CreatePostStateIncreaseRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddStateIncreaseRuleAsync(It.IsAny<PostStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddStateIncreaseRuleAsync(CancellationToken.None, newStateIncreaseRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateStateIncreaseRuleAsync Tests

        [TestMethod]
        public async Task UpdateStateIncreaseRuleAsyncTest_NoContent()
        {
            var newStateIncreaseRule = CreatePutStateIncreaseRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<PutStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => { }));

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateAStateIncreaseRuleAsyncTest_BadRequest()
        {
            var newStateIncreaseRule = CreatePutStateIncreaseRule();

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on id <= 0
            var result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.InvalidId, newStateIncreaseRule).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on stateIncreaseRule = null
            result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, null).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on stateIncreaseRule.CheckId = ""
            newStateIncreaseRule.CheckId = "";
            result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.CheckId = TestParameters.CheckId;

            // Perform Method to test -> BadRequest on stateIncreaseRule.TriggerTime <= 0
            newStateIncreaseRule.TriggerTime = -1;
            result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.TriggerTime = 10;

            // Perform Method to test -> BadRequest on stateIncreaseRule.EnvironmentSubscriptionId = ""
            newStateIncreaseRule.EnvironmentSubscriptionId = "";
            result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newStateIncreaseRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on tateIncreaseRule.ComponentId = ""
            newStateIncreaseRule.ComponentId = "";
            result = await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateStateIncreaseRuleAsyncTest_NotFound()
        {
            var newStateIncreaseRule = CreatePutStateIncreaseRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<PutStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateStateIncreaseRuleAsyncTest_GeneralException()
        {
            var newStateIncreaseRule = CreatePutStateIncreaseRule();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<PutStateIncreaseRule>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId, newStateIncreaseRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteStateIncreaseRuleAsync Tests

        [TestMethod]
        public async Task DeleteStateIncreaseRuleAsyncTest_Accepted()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => { }));

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            var result = await controller.DeleteStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteStateIncreaseRuleAsyncTest_BadRequest()
        {
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            // Id <= 0
            var result = await controller.DeleteStateIncreaseRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }
        
        [TestMethod]
        public async Task DeleteStateIncreaseRuleAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteStateIncreaseRuleAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteStateIncreaseRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new StateIncreaseRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteStateIncreaseRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion
        
        #region Private Methods

        private static GetStateIncreaseRule CreateGetStateIncreaseRule()
        {
            var getStateIncreaseRule = new GetStateIncreaseRule
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                EnvironmentName = TestParameters.EnvironmentName,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CheckId = TestParameters.CheckId,
                AlertName = TestParameters.Name,
                ComponentId = TestParameters.ElementId,
                TriggerTime = 30,
                IsActive = true
            };
            return getStateIncreaseRule;
        }

        private static PostStateIncreaseRule CreatePostStateIncreaseRule()
        {
            var postStateIncreaseRule = new PostStateIncreaseRule
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CheckId = TestParameters.CheckId,
                AlertName = TestParameters.Name,
                ComponentId = TestParameters.ElementId,
                TriggerTime = 30,
                IsActive = true
            };
            return postStateIncreaseRule;
        }

        private static PutStateIncreaseRule CreatePutStateIncreaseRule()
        {
            var putStateIncreaseRule = new PutStateIncreaseRule
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CheckId = TestParameters.CheckId,
                AlertName = TestParameters.Name,
                ComponentId = TestParameters.ElementId,
                TriggerTime = 30,
                IsActive = true
            };
            return putStateIncreaseRule;
        }

        #endregion
    }
}
