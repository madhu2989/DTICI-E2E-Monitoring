using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Service.Models.EnvironmentTree;
using Daimler.Providence.Service.Models.StateTransition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Action = Daimler.Providence.Service.Models.EnvironmentTree.Action;
using Environment = Daimler.Providence.Service.Models.EnvironmentTree.Environment;

namespace Daimler.Providence.Tests
{
    [ExcludeFromCodeCoverage]
    static class TestDataHelper
    {

        /// <summary>
        /// Returns a list of Environment nodes of variable length
        ///               ________________      ________________
        ///              |  Environment 1 |     |  Environment 2 |  . . . 
        ///              |________________|     |________________|
        /// </summary>
        /// <param name="count"></param>
        /// <returns>List of Environment nodes</returns>
        public static List<Environment> GetSimpleTestEnvironmentList(int count)
        {
            var list = new List<Environment>();

            for (var i = 0; i < count; i++)
            {
                list.Add(new Environment
                {
                    Name = "environment_" + i,
                    ElementId = Guid.NewGuid().ToString(),
                    Id = i,
                    Description = "description Of environment " + i
                });
            }
            return list;
        }

        /// <summary>
        /// Builds a simple test environment tree with a variable amount of services
        ///               _______________
        ///              |  Environment  |
        ///              |_______________|
        /// ______________/            \______________
        /// |  Service 1  |             |  Service 2  |
        /// |_____________|             |_____________|
        /// 
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="numberOfServicesPerEnvironment">Optional, default: 2</param>
        /// <param name="numberOfActionsPerService"></param>
        /// <param name="numberOfComponentsPerAction"></param>
        /// <param name="numberOfChecksPerComponent"></param>
        /// <returns>An environment node with 0..n Services</returns>
        public static Environment GetSimpleTestTree(string environmentName, int numberOfServicesPerEnvironment = 0, int numberOfActionsPerService = 0, int numberOfComponentsPerAction = 0, int numberOfChecksPerComponent = 0)
        {
            var counter = 1;
            var environment = new Environment
            {
                Id = 1,
                Name = environmentName,
                ElementId = environmentName,
                Description = "Testenvironment",
            };

            for (var serviceIndex = 1; serviceIndex <= numberOfServicesPerEnvironment; serviceIndex++)
            {
                counter++;

                var service = new Service.Models.EnvironmentTree.Service
                {
                    Id = counter,
                    Name = "service" + serviceIndex,
                    ElementId = "service" + serviceIndex,
                    Description = "description Of Service " + serviceIndex
                };

                for (var actionIndex = 1; actionIndex <= numberOfActionsPerService; actionIndex++)
                {
                    counter++;

                    var actionId = counter;
                    var action = new Action
                    {
                        Id = actionId,
                        Name = service.Name + "_action" + actionIndex,
                        ElementId = service.Name + "_action" + actionIndex,
                        Description = "description Of Action " + actionId
                    };

                    for (var componentIndex = 1; componentIndex <= numberOfComponentsPerAction; componentIndex++)
                    {
                        counter++;

                        var componentId = counter;
                        var component = new Component
                        {
                            Id = componentId,
                            Name = action.Name + "_component" + componentIndex,
                            ElementId = action.Name + "_component" + componentIndex,
                            Description = "description Of Component " + componentId
                        };

                        for (var checkIndex = 1; checkIndex <= numberOfChecksPerComponent; checkIndex++)
                        {
                            counter++;

                            var checkId = counter;
                            var check = new Check
                            {
                                Id = counter,
                                // CheckId = component.Name + "_check" + checkIndex,
                                Name = component.Name + "_check" + checkIndex,
                                ElementId = component.Name + "_check" + checkIndex,
                                Description = "description Of Check " + checkId,
                                VstsLink = "https://wiki",
                                Frequency = 60,

                            };
                            component.Checks.Add(check);
                        }
                        action.Components.Add(component);
                    }
                    service.Actions.Add(action);
                }
                environment.Services.Add(service);
            }
            return environment;
        }

        public static Dictionary<string, List<StateTransition>> GetInitialStates(Environment environment, State state)
        {
            return GetInitialStates(environment, state, null);
        }

        public static Dictionary<string, List<StateTransition>> GetInitialStates(Environment environment, State state, DateTime? dateTime)
        {
            var initialStates = new Dictionary<string, List<StateTransition>>();

            foreach (var service in environment.Services)
            {
                foreach (var action in service.Actions)
                {
                    foreach (var component in action.Components)
                    {
                        foreach (var check in component.Checks)
                        {
                            initialStates.Add(check.ElementId, new List<StateTransition>() { new StateTransition
                            {
                                CheckId = check.ElementId,
                                ElementId = check.ElementId,
                                State = state,
                                SourceTimestamp = dateTime ?? DateTime.UtcNow,
                                TimeGenerated = dateTime ?? DateTime.UtcNow,
                                ComponentType = "Check"

                            }});
                        }
                        initialStates.Add(component.ElementId, new List<StateTransition>() { new StateTransition {
                            State = state,
                            ElementId = component.ElementId,
                            SourceTimestamp = dateTime ?? DateTime.UtcNow,
                            TimeGenerated = dateTime ?? DateTime.UtcNow,
                            ComponentType = "Component"
                        }});
                    }
                    initialStates.Add(action.ElementId, new List<StateTransition>() { new StateTransition
                    {
                        State = state,
                        ElementId = action.ElementId,
                        SourceTimestamp = dateTime ?? DateTime.UtcNow,
                        TimeGenerated = dateTime ?? DateTime.UtcNow,
                        ComponentType = "Action"
                    }});
                }
                initialStates.Add(service.ElementId, new List<StateTransition>() { new StateTransition
                {
                    State = state,
                    ElementId = service.ElementId,
                    SourceTimestamp = dateTime ?? DateTime.UtcNow,
                    TimeGenerated = dateTime ?? DateTime.UtcNow,
                    ComponentType = "Service"
                }});
            }

            initialStates.Add(environment.ElementId, new List<StateTransition>() { new StateTransition {
                State = state,
                ElementId = environment.ElementId,
                SourceTimestamp = dateTime ?? DateTime.UtcNow,
                TimeGenerated = dateTime ?? DateTime.UtcNow,
                ComponentType = "Environment"
            }});
            return initialStates;
        }

        public static Check GenerateGlobalCheck(string checkId)
        {
            var check = new Check
            {
                // CheckId = checkId,
                Description = "Global check" + checkId,
                ElementId = checkId
            };
            return check;
        }

        public static Check GenerateDynamicCheck(string checkId)
        {
            var check = new Check
            {
                // CheckId = checkId,
                Description = "Dynamic check" + checkId,
                ElementId = checkId
            };
            return check;
        }

        public static Check GenerateMetricAlertCheck(string elementId)
        {
            var check = new Check
            {
                // CheckId = "MetricAlert",
                Description = "MetricAlert Sum",
                ElementId = elementId,
            };
            return check;
        }

        public static void CheckStateTransitionsToStore(List<StateTransition> stateTransitions, int expectedAmount)
        {
            Assert.IsNotNull(stateTransitions);
            Assert.AreEqual(expectedAmount, stateTransitions.Count);
            var hashSetForDuplicateCheck = new HashSet<StateTransition>();

            foreach (var stateTransition in stateTransitions)
            {
                Assert.IsNotNull(stateTransition.ComponentType);
                Assert.IsNotNull(stateTransition.SourceTimestamp);
                Assert.IsNotNull(stateTransition.TimeGenerated);
                Assert.IsNotNull(stateTransition.ElementId);

                if (stateTransition.ComponentType == "Check")
                {
                    Assert.IsNotNull(stateTransition.CheckId);

                    if (stateTransition.ElementId.Equals(stateTransition.CheckId))
                    {
                        Assert.IsNull(stateTransition.AlertName);
                    }
                    if (stateTransition.AlertName != null)
                    {
                        Assert.AreNotEqual(stateTransition.ElementId, stateTransition.CheckId);
                    }
                }
                else
                {
                    // checkId and Alertname must not be filled in nodes that are not checks
                    Assert.IsNull(stateTransition.CheckId);
                    Assert.IsNull(stateTransition.AlertName);
                }

                // If you read this your test probably failed due to duplicate statetransitions. Make sure to wait
                // for a couple of milliseconds after each call of HandleAlertsInternal so that 
                // the internal time of the test framework can progress a bit.
                // TODO react
                // Assert.IsTrue(hashSetForDuplicateCheck.Add(stateTransition), "Duplicates detected in " + stateTransition);
            }
        }

        public static void AssertTimestampsEqual(DateTime dateTime1, DateTime dateTime2)
        {
            var diff = Math.Abs(dateTime1.Ticks - dateTime2.Ticks);
            Assert.IsTrue(diff < 1000);
        }
    }
}
