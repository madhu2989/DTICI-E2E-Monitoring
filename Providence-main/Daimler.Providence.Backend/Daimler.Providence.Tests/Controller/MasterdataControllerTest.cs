using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Check;
using Daimler.Providence.Service.Models.MasterData.Component;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Models.MasterData.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MasterdataControllerTests
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

        #region Environments

        #region GetEnvironmentAsync Tests

        [TestMethod]
        public async Task GetEnvironmentAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateGetEnvironment()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetEnvironmentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetEnvironmentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetEnvironmentsAsync

        [TestMethod]
        public async Task GetEnvironmentsAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetEnvironment> { CreateGetEnvironment() }));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetEnvironmentAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetEnvironmentsAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentsAsync(It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetEnvironmentAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetEnvironmentsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetEnvironmentsAsync(It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetEnvironmentAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddEnvironmentAsync

        [TestMethod]
        public async Task AddEnvironmentAsync_Created()
        {
            var newEnvironment = CreatePostEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddEnvironmentAsync(It.IsAny<PostEnvironment>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new GetEnvironment()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Environment name can contain spaces - _ &
            newEnvironment.Name = "dev 123-nbb_tss & Co";
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddEnvironmentAsync_Conflict()
        {
            var newEnvironment = CreatePostEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddEnvironmentAsync(It.IsAny<PostEnvironment>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddEnvironmentAsync_BadRequest()
        {
            var newEnvironment = CreatePostEnvironment();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddEnvironmentAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment = null
            result = await controller.AddEnvironmentAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name = ""
            newEnvironment.Name = "";
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name is to short
            newEnvironment.Name = new string('a', 1);
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name is to long
            newEnvironment.Name = new string('a', 1000);
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.ElementId = null
            newEnvironment.ElementId = "";
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.ElementId is to short
            newEnvironment.ElementId = new string('a', 1);
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.ElementId is to long
            newEnvironment.ElementId = new string('1', 999);
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.ElementId contains wrong characters
            newEnvironment.ElementId = "988{988";
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.ElementId contains wrong character
            newEnvironment.ElementId = "=999393";
            result = await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddEnvironmentAsync_NotFound()
        {
            var newEnvironment = CreatePostEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddEnvironmentAsync(It.IsAny<PostEnvironment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddEnvironmentAsync_InternalServerError()
        {
            var newEnvironment = CreatePostEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddEnvironmentAsync(It.IsAny<PostEnvironment>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddEnvironmentAsync(CancellationToken.None, newEnvironment).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateEnvironmentAsync

        [TestMethod]
        public async Task UpdateEnvironmentAsync_NoContent()
        {
            var newEnvironment = CreatePutEnvironment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateEnvironmentAsync(It.IsAny<string>(), It.IsAny<PutEnvironment>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => { }));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);

            // Environment name can contain spaces _ - &
            newEnvironment.Name = "dev ved_44-89ff & max";
            result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, "elementId").ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateEnvironmentAsync_BadRequest()
        {
            var newEnvironment = CreatePutEnvironment();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on elementId = null
            var result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment = null
            result = await controller.UpdateEnvironmentAsync(CancellationToken.None, null, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name = ""
            newEnvironment.Name = "";
            result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name is to short
            newEnvironment.Name = new string('a', 1);
            result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environment.Name is to long
            newEnvironment.Name = new string('a', 1000);
            result = await controller.UpdateEnvironmentAsync(CancellationToken.None, newEnvironment, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateEnvironmentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateEnvironmentAsync(It.IsAny<string>(), It.IsAny<PutEnvironment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateEnvironmentAsync(CancellationToken.None, new PutEnvironment(), TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateEnvironmentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateEnvironmentAsync(It.IsAny<string>(), It.IsAny<PutEnvironment>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateEnvironmentAsync(CancellationToken.None, new PutEnvironment(), TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteEnvironmentAsync

        [TestMethod]
        public async Task DeleteEnvironmentAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteEnvironmentAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on elementId = ""
            var result = await controller.DeleteEnvironmentAsync(CancellationToken.None, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteEnvironmentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteEnvironmentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteEnvironmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteEnvironmentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #endregion

        #region Services

        #region GetServiceAsync

        [TestMethod]
        public async Task GetServiceAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetService()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetServiceAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscription = "" and elementId != ""
            var result = await controller.GetServiceAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetServiceAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetServiceAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetServicesAsnyc

        [TestMethod]
        public async Task GetServicesAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServicesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => new List<GetService>()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetServiceAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetServicesAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServicesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetServiceAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetServicesAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetServicesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetServiceAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddServiceAsnyc

        [TestMethod]
        public async Task AddServiceAsync_Created()
        {
            var newService = CreatePostService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddServiceAsync(It.IsAny<PostService>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetService()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> service.Name can contain spaces - _ &
            newService.Name = "dev 123-nbb_tss & Co";
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> service.ElementId can be null
            newService.ElementId = string.Empty;
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddServiceAsync_Conflict()
        {
            var newService = CreatePostService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddServiceAsync(It.IsAny<PostService>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddServiceAsync_BadRequest()
        {
            var newService = CreatePostService();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on service = null
            var result = await controller.AddServiceAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.EnvironmentSubscriptionId = ""
            newService.EnvironmentSubscriptionId = "";
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newService.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on service.Name = ""
            newService.Name = "";
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.Name is to short
            newService.Name = new string('a', 1);
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.Name is to long
            newService.Name = new string('a', 1000);
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.ElementId is to short
            newService.ElementId = new string('a', 1);
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.ElementId is to long
            newService.ElementId = new string('a', 1000);
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.ElementId contains wrong characters
            newService.ElementId = "988{988";
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.ElementId starts with wrong character
            newService.ElementId = "=999393";
            result = await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddServiceAsync_NotFound()
        {
            var newService = CreatePostService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddServiceAsync(It.IsAny<PostService>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddServiceAsync_InternalServerError()
        {
            var newService = CreatePostService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddServiceAsync(It.IsAny<PostService>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddServiceAsync(CancellationToken.None, newService).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateServiceAsync

        [TestMethod]
        public async Task UpdateServiceAsync_NoContent()
        {
            var newService = CreatePutService();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);

            // Service name can contain spaces _ - &
            newService.Name = "dev ved_44-89ff & max";
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateServiceAsync_BadRequest()
        {
            var newService = CreatePutService();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on service = null
            var result = await controller.UpdateServiceAsync(CancellationToken.None, null, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, string.Empty, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.ElementId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.Name = ""
            newService.Name = "";
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.Name is to short
            newService.Name = new string('a', 1);
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on service.Name is to long
            newService.Name = new string('a', 1000);
            result = await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateServiceAsync_NotFound()
        {
            var newService = CreatePutService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutService>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateServiceAsync_InternalServerError()
        {
            var newService = CreatePutService();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutService>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateServiceAsync(CancellationToken.None, newService, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteServiceAsync

        [TestMethod]
        public async Task DeleteServiceAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteServiceAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            var result = await controller.DeleteServiceAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.DeleteServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteServiceAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteServiceAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteServiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteServiceAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #endregion

        #region Actions

        #region GetActionAsync

        [TestMethod]
        public async Task GetActionAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetAction()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetActionAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = "" and elementId != ""
            var result = await controller.GetActionAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetActionAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetActionAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetActionsAsync

        [TestMethod]
        public async Task GetActionsAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetActionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetAction>()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetActionAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetActionsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetActionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetActionAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddActionAsync

        [TestMethod]
        public async Task AddActionAsync_Created()
        {
            var newAction = CreatePostAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddActionAsync(It.IsAny<PostAction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetAction()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> action.Name can contain spaces - _ &
            newAction.Name = "dev 123-nbb_tss & Co";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> action.ElementId can be null
            newAction.ElementId = string.Empty;
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddActionAsync_Conflict()
        {
            var newAction = CreatePostAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddActionAsync(It.IsAny<PostAction>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddActionAsync_BadRequest()
        {
            var newAction = CreatePostAction();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on action = null
            var result = await controller.AddActionAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.EnvironmentSubscriptionId = ""
            newAction.EnvironmentSubscriptionId = "";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newAction.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on action.ServiceElementId = ""
            newAction.ServiceElementId = "";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newAction.ServiceElementId = TestParameters.ElementId;

            // Perform Method to test -> BadRequest on action.Name = ""
            newAction.Name = "";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.Name is to short
            newAction.Name = new string('a', 1);
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.Name is to long
            newAction.Name = new string('a', 1000);
            result = await controller.AddActionAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.ElementId is to short
            newAction.ElementId = new string('a', 1);
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.ElementId is to long
            newAction.ElementId = new string('a', 999);
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.ElementId contains wrong characters
            newAction.ElementId = "988{988";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.ElementId starts with wrong character
            newAction.ElementId = "=999393";
            result = await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddActionAsync_NotFound()
        {
            var newAction = CreatePostAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddActionAsync(It.IsAny<PostAction>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddActionAsync_InternalServerError()
        {
            var newAction = CreatePostAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddActionAsync(It.IsAny<PostAction>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddActionAsync(CancellationToken.None, newAction).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateActionAsync

        [TestMethod]
        public async Task UpdateActionAsync_NoContent()
        {
            var newAction = CreatePutAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutAction>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => { }));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);

            // Service name can contain spaces _ - &
            newAction.Name = "dev ved_44-89ff & max";
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateActionAsync_BadRequest()
        {
            var newAction = CreatePutAction();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on action = null
            var result = await controller.UpdateActionAsync(CancellationToken.None, null, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.ServiceElementId = ""
            newAction.ServiceElementId = "";
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newAction.ServiceElementId = TestParameters.ElementId;

            // Perform Method to test -> BadRequest on action.Name = ""
            newAction.Name = "";
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.Name is to short
            newAction.Name = new string('a', 1);
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on action.Name is to long
            newAction.Name = new string('a', 1000);
            result = await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateActionAsync_NotFound()
        {
            var newAction = CreatePutAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutAction>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateActionAsync_InternalServerError()
        {
            var newAction = CreatePutAction();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutAction>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateActionAsync(CancellationToken.None, newAction, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteActionAsync

        [TestMethod]
        public async Task DeleteActionAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteActionAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            var result = await controller.DeleteActionAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.DeleteActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteActionAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteActionAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteActionAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #endregion

        #region Components

        #region GetComponentAsync

        [TestMethod]
        public async Task GetComponentAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetComponent()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetComponentAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = "" and elementId != ""
            var result = await controller.GetComponentAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetComponentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetComponentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetComponentsAsync

        [TestMethod]
        public async Task GetComponentsAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => new List<GetComponent> { new GetComponent() }));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            var result = await controller.GetComponentAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetComponentsAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetComponentAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetComponentsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetComponentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetComponentAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddComponentAsync

        [TestMethod]
        public async Task AddComponentAsync_Created()
        {
            var newComponent = CreatePostComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddComponentAsync(It.IsAny<PostComponent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetComponent()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> component.Name can contain spaces - _ &
            newComponent.Name = "dev 123-nbb_tss & Co";
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> component.ElementId can be null
            newComponent.ElementId = string.Empty;
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddComponentAsync_Conflict()
        {
            var newComponent = CreatePostComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddComponentAsync(It.IsAny<PostComponent>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddComponentAsync_BadRequest()
        {
            var newComponent = CreatePostComponent();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on component = null
            var result = await controller.AddComponentAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.EnvironmentSubscriptionId = ""
            newComponent.EnvironmentSubscriptionId = "";
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newComponent.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on component.Name = ""
            newComponent.Name = "";
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.Name is to short
            newComponent.Name = new string('a', 1);
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.Name is to long
            newComponent.Name = new string('a', 1000);
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.ElementId is to short
            newComponent.ElementId = new string('a', 1);
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.ElementId is to long
            newComponent.ElementId = new string('a', 999);
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.ElementId contains wrong characters
            newComponent.ElementId = "988{988";
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.ElementId starts with wrong character
            newComponent.ElementId = "=999393";
            result = await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddComponentAsync_NotFound()
        {
            var newComponent = CreatePostComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddComponentAsync(It.IsAny<PostComponent>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddComponentAsync_InternalServerError()
        {
            var newComponent = CreatePostComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddComponentAsync(It.IsAny<PostComponent>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddComponentAsync(CancellationToken.None, newComponent).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateComponentAsync

        [TestMethod]
        public async Task UpdateComponentAsync_NoContent()
        {
            var newComponent = CreatePutComponent();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);

            // Perform Method to test -> component.Name can contain spaces - _ &
            newComponent.Name = "dev 123-nbb_tss & Co";
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateComponentAsync_BadRequest()
        {
            var newComponent = CreatePutComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutComponent>(), It.IsAny<CancellationToken>()));

            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on component = null
            var result = await controller.UpdateComponentAsync(CancellationToken.None, null, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.Name = ""
            newComponent.Name = "";
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.Name is to short
            newComponent.Name = new string('a', 1);
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on component.Name is to long
            newComponent.Name = new string('a', 1000);
            result = await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateComponentAsync_NotFound()
        {
            var newComponent = CreatePutComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutComponent>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateComponentAsync_InternalServerError()
        {
            var newComponent = CreatePutComponent();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutComponent>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateComponentAsync(CancellationToken.None, newComponent, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteComponentAsync

        [TestMethod]
        public async Task DeleteComponentAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteComponentAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            var result = await controller.DeleteComponentAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.DeleteComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteComponentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteComponentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteComponentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteComponentAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #endregion

        #region Checks

        #region GetCheckAsync

        [TestMethod]
        public async Task GetCheckAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetCheck()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetCheckAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = "" and elementId != ""
            var result = await controller.GetCheckAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetCheckTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetCheckAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetChecksAsync

        [TestMethod]
        public async Task GetChecksAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChecksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetCheck>()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetCheckAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetChecksTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChecksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetChecksAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetChecksAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetCheckAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddCheckAsync

        [TestMethod]
        public async Task AddCheckAsync_Created()
        {
            var newCheck = CreatePostCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddCheckAsync(It.IsAny<PostCheck>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => CreateGetCheck()));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> check.Name can contain spaces - _ &
            newCheck.Name = "dev 123-nbb_tss & Co";
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);

            // Perform Method to test -> check.ElementId can be null
            newCheck.ElementId = string.Empty;
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(result);
        }

        [TestMethod]
        public async Task AddCheckAsync_Conflict()
        {
            var newCheck = CreatePostCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddCheckAsync(It.IsAny<PostCheck>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddCheckAsync_BadRequest()
        {
            var newCheck = CreatePostCheck();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on check = null
            var result = await controller.AddCheckAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.EnvironmentSubscriptionId = ""
            newCheck.EnvironmentSubscriptionId = "";
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
            newCheck.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on check.Name = ""
            newCheck.Name = "";
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.Name is to short
            newCheck.Name = new string('a', 1);
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.Name is to long
            newCheck.Name = new string('a', 1000);
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.ElementId is to short
            newCheck.ElementId = new string('a', 1);
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.ElementId is to long
            newCheck.ElementId = new string('a', 999);
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.ElementId contains wrong characters
            newCheck.ElementId = "988{988";
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.ElementId starts with wrong character
            newCheck.ElementId = "=999393";
            result = await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddCheckTest_NotFound()
        {
            var newCheck = CreatePostCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddCheckAsync(It.IsAny<PostCheck>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddCheckAsync_InternalServerError()
        {
            var newCheck = CreatePostCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddCheckAsync(It.IsAny<PostCheck>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddCheckAsync(CancellationToken.None, newCheck).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateCheckAsync

        [TestMethod]
        public async Task UpdateCheckAsync_NoContent()
        {
            var newCheck = CreatePutCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutCheck>(), It.IsAny<CancellationToken>())).Returns(Task.Run(() => { }));

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);

            // Perform Method to test -> check.Name can contain spaces - _ &
            newCheck.Name = "dev 123-nbb_tss & Co";
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateCheckAsync_BadRequest()
        {
            var newCheck = CreatePutCheck();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on check = null
            var result = await controller.UpdateCheckAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscription = ""
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.Name = ""
            newCheck.Name = "";
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.Name is to short
            newCheck.Name = new string('a', 1);
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on check.Name is to long
            newCheck.Name = new string('a', 1000);
            result = await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateCheckAsync_NotFound()
        {
            var newCheck = CreatePutCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutCheck>(), It.IsAny<CancellationToken>())).Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);

            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateCheckAsync_InternalServerError()
        {
            var newCheck = CreatePutCheck();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutCheck>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateCheckAsync(CancellationToken.None, newCheck, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteCheckAsync

        [TestMethod]
        public async Task DeleteCheckAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            var result = await controller.DeleteCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteCheckAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            var result = await controller.DeleteCheckAsync(CancellationToken.None, string.Empty, TestParameters.ElementId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on elementId = ""
            result = await controller.DeleteCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, string.Empty).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteCheckAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteCheckAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteCheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new MasterdataController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteCheckAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ElementId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Environment

        private static GetEnvironment CreateGetEnvironment()
        {
            var getEnvironment = new GetEnvironment
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true
            };
            return getEnvironment;
        }

        private static PostEnvironment CreatePostEnvironment()
        {
            var postEnvironment = new PostEnvironment
            {
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                ElementId = TestParameters.EnvironmentSubscriptionId,
                IsDemo = true
            };
            return postEnvironment;
        }

        private static PutEnvironment CreatePutEnvironment()
        {
            var putEnvironment = new PutEnvironment
            {
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                IsDemo = true
            };
            return putEnvironment;
        }

        #endregion

        #region Service

        private static GetService CreateGetService()
        {
            var getService = new GetService
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                CreateDate = TestParameters.PastTime,
            };
            return getService;
        }

        private static PostService CreatePostService()
        {
            var postService = new PostService
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            return postService;
        }

        private static PutService CreatePutService()
        {
            var putService = new PutService
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description
            };
            return putService;
        }

        #endregion

        #region Action

        private static GetAction CreateGetAction()
        {
            var getAction = new GetAction
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                ServiceElementId = TestParameters.ElementId,
                CreateDate = TestParameters.PastTime,
            };
            return getAction;
        }

        private static PostAction CreatePostAction()
        {
            var postAction = new PostAction
            {
                Name = TestParameters.ElementId,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                ServiceElementId = TestParameters.ElementId,
                Components = new List<string>()
            };
            return postAction;
        }

        private static PutAction CreatePutAction()
        {
            var putAction = new PutAction
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ServiceElementId = TestParameters.ElementId,
                Components = new List<string>()
            };
            return putAction;
        }

        #endregion

        #region Component

        private static GetComponent CreateGetComponent()
        {
            var getComponent = new GetComponent
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                CreateDate = TestParameters.PastTime,
            };
            return getComponent;
        }

        private static PostComponent CreatePostComponent()
        {
            var postComponent = new PostComponent
            {
                Name = TestParameters.ElementId,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId
            };
            return postComponent;
        }

        private static PutComponent CreatePutComponent()
        {
            var putComponent = new PutComponent
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description
            };
            return putComponent;
        }

        #endregion

        #region Check

        private static GetCheck CreateGetCheck()
        {
            var getCheck = new GetCheck
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Frequency = TestParameters.Frequency
            };
            return getCheck;
        }

        private static PostCheck CreatePostCheck()
        {
            var postCheck = new PostCheck
            {
                Name = TestParameters.ElementId,
                Description = TestParameters.Description,
                ElementId = TestParameters.ElementId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Frequency = TestParameters.Frequency
            };
            return postCheck;
        }

        private static PutCheck CreatePutCheck()
        {
            var putCheck = new PutCheck
            {
                Name = TestParameters.Name,
                Description = TestParameters.Description,
                Frequency = TestParameters.Frequency
            };
            return putCheck;
        }

        #endregion

        #endregion
    }
}