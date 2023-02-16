using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Controllers;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.AlertComment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.Controller
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AlertCommentControllerTests
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

        #region GetAlertComments Tests

        [TestMethod]
        public async Task GetAlertCommentsAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetAlertComment> {CreateGetAlertComment()}));

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.GetAlertCommentsAsync(CancellationToken.None).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetAlertCommentsAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsAsync(It.IsAny<CancellationToken>())).Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertCommentsAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region GetAlertCommentsByRecordIdAsync Tests

        [TestMethod]
        public async Task GetAlertCommentsByRecordIdAsyncTest_OK()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsByRecordIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetAlertComment> { CreateGetAlertComment() }));

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.GetAlertCommentsByRecordIdAsync(CancellationToken.None, TestParameters.ValidRecordId.ToString()).ConfigureAwait(false);
            TestHelper.AssertOkRequest(response);
        }

        [TestMethod]
        public async Task GetAlertCommentsByRecordIdAsyncTest_BadRequest()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsByRecordIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => new List<GetAlertComment> { CreateGetAlertComment() }));

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on Invalid RecordId
            var response = await controller.GetAlertCommentsByRecordIdAsync(CancellationToken.None, TestParameters.InvalidRecordId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task GetAlertCommentsByRecordIdAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsByRecordIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertCommentsByRecordIdAsync(CancellationToken.None, TestParameters.ValidRecordId.ToString()).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task GetAlertCommentsByRecordIdAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.GetAlertCommentsByRecordIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.GetAlertCommentsByRecordIdAsync(CancellationToken.None, TestParameters.ValidRecordId.ToString()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region AddAlertCommentsAsync Tests

        [TestMethod]
        public async Task AddAlertCommentAsyncTest_Created()
        {
            var newAlertComment = CreatePostAlertComment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertCommentAsync(It.IsAny<PostAlertComment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Run(() => CreateGetAlertComment()));

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertCreatedRequest(response);
        }

        [TestMethod]
        public async Task AddAlertCommentAsyncTest_BadRequest()
        {
            var newAlertComment = CreatePostAlertComment();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test -> BadRequest on alertComment = null
            var response = await controller.AddAlertCommentAsync(CancellationToken.None, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);

            // Perform Method to test -> BadRequest on alertComment.RecordId = null
            newAlertComment.RecordId = null;
            response = await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.RecordId = TestParameters.ValidRecordId.ToString();

            // Perform Method to test -> BadRequest on alertComment.Comment = null
            newAlertComment.Comment = null;
            response = await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.Comment = TestParameters.Comment;

            // Perform Method to test -> BadRequest on alertComment.User = null
            newAlertComment.User = null;
            response = await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.User = TestParameters.User;

            // Perform Method to test -> BadRequest on alertComment.State = undefined
            newAlertComment.State = (ProgressState) 10;
            response = await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.State = ProgressState.Done;
        }

        [TestMethod]
        public async Task AddAlertCommentAsyncTest_NotFound()
        {
            var newAlertComment = CreatePostAlertComment();

            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertCommentAsync(It.IsAny<PostAlertComment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task AddAlertCommentAsyncTest_InternalServerError()
        {
            var newAlertComment = CreatePostAlertComment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.AddAlertCommentAsync(It.IsAny<PostAlertComment>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.AddAlertCommentAsync(CancellationToken.None, newAlertComment).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region UpdateAlertCommentAsync Tests

        [TestMethod]
        public async Task UpdateAlertCommentAsyncTest_NoContent()
        {
            var newAlertComment = CreatePutAlertComment();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertNoContentRequest(response);
        }

        [TestMethod]
        public async Task UpdateAlertCommentAsyncTest_BadRequest()
        {
            var newAlertComment = CreatePutAlertComment();
            
            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);
            
            // Perform Method to test -> BadRequest on alertComment = null
            var response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, null).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);

            // Perform Method to test -> BadRequest on alertComment.Comment = null
            newAlertComment.Comment = null;
            response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.Comment = TestParameters.Comment;

            // Perform Method to test -> BadRequest on alertComment.User = null
            newAlertComment.User = null;
            response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.User = TestParameters.User;

            // Perform Method to test -> BadRequest on alertComment.State = undefined
            newAlertComment.State = (ProgressState)10;
            response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
            newAlertComment.State = ProgressState.Done;

            // Perform Method to test -> BadRequest on id = 0
            response = await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.InvalidId, newAlertComment).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task UpdateAlertCommentAsyncTest_NotFound()
        {
            var newAlertComment = CreatePutAlertComment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateAlertCommentAsync(It.IsAny<int>(), It.IsAny<PutAlertComment>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException{Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);
           
            try
            {
                // Perform Method to test
                await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task UpdateAlertCommentAsyncTest_InternalServerError()
        {
            var newAlertComment = CreatePutAlertComment();
            
            // Setup Mock
            _businessLogic.Setup(mock => mock.UpdateAlertCommentAsync(It.IsAny<int>(), It.IsAny<PutAlertComment>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.UpdateAlertCommentAsync(CancellationToken.None, TestParameters.ValidId, newAlertComment).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region DeleteAlertCommentAsync Tests

        [TestMethod]
        public async Task DeleteAlertCommentAsyncTest_Accepted()
        {
            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // Perform Method to test
            var response = await controller.DeleteAlertCommentAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            TestHelper.AssertAcceptedRequest(response);
        }

        [TestMethod]
        public async Task DeleteAlertCommentAsyncTest_BadRequest()
        {
            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            // BadRequest on id = 0
            var response = await controller.DeleteAlertCommentAsync(CancellationToken.None, TestParameters.InvalidId).ConfigureAwait(false);
            TestHelper.AssertBadRequest(response);
        }

        [TestMethod]
        public async Task DeleteAlertCommentAsyncTest_NotFound()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteAlertCommentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws( new ProvidenceException{ Status = HttpStatusCode.NotFound});

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteAlertCommentAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task DeleteAlertCommentAsyncTest_InternalServerError()
        {
            // Setup Mock
            _businessLogic.Setup(mock => mock.DeleteAlertCommentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Create Controller with Mock
            var controller = new AlertCommentController(_businessLogic.Object);

            try
            {
                // Perform Method to test
                await controller.DeleteAlertCommentAsync(CancellationToken.None, TestParameters.ValidId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var isProvidenceException = e is ProvidenceException;
                isProvidenceException.ShouldBe(false);
            }
        }

        #endregion

        #region Private Methods

        private static GetAlertComment CreateGetAlertComment()
        {
            var getAlertComment = new GetAlertComment
            {
                Id = TestParameters.ValidId,
                User = TestParameters.User,
                Comment = TestParameters.Comment,
                State = ProgressState.Done
            };
            return getAlertComment;
        }

        private static PostAlertComment CreatePostAlertComment()
        {
            var postAlertComment = new PostAlertComment
            {
                User = TestParameters.User,
                Comment = TestParameters.Comment,
                RecordId = TestParameters.ValidRecordId.ToString(),
                State = ProgressState.Done,
            };
            return postAlertComment;
        }

        private static PutAlertComment CreatePutAlertComment()
        {
            var putAlertComment = new PutAlertComment
            {
                User = TestParameters.User,
                Comment = TestParameters.Comment,
                State = ProgressState.Done,
            };
            return putAlertComment;
        }

        #endregion
    }
}