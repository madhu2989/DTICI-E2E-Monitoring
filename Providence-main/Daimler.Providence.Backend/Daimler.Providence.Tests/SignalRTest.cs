using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.SignalR;
using Daimler.Providence.Service.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Daimler.Providence.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SignalRTest
    {
        Mock<IHubCallerClients> mockClients;
        Mock<IClientProxy> mockClientProxy;
        DeviceEventHub hub;


        
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
                
                .AddJsonFile("appsettings.json", false, true)
                
                .AddEnvironmentVariables();

           
            var Configuration = builder.Build();
            ProvidenceConfigurationManager.SetConfiguration(Configuration);



            // Arrange
            mockClients = new Mock<IHubCallerClients>();
            mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);


            hub = new DeviceEventHub(new ClientRepository(null))
            {
                Clients = mockClients.Object
            };
        }

        private void AssertClient(string methodName)
        {
            // assert
            mockClients.Verify(clients => clients.All, Times.Once);

            mockClientProxy.Verify(
                clientProxy => clientProxy.SendCoreAsync(
                    methodName,
                    It.Is<object[]>(o => o != null && o.Length == 1),
                    default(CancellationToken)),
                Times.Once);
        }



         [TestMethod]
        public void TestSendingHeartbeatMessage()
        {

            Setup();


            var heartBeatMsg = new HeartbeatMsg
            {
                LogSystemState = State.Ok,
                EnvironmentName = TestParameters.EnvironmentName,
                TimeStamp = DateTime.UtcNow
            };

            // act
            hub.SendHeartbeat(heartBeatMsg);

            // assert
            AssertClient("updateHeartbeat");
            
            
           
        }




        

        [TestMethod]
        public void TestSendTreeUpdate()
        {
            Setup();

            hub.SendTreeUpdate(TestParameters.EnvironmentName);

            
            // assert
            AssertClient("updateTree");

           
        }

        [TestMethod]
        public void TestSendTreeDeletion()
        {
            Setup();

            hub.SendTreeDeletion(TestParameters.EnvironmentName);

            // assert
            

            AssertClient("deleteTree");

           
        }

        

        
        [TestMethod]
        public void TestSendingStateTransitions()
        {
            Setup();

            var transition = new StateTransition();
            var list = new List<StateTransition> { transition };

            hub.SendStateTransitions(list);

            AssertClient("updateStateTransitions");

            
        }

        [TestMethod]
        public void TestSendingDeploymentWindows()
        {
            Setup();
            hub.SendDeploymentWindows(TestParameters.EnvironmentName);

            AssertClient("updateDeploymentWindows");
        }

        [TestMethod]
        public void TestClientRepo()
        {
            var clientRepo = new ClientRepository(null);

            clientRepo.RegisterClient("CONNECTIONID");

            Assert.IsTrue(clientRepo.IsRegisteredClient("CONNECTIONID"));

            clientRepo.UnregisterClient("CONNECTIONID");

            Assert.IsFalse(clientRepo.IsRegisteredClient("CONNECTIONID"));
        }


        
        
        [TestMethod]
        public void TestHubRegisterUnregisterClient()
        {

            var hub = new DeviceEventHub(new ClientRepository(null));
            
            Mock<HubCallerContext> mockClientContext = new Mock<HubCallerContext>();
            mockClientContext.Setup(c => c.ConnectionId).Returns(Guid.NewGuid().ToString);
            hub.Context = mockClientContext.Object;
                        

            Assert.IsTrue(hub.RegisterClient());
            Assert.IsTrue(hub.UnregisterClient());
            Assert.IsNotNull(hub.OnConnectedAsync().ConfigureAwait(true));
            Assert.IsNotNull(hub.OnDisconnectedAsync(null).ConfigureAwait(true));
        }
        
       
        
    }
}
