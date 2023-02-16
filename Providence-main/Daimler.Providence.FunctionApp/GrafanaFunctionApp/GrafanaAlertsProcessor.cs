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
using System.Linq;
using System.Collections.Generic;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Azure.Identity;
using ProvidenceFuncAppPayload;
using ProvidenceFuncApp;

namespace GrafanaFuncApp
{
    public static class GrafanaAlertsProcessor
    {
        [FunctionName("GrafanaAlertsProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                AILogger.Log(SeverityLevel.Information, "Grafana Function App started processing with request.");
                PayloadOut payLoadDataOut = new PayloadOut();

                string payLoadString = String.Empty;
                string eventHubName = Environment.GetEnvironmentVariable("EVENTHUB_NAME");
                string userAssignedClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                string eventHubHostName = Environment.GetEnvironmentVariable("EVENTHUB_HOSTNAME");
                if (eventHubName != null)
                {
                    AILogger.Log(SeverityLevel.Information, $"Grafana Function App received connection string for the Eventhub '{eventHubName}'.");
                }
                else
                {
                    string msg = "Grafana Function App failed to receive Eventhub name and it's connection string..!";
                    AILogger.Log(SeverityLevel.Error, msg, string.Empty, StateConstants.ErrorCodes.Error.ToString());
                    return new NotFoundObjectResult(msg);
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                log.LogInformation("Req received from Grafana : " + req);
                AILogger.Log(SeverityLevel.Information, $"GrafanaFunctionApp: Request - {req}");
                AILogger.Log(SeverityLevel.Information, $"GrafanaFunctionApp: payload - {data}");

                if (!String.IsNullOrEmpty((string)data?.alerts[0].labels?.checkId) && !String.IsNullOrEmpty((string)data?.alerts[0].labels?.componentId)
                    && !String.IsNullOrEmpty((string)data?.alerts[0].labels?.envId) && !String.IsNullOrEmpty((string)data?.alerts[0].labels?.alertname)) 
                {
                    payLoadDataOut.AlertName = data?.alerts[0].labels?.alertname;
                    payLoadDataOut.CustomField1 = data?.alerts[0].panelURL;
                    payLoadDataOut.CustomField3 = (data?.commonAnnotations?.message != null)? data?.commonAnnotations?.message:"";
                    payLoadDataOut.State = data?.commonLabels?.state;
                    payLoadDataOut.Description = retMetricAndValue(Convert.ToString(data?.alerts[0].valueString));
                    payLoadDataOut.TimeGenerated = DateTime.Now;
                    payLoadDataOut.SourceTimestamp = DateTime.Now;
                    payLoadDataOut.CheckId = data?.alerts[0].labels?.checkId;
                    payLoadDataOut.ComponentId = data?.alerts[0].labels?.componentId;
                    payLoadDataOut.SubscriptionId = data?.alerts[0].labels?.envId;
                }
                else
                {
                    string msg = "GrafanaFunctionApp stopped processing as alertname, checkId, envId or componentId is missing from tags of alert payload.";
                    AILogger.Log(SeverityLevel.Error, msg, string.Empty, StateConstants.ErrorCodes.Error.ToString());
                    return new NotFoundObjectResult(msg);
                }

                log.LogInformation("Rule Name : " + payLoadDataOut.AlertName.ToString());
                log.LogInformation("Message  : " + payLoadDataOut.CustomField3.ToString());
                log.LogInformation("Description  : " + payLoadDataOut.Description.ToString());

                payLoadString = JsonConvert.SerializeObject(payLoadDataOut);
                AILogger.Log(SeverityLevel.Information, $"GrafanaFunctionApp: Payload Out - {payLoadString}");

                List<string> environmentList = null;
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
                foreach (var env in environmentList)
                {
                    payLoadDataOut.RecordId = Guid.NewGuid().ToString();
                    log.LogInformation("RecordId - " + payLoadDataOut.RecordId);
                    payLoadDataOut.SubscriptionId = env;
                    payLoadString = JsonConvert.SerializeObject(payLoadDataOut);
                    bool value = eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(payLoadString)));
                }
                await producerClient.SendAsync(eventBatch);
                string responseMessage = String.Format("Grafana Function App triggered successfully.");
                AILogger.Log(SeverityLevel.Information, responseMessage);
                return new OkObjectResult(responseMessage);

            }
            catch (Exception ex)
            {
                AILogger.Log(SeverityLevel.Error, $"Grafana Exception Message - '{ex}'.");
                throw;
            }
        }

        public static string retMetricAndValue(string inputString)
        {
            //inputString = "[ var='A0' metric='Requests' labels={} value=1 ]";
            if (inputString != null)
            {
                string metric = inputString.Split("metric='").LastOrDefault().Split("'").FirstOrDefault();
                string metricsValue = inputString.Split("value=").LastOrDefault().Replace("]", "");
                return metric + " = " + metricsValue;
            }
            return null;
        }
    }
}
