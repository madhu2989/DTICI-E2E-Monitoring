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
using Daimler.Providence.Service.Models.InternalJob;
using Daimler.Providence.Service.Models.SLA;
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
    public class InternalJobControllerTest
    {
        #region Private Members

        private Mock<IInternalJobManager> _businessLogic;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialization()
        {
            _businessLogic = new Mock<IInternalJobManager>();
        }

        #endregion

        #region StartInternalJobAsync Tests

        [TestMethod]
        public async Task StartInternalJobAsync_Created()
        {
            var newInternalJob = CreatePostInternalJob();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.StartDate}={TestParameters.PastTime}&{RequestParameters.EndDate}={TestParameters.FutureTime}"); 
          
            // Perform Method to test
            var response = await controller.StartInternalJobAsync(new CancellationToken(false), newInternalJob);
            TestHelper.AssertCreatedRequest(response);
        }

        [TestMethod]
        public async Task StartInternalJobAsync_BadRequest()
        {
            var newInternalJob = CreatePostInternalJob();

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test -> BadRequest on internalJob = null
            var response = await controller.StartInternalJobAsync(new CancellationToken(false), null);
            TestHelper.AssertBadRequest(response);

            // Perform Method to test -> BadRequest on internalJob.EnvironmentSubscriptionId = ""
            newInternalJob.EnvironmentSubscriptionId = "";
            response = await controller.StartInternalJobAsync(new CancellationToken(), newInternalJob);
            TestHelper.AssertBadRequest(response);

            newInternalJob.EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId;

            // Perform Method to test -> BadRequest on internalJob.StartDate > internalJob.EndData
            newInternalJob.StartDate = TestParameters.FutureTime;
            newInternalJob.EndDate = TestParameters.PastTime;
            response = await controller.StartInternalJobAsync(new CancellationToken(), newInternalJob);
            TestHelper.AssertBadRequest(response);

            newInternalJob.StartDate = TestParameters.PastTime;
            newInternalJob.EndDate = TestParameters.FutureTime;
        }

        [TestMethod]
        public async Task StartInternalJobAsync_NotFound()
        {
            var newInternalJob = CreatePostInternalJob();

            // Setup Mock
            _businessLogic.Setup(mock => mock.EnqueueInternalJobAsync(It.IsAny<PostInternalJob>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            try
            {
                // Perform Method to test
                await controller.StartInternalJobAsync(new CancellationToken(), newInternalJob);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task StartInternalJobAsync_InternalServerError()
        {
            var newInternalJob = CreatePostInternalJob();

            // Setup Mock
            _businessLogic.Setup(mock => mock.EnqueueInternalJobAsync(It.IsAny<PostInternalJob>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            try
            {
                await controller.StartInternalJobAsync(new CancellationToken(), newInternalJob);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetInternalJobDataAsync Tests

        [TestMethod]
        public async Task GetInternalJobDataAsync_Ok()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetInternalJobDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(JsonConvert.SerializeObject(CreateSlaBlobRecord())));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            var response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertOkRequest(response);

            // With valid ElementId
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ActionElementId}");

            // Perform Method to test
            response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertOkRequest(response);

            // History with valid ElementId
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ActionElementId}&{RequestParameters.SlaHistory}=true");

            // Perform Method to test
            response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertOkRequest(response);

            // History without ElementId
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.SlaHistory}=true");

            // Perform Method to test
            response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetInternalJobDataAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test -> BadRequest on invalid id
            var response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.InvalidId);
            TestHelper.AssertBadRequest(response);           
        }

        [TestMethod]
        public async Task GetInternalJobDataAsync_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetInternalJobDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(JsonConvert.SerializeObject(CreateSlaBlobRecord())));

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ActionElementId2}");

            // Perform Method to test -> Not found on invalid ElementId
            var response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertNotFoundRequest(response);

            // Create Controller with Mock
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ActionElementId2}&{RequestParameters.SlaHistory}=true");

            // Perform Method to test -> Not found on invalid ElementId
            response = await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertNotFoundRequest(response);
        }

        [TestMethod]
        public async Task GetInternalJobDataAsync_InternalServerError()
        {
            var newInternalJob = CreatePostInternalJob();

            // Setup Mock
            _businessLogic.Setup(mock => mock.GetInternalJobDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.ElementId}={TestParameters.ActionElementId2}");

            try
            {
                await controller.GetInternalJobDataAsync(new CancellationToken(false), TestParameters.ValidId);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetInternalJobsAsync Tests

        [TestMethod]
        public async Task GetInternalJobsAsync_Ok()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetInternalJobsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetInternalJob> { CreateGetInternalJob() }));

            // Create Controller with Mock (No Queryparameter)
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            var response = await controller.GetInternalJobsAsync(new CancellationToken(false));
            TestHelper.AssertOkRequest(response);
            TestHelper.AssertContentRequest(response);

            // Create Controller with Mock (With valid Queryparameter)
            controller = SetupController(_businessLogic.Object, $"?{RequestParameters.JobType}=Sla");

            // Perform Method to test
            response = await controller.GetInternalJobsAsync(new CancellationToken(false));
            TestHelper.AssertContentRequestList(response, 1);
            TestHelper.AssertContentRequestType(response, typeof(List<GetInternalJob>));
        }

        [TestMethod]
        public async Task GetInternalJobsAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, $"?{RequestParameters.JobType}=Slaa");

            // Perform Method to test -> BadRequest on unkown type
            var response = await controller.GetInternalJobsAsync(new CancellationToken(false));
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task GetInternalJobsAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetInternalJobsAsync(It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            try
            {
                await controller.GetInternalJobsAsync(new CancellationToken(false));
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteInternalJobAsync Tests

        [TestMethod]
        public async Task DeleteInternalJobAsync_Accepted()
        {
            // Create Controller with Mock (No Queryparameter)
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test
            var response = await controller.DeleteInternalJobAsync(new CancellationToken(false), TestParameters.ValidId);
            TestHelper.AssertAcceptedRequest(response);
        }

        [TestMethod]
        public async Task DeleteInternalJobAsync_BadRequest()
        {
            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            // Perform Method to test -> BadRequest on unkown type
            var response = await controller.DeleteInternalJobAsync(new CancellationToken(false), TestParameters.InvalidId);
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task DeleteInternalJobAsync_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteInternalJobAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Create Controller with Mock
            var controller = SetupController(_businessLogic.Object, "");

            try
            {
                await controller.DeleteInternalJobAsync(new CancellationToken(false), TestParameters.ValidId);
            }
            catch (Exception ex)
            {
                var isProvidenceException = ex is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private InternalJobController SetupController(IInternalJobManager businessLogic, string queryString)
        {
            var httpContext = new DefaultHttpContext();
            if(!string.IsNullOrEmpty(queryString))
            {
                httpContext.Request.QueryString = new QueryString(queryString);
            }
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new InternalJobController(businessLogic)
            {
                ControllerContext = controllerContext
            };
            return controller;
        }

        private static SlaBlobRecord CreateSlaBlobRecord()
        {
            var slaBlobRecord = new SlaBlobRecord
            {
                SlaDataPerElement = new Dictionary<string, SlaData>{{TestParameters.ActionElementId, new SlaData()}},
                SlaDataPerElementPerDay = new Dictionary<string, SlaData[]>{{TestParameters.ActionElementId, new SlaData[]{new SlaData()}}}
            };
            return slaBlobRecord;
        }

        private static GetInternalJob CreateGetInternalJob()
        {
            var internalJob = new GetInternalJob
            {
                Id = TestParameters.ValidId,
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Type = TestParameters.JobType,
                UserName = TestParameters.UserName,
                State = (int)TestParameters.JobState,
                StartDate = TestParameters.PastTime,
                EndDate = TestParameters.FutureTime,
                QueuedDate = TestParameters.PastTime,
                FileName = TestParameters.FileName
            };
            return internalJob;
        }

        private static PostInternalJob CreatePostInternalJob()
        {
            var postDeployment = new PostInternalJob
            {
                EnvironmentSubscriptionId = TestParameters.EnvironmentSubscriptionId,
                Type = JobType.Sla,
                StartDate = TestParameters.PastTime,
                EndDate = TestParameters.FutureTime
            };
            return postDeployment;
        }

        #endregion
    }
}