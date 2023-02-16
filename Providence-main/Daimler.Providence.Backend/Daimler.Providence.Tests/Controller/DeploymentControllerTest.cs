using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
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
    public class DeploymentControllerTest
    {
        #region Private Members

        private Mock<IMasterdataManager> _businessLogic;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IMasterdataManager>();
        }

        #endregion
        
        #region GetDeploymentHistoryAsync Tests

        [TestMethod]
        public async Task GetDeploymentHistoryAsync_OK()
        {
            var validHistory = CreateDeploymentHistory();

            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(validHistory));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}"); 
          
            // Perform Method to test
            var result = await controller.GetDeploymentHistoryAsync(new CancellationToken(false), TestParameters.EnvironmentName);

            var content = TestHelper.AssertContentRequestString(result) ;
            content.ShouldBe(JsonConvert.SerializeObject(validHistory));
        }

        [TestMethod]
        public async Task GetDeploymentHistoryAsync_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateDeploymentHistory()));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.FutureTime}&{RequestParameters.EndDate}={TestParameters.PastTime}");
           
            // Perform Method to test -> BadRequest on startDate >= endDate
            var result = await controller.GetDeploymentHistoryAsync(new CancellationToken(false), TestParameters.EnvironmentName);
            TestHelper.AssertBadRequest(result);
            

            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}");
                     
            // Perform Method to test -> BadRequest on environmentName = ""
            result = await controller.GetDeploymentHistoryAsync(new CancellationToken(false), "");
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentName = null
            result = await controller.GetDeploymentHistoryAsync(new CancellationToken(false), null);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task GetDeploymentHistoryAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}");
                      
            try
            {
                // Perform Method to test
                await controller.GetDeploymentHistoryAsync(new CancellationToken(false), TestParameters.EnvironmentName);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }
       
        [TestMethod]
        public async Task GetDeploymentHistoryAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}");
                      
            try
            {
                // Perform Method to test
                await controller.GetDeploymentHistoryAsync(new CancellationToken(false), TestParameters.EnvironmentName);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }
        
        #endregion

        #region GetDeploymentsAsync Tests
        
        [TestMethod]
        public async Task GetDeploymentsAsync_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetDeployment> { CreateGetDeployment(false) }));

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.GetDeploymentsAsync(new CancellationToken());
            TestHelper.AssertOkRequest(result);

            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetDeployment>()));

            // Create Controller with Mock
            controller = new DeploymentController(_businessLogic.Object);
    
            // Perform Method to test
            result = await controller.GetDeploymentsAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId);
            TestHelper.AssertOkRequest(result);

            // ----------- Recurring Deployments

            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetDeployment> { CreateGetDeployment(true) }));

            // Create Controller with Mock
            controller = new DeploymentController(_businessLogic.Object);
          
            // Perform Method to test
            result = await controller.GetDeploymentsAsync(new CancellationToken());
            TestHelper.AssertOkRequest(result);
            
            var deployments = (List<GetDeployment>)TestHelper.AssertContentRequestType(result, typeof(List<GetDeployment>));
            deployments.ShouldAllBe(d => d.ParentId == null);
        }

        [TestMethod]
        public async Task GetDeploymentsAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetDeploymentsAsync(new CancellationToken());
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetDeploymentsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetDeploymentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetDeploymentsAsync(new CancellationToken());
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }
        
        #endregion

        #region AddDeploymentAsync Tests
        
        [TestMethod]
        public async Task AddDeploymentAsync_Created()
        {
            var newDeployment = CreatePostDeployment();

            // Setup Mock
            _businessLogic.Setup(mock =>  mock.AddDeploymentAsync(It.IsAny<PostDeployment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateGetDeployment(false)));

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);

            TestHelper.AssertCreatedRequest(result).ToString().ShouldNotBeNullOrEmpty();
            
        }
        
        [TestMethod]
        public async Task AddDeploymentAsync_BadRequest()
        {
            var newDeployment = CreatePostDeployment();
            
            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);
         
            // Perform Method to test -> BadRequest on deployment = null
            var result = await controller.AddDeploymentAsync(new CancellationToken(), null);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on deployment.EnvironmentSubscriptionId = ""
            newDeployment.EnvironmentSubscriptionId = "";
            result = await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            TestHelper.AssertBadRequest(result);

            newDeployment.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on deployment.ElementIds is empty
            newDeployment.ElementIds = new List<string>();
            result = await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            TestHelper.AssertBadRequest(result);

            newDeployment.ElementIds = new List<string> { TestParameters.ComponentElementId };

            // Perform Method to test -> BadRequest on deployment.StartDate >= deployment.EndData
            newDeployment.StartDate = TestParameters.FutureTime;
            newDeployment.EndDate = TestParameters.PastTime;
            result = await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            TestHelper.AssertBadRequest(result);

            newDeployment.StartDate = TestParameters.PastTime;
            newDeployment.EndDate = TestParameters.FutureTime;

            // ----------- Recurring Deployments

            // Perform Method to test -> BadRequest on deployment.StartDate >= deployment.RepeatInformation.RepeatUntil 
            newDeployment.StartDate = TestParameters.CurrentTime;
            newDeployment.RepeatInformation = new RepeatInformation {RepeatUntil = TestParameters.PastTime};
            result = await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task AddDeploymentAsync_NotFound()
        {
            var newDeployment = CreatePostDeployment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.AddDeploymentAsync(It.IsAny<PostDeployment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddDeploymentAsync_InternalServerError()
        {
            var newDeployment = CreatePostDeployment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddDeploymentAsync(It.IsAny<PostDeployment>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                await controller.AddDeploymentAsync(new CancellationToken(), newDeployment);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }
        
        #endregion

        #region UpdateDeploymentAsync Tests
        
        [TestMethod]
        public async Task UpdateDeploymentAsync_NoContent()
        {
            var newDeployment = CreatePutDeployment();
            
            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test 
            var result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId); 
            
            TestHelper.AssertNoContentRequest(result);

            // If no StartDate is provided -> UtcNow should be used
            newDeployment.StartDate = DateTime.MinValue;

            // Perform Method to test 
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertNoContentRequest(result);
        }

        [TestMethod]
        public async Task UpdateDeploymentAsync_BadRequest()
        {
            var newDeployment = CreatePutDeployment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateDeploymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<PutDeployment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on deployment = null
            var result = await controller.UpdateDeploymentAsync(new CancellationToken(), null, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = null
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, null, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on id <= 1
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.InvalidId);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on deployment.StartDate >= deployment.EndData
            newDeployment.StartDate = TestParameters.FutureTime;
            newDeployment.EndDate = TestParameters.PastTime;
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);
            newDeployment.StartDate = TestParameters.PastTime;
            newDeployment.EndDate = TestParameters.FutureTime;

            // Perform Method to test -> BadRequest on deployment.ElementIds = null
            newDeployment.ElementIds = null;
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);

            newDeployment.ElementIds = new List<string> { TestParameters.ComponentElementId };

            // Perform Method to test -> BadRequest on deployment.ElementIds is empty
            newDeployment.ElementIds = new List<string>();
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);

            newDeployment.ElementIds = new List<string> { TestParameters.ComponentElementId };

            // ----------- Recurring Deployments

            // Perform Method to test -> BadRequest on deployment.StartDate >= deployment.RepeatInformation.RepeatUntil 
            newDeployment.StartDate = TestParameters.CurrentTime;
            newDeployment.RepeatInformation = new RepeatInformation { RepeatUntil = TestParameters.PastTime };
            result = await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task UpdateDeploymentAsync_NotFound()
        {
            var newDeployment = CreatePutDeployment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateDeploymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<PutDeployment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateDeploymentAsync_InternalServerError()
        {
            var newDeployment = CreatePutDeployment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateDeploymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<PutDeployment>(),It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);
       
            try
            {
                // Perform Method to test
                await controller.UpdateDeploymentAsync(new CancellationToken(), newDeployment, TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }
        
        #endregion

        #region DeleteDeploymentAsync Tests

        [TestMethod]
        public async Task DeleteDeploymentAsync_Accepted()
        {
            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test
            var result = await controller.DeleteDeploymentAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            TestHelper.AssertAcceptedRequest(result);
        }

        [TestMethod]
        public async Task DeleteDeploymentAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on environmentSubscriptionId = null
            var result = await controller.DeleteDeploymentAsync(new CancellationToken(), null, TestParameters.ValidId);
            TestHelper.AssertBadRequest(result);

            // Perform Method to test -> BadRequest on id <= 1
            result = await controller.DeleteDeploymentAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, TestParameters.InvalidId);
            TestHelper.AssertBadRequest(result);
        }

        [TestMethod]
        public async Task DeleteDeploymentAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteDeploymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteDeploymentAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteDeploymentAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteDeploymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new DeploymentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteDeploymentAsync(new CancellationToken(), TestParameters.EnvironmentSubscriptionId, TestParameters.ValidId);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private DeploymentController SetupController(IMasterdataManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`

            httpContext.Request.QueryString = new QueryString(queryString);
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new DeploymentController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }


        private static List<GetDeployment> CreateDeploymentHistory()
        {
            return new List<GetDeployment> { new GetDeployment() };
        }

        private static GetDeployment CreateGetDeployment(bool recurring)
        {
            var deployment = new GetDeployment
            {
                Id = TestParameters.ValidId,
                EnvironmentName = TestParameters.EnvironmentName,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Description = TestParameters.Description,
                ShortDescription = TestParameters.ShortDescription,
                CloseReason = TestParameters.CloseReason,
                StartDate = TestParameters.PastTime,
                EndDate = TestParameters.FutureTime
            };
            deployment.Length = (int) deployment.EndDate?.Subtract(deployment.StartDate).TotalMilliseconds;
            if (recurring)
            {
                deployment.RepeatInformation = TestParameters.RepeatInformation;
            }
            return deployment;
        }

        private static PostDeployment CreatePostDeployment()
        {
            var postDeployment = new PostDeployment
            {
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                ElementIds = new List<string> { TestParameters.ComponentElementId },
                Description = TestParameters.Description,
                ShortDescription = TestParameters.ShortDescription,
                CloseReason = TestParameters.CloseReason,
                StartDate = TestParameters.PastTime,
                EndDate = TestParameters.FutureTime,
                RepeatInformation = null
            };
            return postDeployment;
        }
        
        private static PutDeployment CreatePutDeployment()
        {
            var putDeployment = new PutDeployment
            {
                ElementIds = new List<string> { TestParameters.ComponentElementId },
                Description = TestParameters.Description,
                ShortDescription = TestParameters.ShortDescription,
                CloseReason = TestParameters.CloseReason,
                StartDate = TestParameters.PastTime,
                EndDate = TestParameters.FutureTime,
                RepeatInformation = null
            };
            return putDeployment;
        }

        #endregion
    }
}