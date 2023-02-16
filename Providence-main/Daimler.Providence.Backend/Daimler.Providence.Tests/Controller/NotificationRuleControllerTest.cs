using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.NotificationRule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NotificationRuleControllerTest
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

        #region GetNotificationRuleAsync Tests

        [TestMethod]
        public async Task GetNotificationRuleAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetNotificationRule()));

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetNotificationRuleAsyncTest_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetNotificationRule()));

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetNotificationRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetNotificationRuleAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetNotificationRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetNotificationRuleAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetNotificationRulesAsync Tests

        [TestMethod]
        public async Task GetNotificationRulesAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRulesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetNotificationRule>()));

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetNotificationRulesAsync(CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertOkRequest(result);
        }


        [TestMethod]
        public async Task GetNotificationRulesAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.GetNotificationRulesAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetNotificationRulesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddNotificationRuleAsync Tests

        [TestMethod]
        public async Task AddNotificationRuleAsyncTest_Created()
        {
            var newNotificationRule = CreatePostNotificationRule();

            // Setup Mock
            _businessLogic.Setup(x => x.AddNotificationRuleAsync(It.IsAny<PostNotificationRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetNotificationRule()));

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddNotificationRuleAsyncTest_BadRequest()
        {
            var newNotificationRule = CreatePostNotificationRule();
            
            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on notificationRule = null
            var result = await controller.AddNotificationRuleAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on notificationRule.EmailAddresses contains invalid item
            newNotificationRule.EmailAddresses = "unit-test.com";
            result = await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newNotificationRule.EmailAddresses = TestParameters.EmailAddress;

            // Perform Method to test -> BadRequest on notificationRule.EnvironmentSubscriptionId = ""
            newNotificationRule.EnvironmentSubscriptionId = string.Empty;
            result = await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newNotificationRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on notificationRule.States contains invalid item
            newNotificationRule.States = new List<string> { "OK" };
            result = await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddNotificationRuleAsyncTest_NotFound()
        {
            var newNotificationRule = CreatePostNotificationRule();
            
            // Setup Mock
            _businessLogic.Setup(x => x.AddNotificationRuleAsync(It.IsAny<PostNotificationRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddNotificationRuleAsyncTest_InternalServerError()
        {
            var newNotificationRule = CreatePostNotificationRule();
            
            // Setup Mock
            _businessLogic.Setup(x => x.AddNotificationRuleAsync(It.IsAny<PostNotificationRule>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);


            // Perform Method to test
            try
            {
                // Perform Method to test
                await controller.AddNotificationRuleAsync(CancellationToken.None, newNotificationRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateNotificationRuleAsync Test

        [TestMethod]
        public async Task UpdateNotificationRuleAsyncTest_NoContent()
        {
            var newNotificationRule = CreatePutNotificationRule();
            
            // Setup Mock
            _businessLogic.Setup(x => x.UpdateNotificationRuleAsync(It.IsAny<int>(), It.IsAny<PutNotificationRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetNotificationRule()));

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            result.ShouldNotBe(null);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateNotificationRuleAsyncTest_BadRequest()
        {
            var newNotificationRule = CreatePutNotificationRule();

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on id <= 0
            var result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.InvalidId, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on notificationRule = null
            result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on notificationRule.EmailAddresses contains invalid item
            newNotificationRule.EmailAddresses = "unit-test.com";
            result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newNotificationRule.EmailAddresses = TestParameters.EmailAddress;

            // Perform Method to test -> BadRequest on notificationRule.EnvironmentSubscriptionId = ""
            newNotificationRule.EnvironmentSubscriptionId = "";
            result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            newNotificationRule.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on notificationRule.States contains invalid item
            newNotificationRule.States = new List<string> { "OK" };
            result = await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateNotificationRuleAsyncTest_NotFound()
        {
            var newNotificationRule = CreatePutNotificationRule();

            // Setup Mock
            _businessLogic.Setup(x => x.UpdateNotificationRuleAsync(It.IsAny<int>(), It.IsAny<PutNotificationRule>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateNotificationRuleAsyncTest_InternalServerError()
        {
            var newNotificationRule = CreatePutNotificationRule();

            // Setup Mock
            _businessLogic.Setup(x => x.UpdateNotificationRuleAsync(It.IsAny<int>(), It.IsAny<PutNotificationRule>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId, newNotificationRule).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteNotificationRuleAsync Test

        [TestMethod]
        public async Task DeleteNotificationRuleAsyncTest_Accepted()
        {
            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteNotificationRuleAsyncTest_BadRequest()
        {
            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteNotificationRuleAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteNotificationRuleAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.DeleteNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteNotificationRuleAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(x => x.DeleteNotificationRuleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new NotificationRuleController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteNotificationRuleAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private static GetNotificationRule CreateGetNotificationRule()
        {
            var getNotificationRule = new GetNotificationRule
            {
                EmailAddresses = TestParameters.EmailAddress,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                NotificationInterval = 6000,
                IsActive = true,
                States = new List<string> { "ERROR" },
                Levels = new List<string> { "Environment" }
            };
            return getNotificationRule;
        }

        private static PostNotificationRule CreatePostNotificationRule()
        {
            var postNotificationRule = new PostNotificationRule
            {
                EmailAddresses = TestParameters.EmailAddress,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                NotificationInterval = 6000,
                IsActive = true,
                States = new List<string> { "ERROR" },
                Levels = new List<string> { "Environment" }
            };
            return postNotificationRule;
        }

        private static PutNotificationRule CreatePutNotificationRule()
        {
            var putNotificationRule = new PutNotificationRule
            {
                EmailAddresses = TestParameters.EmailAddress,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                NotificationInterval = 6000,
                IsActive = true,
                States = new List<string> { "ERROR" },
                Levels = new List<string> { "Environment" }
            };
            return putNotificationRule;
        }

        #endregion 
    }
}
