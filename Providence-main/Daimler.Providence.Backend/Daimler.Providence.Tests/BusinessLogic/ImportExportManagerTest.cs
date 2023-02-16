using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL.Interfaces;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.ImportExport;
using Daimler.Providence.Service.Models.MasterData.Action;
using Daimler.Providence.Service.Models.MasterData.Environment;
using Daimler.Providence.Service.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Daimler.Providence.Tests.BusinessLogic
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ImportExportManagerTest
    {
        #region Private Members

        private Mock<IStorageAbstraction> _dataAccessLayer;
        private Mock<IEnvironmentManager> _environmentManager;

        #endregion

        #region TestInitialization

        [TestInitialize]
        public void TestInitialization()
        {
            _dataAccessLayer = new Mock<IStorageAbstraction>();
            _environmentManager = new Mock<IEnvironmentManager>();

            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", false, true)
               .AddEnvironmentVariables();

            var Configuration = builder.Build();
            ProvidenceConfigurationManager.SetConfiguration(Configuration);
        }

        #endregion

        #region ImportEnvironmentAsync Tests

        [TestMethod]
        public async Task ImportEnvironmentAsync_ReplacedEnvironment()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(0);

            result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.True, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(0);

            result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.False, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(0);
        }

        [TestMethod]
        public async Task ImportEnvironmentAsync_NotReplacedEnvironment_BadRequest()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            try
            {
                // Perform Method to test -> BadRequest on invalid EnvironmentName
                await businessLogic.ImportEnvironmentAsync(newEnvironment, string.Empty, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.BadRequest);
            }
        }

        [TestMethod]
        public async Task ImportEnvironmentAsync_NotReplacedEnvironment_NotFound()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException { Status = HttpStatusCode.NotFound });

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);
            try
            {
                // Perform Method to test -> NotFound on invalid EnvironmentSubscriptionId
                await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            }
            catch (ProvidenceException pe)
            {
                pe.Status.ShouldBe(HttpStatusCode.NotFound);
            }
        }

        #endregion

        #region ValidateEnvironment Tests

        [TestMethod]
        public async Task ValidateEnvironment_DuplicateElementIds()
        {
            var newEnvironment = CreateEnvironmentWithDuplicates();

            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task ValidateEnvironment_InvalidServiceActionAssignments()
        {
            var newEnvironment = CreateEnvironmentWithInvalidServiceActionAssignments();

            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task ValidateEnvironment_InvalidServiceActionAssignments_ActionAssignedToOtherService()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // Action defined in Environment is already assigned to another Service
            _dataAccessLayer.Setup(mock => mock.GetActions(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetAction>
                {
                    new GetAction
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId },
                        ServiceElementId = TestParameters.ServiceElementId + "2"
                    }
                }));

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task ValidateEnvironment_MultipleUsageOfSameElementId()
        {
            var newEnvironment = CreateEnvironmentWithMultipleUsageOfSameElementId();

            // Setup Mock
            SetupValidMock();

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.All, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task ValidateEnvironment_ElementWithSameElementIdExists_Component()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // GetComponent throws Exception -> An Component within the Payload has the same ElementId as an Element which is not a Component on the DB
            _dataAccessLayer.Setup(mock => mock.GetComponent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException());

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.True, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task ValidateEnvironment_ElementWithSameElementIdExists_Action()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // GetComponent throws Exception -> An Component within the Payload has the same ElementId as an Element which is not a Component on the DB
            _dataAccessLayer.Setup(mock => mock.GetAction(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException());

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.True, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task ValidateEnvironment_ElementWithSameElementIdExists_Service()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // GetComponent throws Exception -> An Component within the Payload has the same ElementId as an Element which is not a Component on the DB
            _dataAccessLayer.Setup(mock => mock.GetService(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException());

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.True, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task ValidateEnvironment_ElementWithSameElementIdExists_Check()
        {
            var newEnvironment = CreateValidEnvironment();

            // Setup Mock
            SetupValidMock();

            // GetComponent throws Exception -> An Component within the Payload has the same ElementId as an Element which is not a Component on the DB
            _dataAccessLayer.Setup(mock => mock.GetCheck(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new ProvidenceException());

            // Create BusinessLogic with Mock
            var businessLogic = ManagerBuilder.CreateImportExportManager(_dataAccessLayer.Object, _environmentManager.Object);

            // Perform Method to test
            var result = await businessLogic.ImportEnvironmentAsync(newEnvironment, TestParameters.EnvironmentName, TestParameters.EnvironmentSubscriptionId, ReplaceFlag.True, CancellationToken.None).ConfigureAwait(false);
            result.ShouldNotBe(null);
            result.Count.ShouldBe(1);
        }

        #endregion

        #region Private Methods   

        private void SetupValidMock()
        {
            _dataAccessLayer.Setup(mock => mock.GetEnvironment(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CreateGetEnvironment()));

            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironment(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>
                {
                    new EnvironmentElement{ElementId = TestParameters.EnvironmentSubscriptionId},
                    new EnvironmentElement{ElementId = TestParameters.ServiceElementId},
                    new EnvironmentElement{ElementId = TestParameters.ActionElementId},
                    new EnvironmentElement{ElementId = TestParameters.ComponentElementId}
                }));

            _dataAccessLayer.Setup(mock => mock.GetAllElementsOfEnvironmentTree(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<EnvironmentElement>
                {
                    new EnvironmentElement{ElementId = TestParameters.EnvironmentSubscriptionId, ElementType = "Element"},
                    new EnvironmentElement{ElementId = TestParameters.ServiceElementId, ElementType = "Element"},
                    new EnvironmentElement{ElementId = TestParameters.ActionElementId, ElementType = "Element"},
                    new EnvironmentElement{ElementId = TestParameters.ComponentElementId, ElementType = "Element"},
                    new EnvironmentElement{ElementId = TestParameters.CheckId, ElementType = "Element"}
                }));

            _dataAccessLayer.Setup(mock => mock.GetActions(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetAction>
                {
                    new GetAction
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId },
                        ServiceElementId = TestParameters.ServiceElementId
                    }
                }));

            _dataAccessLayer.Setup(mock => mock.GetElementCreationDate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(TestParameters.PastTime));
        }

        private static GetEnvironment CreateGetEnvironment()
        {
            var getEnvironment = new GetEnvironment
            {
                Id = TestParameters.ValidId,
                Name = TestParameters.EnvironmentName,
                Description = TestParameters.Description,
                SubscriptionId = TestParameters.EnvironmentSubscriptionId,
                CreateDate = TestParameters.PastTime,
                IsDemo = true
            };
            return getEnvironment;
        }

        private static Environment CreateValidEnvironment()
        {
            return new Environment
            {
                Services = new List<Service.Models.ImportExport.Service>
                {
                    new Service.Models.ImportExport.Service
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Actions = new List<string>{ TestParameters.ActionElementId }
                    }
                },
                Actions = new List<Action>
                {
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId }
                    }
                },
                Components = new List<Component>
                {
                    new Component
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ComponentElementId,
                        Description = TestParameters.Description,
                        ComponentType = TestParameters.ComponentType
                    }
                },
                Checks = new List<Check>
                {
                    new Check
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.CheckId,
                        Description = TestParameters.Description,
                        Frequency = TestParameters.Frequency,
                    }
                }
            };
        }

        private static Environment CreateEnvironmentWithDuplicates()
        {
            return new Environment
            {
                Services = new List<Service.Models.ImportExport.Service>
                {
                    new Service.Models.ImportExport.Service
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Actions = new List<string>{ TestParameters.ActionElementId }
                    }
                },
                Actions = new List<Action>
                {
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId }
                    },
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId }
                    }
                },
                Components = new List<Component>
                {
                    new Component
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ComponentElementId,
                        Description = TestParameters.Description,
                        ComponentType = TestParameters.ComponentType
                    }
                },
                Checks = new List<Check>
                {
                    new Check
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.CheckId,
                        Description = TestParameters.Description,
                        Frequency = TestParameters.Frequency,
                    },
                    new Check
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.CheckId,
                        Description = TestParameters.Description,
                        Frequency = TestParameters.Frequency,
                    }
                }
            };
        }

        private static Environment CreateEnvironmentWithInvalidServiceActionAssignments()
        {
            return new Environment
            {
                Services = new List<Service.Models.ImportExport.Service>
                {
                    new Service.Models.ImportExport.Service
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Actions = new List<string>{ TestParameters.ActionElementId }
                    },
                    new Service.Models.ImportExport.Service
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId + "2",
                        Description = TestParameters.Description,
                        Actions = new List<string>{ TestParameters.ActionElementId, TestParameters.ActionElementId + "3" } // Double assignment for Action and invalid ActionId
                    }
                },
                Actions = new List<Action>
                {
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId }
                    },
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ActionElementId + "2", // No assignment for Action
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ComponentElementId }
                    }
                },
                Components = new List<Component>
                {
                    new Component
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ComponentElementId,
                        Description = TestParameters.Description,
                        ComponentType = TestParameters.ComponentType
                    }
                },
                Checks = new List<Check>
                {
                    new Check
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.CheckId,
                        Description = TestParameters.Description,
                        Frequency = TestParameters.Frequency,
                    }
                }
            };
        }

        private static Environment CreateEnvironmentWithMultipleUsageOfSameElementId()
        {
            return new Environment
            {
                Services = new List<Service.Models.ImportExport.Service>
                {
                    new Service.Models.ImportExport.Service
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Actions = new List<string>{ TestParameters.ServiceElementId }
                    }
                },
                Actions = new List<Action>
                {
                    new Action
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Components = new List<string>{ TestParameters.ServiceElementId }
                    }
                },
                Components = new List<Component>
                {
                    new Component
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        ComponentType = TestParameters.ComponentType
                    }
                },
                Checks = new List<Check>
                {
                    new Check
                    {
                        Name = TestParameters.Name,
                        ElementId = TestParameters.ServiceElementId,
                        Description = TestParameters.Description,
                        Frequency = TestParameters.Frequency,
                    }
                }
            };
        }

        #endregion
    }
}
