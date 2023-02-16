using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConfigurationControllerTest
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

        #region GetConfiguration Tests

        


        [TestMethod]
        public async Task GetConfiguration_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateGetConfiguration());
            _businessLogic.Setup(mock => mock.GetConfigurationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetConfiguration> {CreateGetConfiguration()});

            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ConfigurationKey}={TestParameters.ConfigurationKey}");
                      
            // Perform Method to test
            var result = await controller.GetConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            result = await controller.GetConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            TestHelper.AssertOkRequest(result);
        }

        [TestMethod]
        public async Task GetConfiguration_BadRequest()
        {
            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ConfigurationKey}={TestParameters.ConfigurationKey}");

          
            // Perform Method to test -> BadRequest on environmentSubscriptionId = null
            var result = await controller.GetConfigurationAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.GetConfigurationAsync(CancellationToken.None, "").ConfigureAwait(false);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetConfiguration_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ConfigurationKey}={TestParameters.ConfigurationKey}");

          
            try
            {
                // Perform Method to test
                await controller.GetConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetConfiguration_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ConfigurationKey}={TestParameters.ConfigurationKey}");


            try
            {
                // Perform Method to test 
                await controller.GetConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddConfigurationAsync Tests

        [TestMethod]
        public async Task AddConfiguration_Created()
        {
            var newConfiguration = CreatePostConfiguration();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.AddConfigurationAsync(It.IsAny<PostConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateGetConfiguration());

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertCreatedRequest(result);

            GetConfiguration configuration = (GetConfiguration)TestHelper.AssertContentRequest(result,typeof(GetConfiguration));
            
            
            configuration.Id.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task AddConfiguration_BadRequest()
        {
            var newConfiguration = CreatePostConfiguration();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddConfigurationAsync(It.IsAny<PostConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateGetConfiguration());
            
            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on configuration = null
            var result = await controller.AddConfigurationAsync(new CancellationToken(), null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.EnvironmentSubscriptionId = null
            newConfiguration.EnvironmentSubscriptionId = null;
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.EnvironmentSubscriptionId = ""
            newConfiguration.EnvironmentSubscriptionId = "";
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            newConfiguration.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on configuration.Key.Length < 5
            newConfiguration.Key = new string('a', 1);
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.Key.Length > 100
            newConfiguration.Key = new string('a', 101);
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.Key contains invalid characters
            newConfiguration.Key = "key12345-!";
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.Key starts with invalid character
            newConfiguration.Key = "-key12345";
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);

            newConfiguration.Key = TestParameters.ConfigurationKey;

            // Perform Method to test -> BadRequest on configuration.Value = null
            newConfiguration.Value = null;
            result = await controller.AddConfigurationAsync(new CancellationToken(), newConfiguration);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddConfiguration_Conflict()
        {
            var newConfiguration = CreatePostConfiguration();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddConfigurationAsync(It.IsAny<PostConfiguration>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.Conflict });

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.AddConfigurationAsync(CancellationToken.None, newConfiguration).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.Conflict);
            }
        }

        [TestMethod]
        public async Task AddConfiguration_InternalServerError()
        {
            var newConfiguration = CreatePostConfiguration();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddConfigurationAsync(It.IsAny<PostConfiguration>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.AddConfigurationAsync(CancellationToken.None, newConfiguration).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateConfigurationAsync Tests

        [TestMethod]
        public async Task UpdateConfiguration_NoContent()
        {
            var newConfiguration = CreatePutConfiguration();

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateConfiguration_BadRequest()
        {
            var newConfiguration = CreatePutConfiguration();

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on configurationKey == null
            var result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, TestParameters.EnvironmentSubscriptionId, null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configurationKey == ""
            result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, TestParameters.EnvironmentSubscriptionId, "");
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId == null
            result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, null, TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId == ""
            result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, "", TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration = null 
            result = await controller.UpdateConfigurationAsync(new CancellationToken(), null, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configuration.Value = null 
            newConfiguration.Value = null;
            result = await controller.UpdateConfigurationAsync(new CancellationToken(), newConfiguration, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateConfiguration_NotFound()
        {
            var newConfiguration = CreatePutConfiguration();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutConfiguration>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.UpdateConfigurationAsync(CancellationToken.None, newConfiguration, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateConfiguration_InternalServeError()
        {
            var newConfiguration = CreatePutConfiguration();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PutConfiguration>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.UpdateConfigurationAsync(CancellationToken.None, newConfiguration, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteConfigurationAsync Tests

        [TestMethod]
        public async Task DeleteConfiguration_Accepted()
        {
            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            var result = await controller.DeleteConfigurationAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey);
            
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteConfiguration_BadRequest()
        {
            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = null
            var result = await controller.DeleteConfigurationAsync(new CancellationToken(), null, TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = ""
            result = await controller.DeleteConfigurationAsync(new CancellationToken(), "", TestParameters.ConfigurationKey);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configurationKey = null
            result = await controller.DeleteConfigurationAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on configurationKey = ""
            result = await controller.DeleteConfigurationAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, "");
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteConfiguration_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.DeleteConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteConfiguration_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteConfigurationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new ConfigurationController(_businessLogic.Object);

            try
            {
                // Perform Method to test 
                await controller.DeleteConfigurationAsync(CancellationToken.None, TestParameters.EnvironmentSubscriptionId, TestParameters.ConfigurationKey).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private ConfigurationController SetupController(IMasterdataManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            httpContext.Request.QueryString = new QueryString(queryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new ConfigurationController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }


        private static GetConfiguration CreateGetConfiguration()
        {
            return new GetConfiguration
            {
                Id = TestParameters.ValidId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Key = TestParameters.ConfigurationKey,
                Value = TestParameters.ConfigurationValue,
                Description = TestParameters.Description
            };
        }

        private static PostConfiguration CreatePostConfiguration()
        {
            return new PostConfiguration
            {
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Key = TestParameters.ConfigurationKey,
                Value = TestParameters.ConfigurationValue,
                Description = TestParameters.Description
            };
        }

        private static PutConfiguration CreatePutConfiguration()
        {
            return new PutConfiguration
            {
                Value = TestParameters.ConfigurationValue,
                Description = TestParameters.Description
            };
        }

        #endregion
    }
}
