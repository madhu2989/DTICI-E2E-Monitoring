using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AlertIgnoreControllerTest
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

        #region GetAlertIgnoreRuleAsync Tests

        [TestMethod]
        public async Task GetAlertIgnoreRuleAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetAlertIgnoreRule()));

             // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetAlertIgnoreRuleAsyncTest_BadRequest()
        {
            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            
        }

        [TestMethod]
        public async Task GetAlertIgnoreRuleAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetAlertIgnoreRuleAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetAlertIgnoreRulesAsync Tests

        [TestMethod]
        public async Task GetAlertIgnoreRulesAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertIgnoreRulesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetAlertIgnoreRule> { CreateGetAlertIgnoreRule() }));

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetAlertIgnoreRulesAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }
                
        [TestMethod]
        public async Task GetAlertIgnoreRulesAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertIgnoreRulesAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertIgnoreRulesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddAlertIgnoreRulesAsync Tests

        [TestMethod]
        public async Task AddAlertIgnoreRulesAsyncTest_Created()
        {
            var newAlertIgnoreRule = CreatePostAlertIgnoreRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertIgnoreRuleAsync(It.IsAny<PostAlertIgnoreRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetAlertIgnoreRule()));

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
            
        }
       
        [TestMethod]
        public async Task AddAlertIgnoreRulesAsyncTest_BadRequest()
        {
            var newAlertIgnoreRule = CreatePostAlertIgnoreRule();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on alertIgnoreRule = null
            var result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on alertIgnoreRule.Name = null
            newAlertIgnoreRule.Name = null;
            result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.Name = TestParameters.Name;

            // Perform Method to test -> BadRequest on alertIgnoreRule.EnvironmentSubscriptionId = null
            newAlertIgnoreRule.EnvironmentSubscriptionId = null;
            result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on alertIgnoreRule.AlertIgnoreCondition = null
            newAlertIgnoreRule.IgnoreCondition = null;
            result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.IgnoreCondition = new AlertIgnoreCondition();

            // Perform Method to test -> BadRequest on alertIgnoreRule.CreationDate > alertIgnoreRule.ExpirationDate
            newAlertIgnoreRule.ExpirationDate = TestParameters.PastTime;
            result = await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddAlertIgnoreRulesAsyncTest_NotFound()
        {
            var newAlertIgnoreRule = CreatePostAlertIgnoreRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertIgnoreRuleAsync(It.IsAny<PostAlertIgnoreRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddAlertIgnoreRulesAsyncTest_InternalServerError()
        {
            var newAlertIgnoreRule = CreatePostAlertIgnoreRule();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertIgnoreRuleAsync(It.IsAny<PostAlertIgnoreRule>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddAlertIgnoreRuleAsync(CancellationToken.None, newAlertIgnoreRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateAlertIgnoreRulesAsync Tests

        [TestMethod]
        public async Task UpdateAlertIgnoreAsyncTest_NoContent()
        {
            var newAlertIgnoreRule = CreatePutAlertIgnoreRule();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
            
        }

        [TestMethod]
        public async Task UpdateAlertIgnoreAsyncTest_BadRequest()
        {
            var newAlertIgnoreRule = CreatePutAlertIgnoreRule();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on alertIgnoreRule = null
            var result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on alertIgnoreRule.Name = null
            newAlertIgnoreRule.Name = null;
            result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.Name = TestParameters.Name;

            // Perform Method to test -> BadRequest on alertIgnoreRule.EnvironmentSubscriptionId = null
            newAlertIgnoreRule.EnvironmentSubscriptionId = null;
            result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on alertIgnoreRule.AlertIgnoreCondition = null
            newAlertIgnoreRule.IgnoreCondition = null;
            result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newAlertIgnoreRule.IgnoreCondition = new AlertIgnoreCondition();

            // Perform Method to test -> BadRequest on alertIgnoreRule.CreationDate > alertIgnoreRule.ExpirationDate
            newAlertIgnoreRule.ExpirationDate = TestParameters.PastTime;
            result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on id <= 0
            result = await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.InvalidId, newAlertIgnoreRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateAlertIgnoreAsyncTest_NotFound()
        {
            var newAlertIgnoreRule = CreatePutAlertIgnoreRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<PutAlertIgnoreRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateAlertIgnoreAsyncTest_InternalServerError()
        {
            var newAlertIgnoreRule = CreatePutAlertIgnoreRule();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<PutAlertIgnoreRule>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId, newAlertIgnoreRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteAlertIgnoreRulesAsync Tests

        [TestMethod]
        public async Task DeleteAlertIgnoreAsyncTest_Accepted()
        {
            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
            
        }

        [TestMethod]
        public async Task DeleteAlertIgnoreAsyncTest_BadRequest()
        {
            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on id <= 0
            var result = await controller.DeleteAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteAlertIgnoreAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteAlertIgnoreAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteAlertIgnoreRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertIgnoreController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteAlertIgnoreRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private static GetAlertIgnoreRule CreateGetAlertIgnoreRule()
        {
            var getAlertIgnoreRule = new GetAlertIgnoreRule
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                EnvironmentName = TestParameters.EnvironmentName,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                ExpirationDate = DateTime.UtcNow,
                IgnoreCondition = new AlertIgnoreCondition()
            };
            return getAlertIgnoreRule;
        }

        private static PostAlertIgnoreRule CreatePostAlertIgnoreRule()
        {
            var postAlertIgnoreRule = new PostAlertIgnoreRule
            {
                Name = TestParameters.Name,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                ExpirationDate = TestParameters.FutureTime,
                IgnoreCondition = new AlertIgnoreCondition()
            };
            return postAlertIgnoreRule;
        }

        private static PutAlertIgnoreRule CreatePutAlertIgnoreRule()
        {
            var putAlertIgnoreRule = new PutAlertIgnoreRule
            {
                Name = TestParameters.Name,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                ExpirationDate = TestParameters.FutureTime,
                IgnoreCondition = new AlertIgnoreCondition()
            };
            return putAlertIgnoreRule;
        }

        #endregion
    }
}
