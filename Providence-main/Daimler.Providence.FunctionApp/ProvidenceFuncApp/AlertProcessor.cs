using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using System.Text;
using ProvidenceFuncAppPayload;
using System.Linq;
using System.Collections.Generic;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Azure.Identity;

namespace ProvidenceFuncApp
{
    public static class AlertProcessor
    {
        [FunctionName("AlertProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //var msiEnvironment = new MSIEnvironment(); 
            //Configuration.Bind("MSIEnvironment", msiEnvironment);
            //Environment.SetEnvironmentVariable("AZURE_TENANT_ID", msiEnvironment.TenantId);
            //Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", msiEnvironment.ClientId);
            log.LogInformation("C# HTTP trigger function processed a request.");
            AILogger.Log(SeverityLevel.Information, "Function App started processing with request.");
            string alertSchema = req.Query["name"];
            string metricValue = req.Query["name"];
            PayloadOut payLoadDataOut = new PayloadOut();

            string payLoadString = String.Empty;
            //string connectionString = Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION");
            string eventHubName = Environment.GetEnvironmentVariable("EVENTHUB_NAME");
            string userAssignedClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            string eventHubHostName = Environment.GetEnvironmentVariable("EVENTHUB_HOSTNAME");
            //string userAssignedClientId = "2706c202-b9f3-4901-8294-d473e835e758";

            //string connectionString = "Endpoint=sb://csg-stg-eh-mon-weu.servicebus.windows.net/;SharedAccessKeyName=ProvidenceAPIAccess;SharedAccessKey=oAcTZ+rvqjTv+1SmjypdfUciQ0ConZwLnAF302SM9L4=;EntityPath=csg-stg-eh-mon-hub1-weu";
            //string eventHubName = "csg-stg-eh-mon-hub1-weu";

            //string connectionString = "Endpoint = sb://csgprovidencedev.servicebus.windows.net/;SharedAccessKeyName=ProvidenceAPIAccess;SharedAccessKey=gQLmMBTtbnUzjwPF6lJGiw+vqZpmrmDG8/paRu0dO0I=;EntityPath=providencedev";
            //string eventHubName = "providencedev";

            if (eventHubName != null)
            {
                AILogger.Log(SeverityLevel.Information, $"Function App received connection string for the Eventhub '{eventHubName}'.");
            }
            else
            {
                string msg = "Function App failed to receive Eventhub name and it's connection string..!";
                AILogger.Log(SeverityLevel.Error, msg, string.Empty, Constants.ErrorCodes.Error.ToString());
                return new NotFoundObjectResult(msg);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            alertSchema = alertSchema ?? data?.schemaId;

            List<string> environmentList = null;
            AILogger.Log(SeverityLevel.Information, "Function App received " + alertSchema + " payload.");
            if (alertSchema == "AzureMonitorMetricAlert")
            {
                if (!String.IsNullOrEmpty((string)data?.data?.properties?.CheckId) && !String.IsNullOrEmpty((string)data?.data?.properties?.ComponentId)
                    && !String.IsNullOrEmpty((string)data?.data?.properties?.Environment))
                {
                    metricValue = metricValue ?? data?.data?.context?.condition?.allOf[0].metricValue;

                    payLoadDataOut.AlertName = data?.data?.context?.name;
                    payLoadDataOut.CheckId = data?.data?.properties?.CheckId;
                    payLoadDataOut.ComponentId = data?.data?.properties?.ComponentId;
                    payLoadDataOut.State = (data?.data?.context?.severity == "1") ? Constants.ErrorCodes.Error.ToString() : Constants.ErrorCodes.Ok.ToString();
                    payLoadDataOut.SubscriptionId = data?.data?.properties?.Environment;
                    AILogger.Log(SeverityLevel.Information, $"Function App composing the request for eventhub using {payLoadDataOut.AlertName}'s payload.");
                }
                else
                {
                    string msg = "Function App stopped processing as CheckId, EnvironmentId or ComponentId is missing from custom properties of alert payload.";
                    AILogger.Log(SeverityLevel.Error, msg, string.Empty, Constants.ErrorCodes.Error.ToString());
                    return new NotFoundObjectResult(msg);
                }

            }
            else if (alertSchema == "azureMonitorCommonAlertSchema")
            {
                if (!String.IsNullOrEmpty((string)data?.data?.alertContext?.properties.CheckId) && !String.IsNullOrEmpty((string)data?.data?.alertContext?.properties.ComponentId)
                    && !String.IsNullOrEmpty((string)data?.data?.alertContext?.properties.Environment))
                {
                    metricValue = metricValue ?? data?.data?.alertContext?.condition?.allOf[0].metricValue;

                    payLoadDataOut.AlertName = data?.data?.essentials?.alertRule;
                    payLoadDataOut.CheckId = data?.data?.alertContext?.properties.CheckId;
                    payLoadDataOut.ComponentId = data?.data?.alertContext?.properties.ComponentId;
                    string queryValue = data?.data?.alertContext?.condition?.allOf[0].searchQuery;
                    //from queryValue, below line will chop the string when it finds first space to search only with Tablenames
                    string tableName = queryValue.Contains(" ") ? queryValue.Substring(0, queryValue.IndexOf(" ")) : queryValue;
                    if (tableName.ToLower().Contains("kubepodinventory"))
                    {
                        AILogger.Log(SeverityLevel.Information, $"Function app recieved POD search query : '{queryValue}'");
                        payLoadDataOut.State = (metricValue == "1") ? Constants.ErrorCodes.Warning.ToString() : ((metricValue == "0") ? Constants.ErrorCodes.Error.ToString() : Constants.ErrorCodes.Ok.ToString());
                    }
                    else if (tableName.ToLower().Contains("exceptions"))
                    {
                        AILogger.Log(SeverityLevel.Information, $"Function app recieved exception search query : '{queryValue}'");
                        payLoadDataOut.State = (metricValue == "0") ? Constants.ErrorCodes.Ok.ToString() : Constants.ErrorCodes.Error.ToString();
                    }
                    else
                    {
                        AILogger.Log(SeverityLevel.Information, $"Function app recieved schceduled query : '{queryValue}'");
                        payLoadDataOut.State = (metricValue == "0") ? Constants.ErrorCodes.Error.ToString() : Constants.ErrorCodes.Ok.ToString();
                    }
                    payLoadDataOut.SubscriptionId = data?.data?.alertContext?.properties.Environment;
                    AILogger.Log(SeverityLevel.Information, $"Function App composing the request for eventhub using {payLoadDataOut.AlertName}'s payload.");
                }
                else
                {
                    string msg = "Function App stopped processing as CheckId, EnvironmentId or ComponentId is missing from custom properties of alert payload.";
                    AILogger.Log(SeverityLevel.Error, msg, string.Empty, Constants.ErrorCodes.Error.ToString());
                    return new NotFoundObjectResult(msg);
                }
            }

            string environment = payLoadDataOut.SubscriptionId;
            if (environment != null)
            {
                environmentList = environment.Split(",").ToList();
            }
            
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = userAssignedClientId
            });
            await using var producerClient = new EventHubProducerClient(eventHubHostName, eventHubName, credential);
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
            log.LogInformation("Using client Id: " + userAssignedClientId);
            int Count = 1;
            foreach (var env in environmentList)
            {
                AILogger.Log(SeverityLevel.Information, $"{Count}.) Function App processing request for '{env}' environment.");
                Count++;
                payLoadDataOut.SubscriptionId = env;
                payLoadDataOut.SourceTimestamp = DateTime.UtcNow;
                payLoadDataOut.TimeGenerated = DateTime.UtcNow;
                payLoadDataOut.Description = "Alert is triggered for " + payLoadDataOut.State + " condition.";
                payLoadDataOut.RecordId = Guid.NewGuid().ToString();
                payLoadString = JsonConvert.SerializeObject(payLoadDataOut);
                bool value = eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(payLoadString)));
                string batchMsg = String.Format("Function App processed request & added to Eventhub batch for {0} Environment : {1}", env, value.ToString().ToUpper());
                AILogger.Log(value ? SeverityLevel.Information : SeverityLevel.Warning, batchMsg);
                AILogger.Log(SeverityLevel.Information, $"Function App sending data to EventHub - Check ID:'{payLoadDataOut.CheckId}', Component ID:'{payLoadDataOut.ComponentId}', Environment ID:'{payLoadDataOut.SubscriptionId}', State of Alert:'{payLoadDataOut.State}'");
                log.LogInformation("Check ID: " + payLoadDataOut.CheckId);
                log.LogInformation("Component ID: " + payLoadDataOut.ComponentId);
                log.LogInformation("Environment ID: " + payLoadDataOut.SubscriptionId);
                log.LogInformation("State of Alert: " + payLoadDataOut.State);
                log.LogInformation("Eventhub's event status: " + value.ToString());
            }

            await producerClient.SendAsync(eventBatch);
            string responseMessage = String.Format("Function App triggered successfully.");
            AILogger.Log(SeverityLevel.Information, responseMessage);
            return new OkObjectResult(responseMessage);

        }
    }
}
