using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Daimler.Providence.Service.Clients.Interfaces;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateTransition;
using Daimler.Providence.Service.Utilities;
using MailKit.Net.Smtp;
using Microsoft.ApplicationInsights.DataContracts;
using MimeKit;
using Newtonsoft.Json;

namespace Daimler.Providence.Service.Clients
{
    /// <summary>
    /// Email notification client
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailNotificationClient : IEmailNotificationClient
    {
        #region Private Members

        private readonly string _environment;
        private readonly string _username;
        private readonly string _password;
        private readonly string _region;
        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EmailNotificationClient()
        {
            _environment = ProvidenceConfigurationManager.Environment;
            _region = ProvidenceConfigurationManager.Region;
            //_username = ProvidenceConfigurationManager.DaimlerRelayUsername;
            //_password = ProvidenceConfigurationManager.DaimlerRelayPassword;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task SendEmail(StateTransition transition, State? lastState, bool resolved, GetNotificationRule rule, string highestNotificationLevel)
        {
            using (new ElapsedTimeLogger())
            {
                //string IncomingWebhookUrl = "https://corptb.webhook.office.com/webhookb2/5c321d44-063f-49be-973c-43b2ef352b8f@505cca53-5750-4134-9501-8d52d5df3cd1/IncomingWebhook/defb1f2e80c2484ea901f0f97fca9f4b/8642b0aa-1374-4f2c-b4ab-d4ce6bba6010";
                AILogger.Log(SeverityLevel.Information, "Send message started.");
                using (var client = new HttpClient())
                {
                    //await client.ConnectAsync(ProvidenceConstants.EmailNotificationMailServerAddress, ProvidenceConstants.EmailNotificationMailServerPort).ConfigureAwait(false); ;
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //await client.AuthenticateAsync(_username, _password).ConfigureAwait(false);

                    //var emailAddresses = rule.EmailAddresses.Split(';');
                    //foreach (var emailAddress in emailAddresses)
                    //{
                    //var message = new MimeMessage();
                    //message.From.Add(new MailboxAddress(ProvidenceConstants.EmailNotificationSenderName, ProvidenceConstants.EmailNotificationSenderAddress));
                    //message.To.Add(MailboxAddress.Parse(emailAddress));

                    string subject;
                    if (resolved && lastState != null)
                    {
                        subject = $"{lastState} Resolved: E2E Monitoring State Notification: {_environment} - {transition.EnvironmentName} - {transition.ElementId}";
                    }
                    else
                    {
                        subject = $"{transition.State} occurred: E2E Monitoring State Notification: {_environment} - {transition.EnvironmentName} - {transition.ElementId}";
                    }
                    //message.Subject = subject;

                    //var bodyBuilder = new BodyBuilder
                    //{
                    //    HtmlBody = CreateHtmlContent(transition, rule.EnvironmentSubscriptionId, highestNotificationLevel)
                    //};
                    //message.Body = bodyBuilder.ToMessageBody();

                    string content = CreateMessageContent(transition, rule.EnvironmentSubscriptionId, highestNotificationLevel, subject);
                    StringContent strcontent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

                    var emailAddresses = rule.EmailAddresses.Split(';');
                    foreach (var emailAddress in emailAddresses)
                    {

                        // Perform Connector POST operation     
                        var httpResponseMessage = await client.PostAsync(emailAddress, strcontent);
                        // Read response content
                        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                        if (responseContent.Contains("Microsoft Teams endpoint returned HTTP error 429"))
                        {
                            // initiate retry logic
                        }
                    }

                    //AILogger.Log(SeverityLevel.Information, $"Sending Notification E-Mail to \"{rule.EmailAddresses}\".");
                    //await client.SendAsync(message).ConfigureAwait(false);
                    //AILogger.Log(SeverityLevel.Information, $"Sending Notification E-Mail to \"{rule.EmailAddresses}\" performed successfully.");
                    //}
                    //await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Private Methods

        private string CreateMessageContent(StateTransition transition, string environmentId, string highestNotificationLevel, string subject)
        {

            string baseUrl = "";
            if (_region == "cn")
            {
                baseUrl = "https://e2emonitoring." + _environment + ".csg.connectivity.fotondaimler.com";
            }
            else
            {
                if (_environment == "prod")
                {
                    baseUrl = "https://e2emonitoring." + _region + ".csg.daimler-truck.com";
                }
                else
                {
                    baseUrl = "https://e2emonitoring." + _region + "." + _environment + ".csg.daimler-truck.com";
                }
            }
            var messageContent = $@"{{
                                        ""type"":""message"",
                                        ""attachments"":[
                                      {{
                                        ""contentType"":""application/vnd.microsoft.card.adaptive"",
										""content"" : {{
										""type"":""AdaptiveCard"",
                                        ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                                        ""version"": ""1.5"",
                                          ""body"": [
                                            {{
                                              ""type"": ""TextBlock"",
                                              ""text"": ""E2E Monitoring Notification"",
                                              ""wrap"": true,
                                              ""style"": ""heading""
                                            }},
                                            {{
                                              ""type"": ""TextBlock"",
                                              ""text"": ""{subject}"",
                                              ""wrap"": true,
                                              ""style"": ""heading""
                                            }},
                                            {{
                                              ""type"": ""FactSet"",
                                              ""facts"": [
                                            {{
                                               ""title"": ""Environment Name"",
                                               ""value"": ""{transition.EnvironmentName}""
                                            }},
                                            {{
                                               ""title"": ""Environment ID"",
                                                ""value"": ""{environmentId}""
                                            }},
		                                    {{
                                               ""title"": ""Notification Level"",
                                               ""value"": ""{highestNotificationLevel}""
                                            }},
		                                    {{
                                               ""title"": ""New State"",
                                               ""value"": ""{transition.State}""
                                            }}
                                              ]
                                            }},
                                            {{
                                               ""type"": ""TextBlock"",
                                               ""text"": ""Alert"",
                                               ""wrap"": true,
                                               ""style"": ""heading""
                                            }},
                                            {{
                                              ""type"": ""FactSet"",
                                              ""facts"": [
                                            {{
                                               ""title"": ""State"",
                                               ""value"": ""{transition.State}""
                                            }},
                                            {{
                                               ""title"": ""Timestamp"",
                                                ""value"": ""{transition.TimeGenerated.ToUniversalTime()}""
                                            }},
		                                    {{
                                               ""title"": ""Check ID"",
                                               ""value"": ""{transition.TriggeredByCheckId}""
                                            }},
		                                    {{
                                               ""title"": ""Affected component"",
                                               ""value"": ""{transition.TriggeredByElementId}""
                                            }},
                                            {{
                                               ""title"": ""Alert Description"",
                                               ""value"": ""{transition.Description}""
                                            }}
                                              ]
                                            }},
                                          ],
                                          ""actions"": [
                                            {{
                                              ""type"": ""Action.OpenUrl"",
                                              ""title"": ""Take me to Dashboard"",
                                              ""url"": ""{baseUrl}/{transition.EnvironmentName}""
                                            }}
                                          ]
										  }}
                                            }}
                                            ]
                                        }}";

            return messageContent;
        }
        private string CreateHtmlContent(StateTransition transition, string environmentId, string highestNotificationLevel)
        {
            var htmlContent = "<p><strong> Daimler adVANce Providence Notification </strong></p>" +
                               "<p style= \"width: 50%; border-bottom: solid #A5A5A5 1.0pt\" ><strong> Overview </strong></p>" +
                                "<table style = \"border-collapse: collapse; width:150%\">" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Providence Instance </td>" +
                                $"<td > {_environment} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Environment Name </td>" +
                                $"<td > {transition.EnvironmentName} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Environment ID </td>" +
                                $"<td> {environmentId} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Notification Level </td>" +
                                $"<td> {highestNotificationLevel} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Element ID </td>" +
                                $"<td> {transition.ElementId} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> New State </td>" +
                                $"<td> {transition.State} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> URL </td>" +
                                $"<td><a href = \"https://spp-monitoringservice-{_environment}.azurewebsites.net/{transition.EnvironmentName}\"> https://spp-monitoringservice-{_environment}.azurewebsites.net/{transition.EnvironmentName} </td>" +
                                "</tr>" +
                                "</table>" +
                                "<p style= \"width: 50%; border-bottom: solid #A5A5A5 1.0pt\" ><strong> Alert </strong></p>" +
                                "<table style = \"border-collapse: collapse; width:150%\">" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> State </td>" +
                                $"<td> {transition.State} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Timestamp </td>" +
                                $"<td> {transition.TimeGenerated.ToUniversalTime()} UTC </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Check ID </td>" +
                                $"<td> {transition.TriggeredByCheckId} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Affected component </td>" +
                                $"<td> {transition.TriggeredByElementId} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> Record ID </td>" +
                                $"<td> {transition.RecordId} </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\">Alert Description </td>" +
                                $"<td> {transition.Description} </td>" +
                                "</tr>";

            if (!string.IsNullOrEmpty(transition.CustomField1))
            {
                htmlContent += "</tr>" +
                               "<tr>" +
                               "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> CustomField1 </td>" +
                               $"<td> {transition.CustomField1} </td>" +
                               "</tr>";
            }
            if (!string.IsNullOrEmpty(transition.CustomField2))
            {
                htmlContent += "</tr>" +
                               "<tr>" +
                               "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> CustomField2 </td>" +
                               $"<td> {transition.CustomField2} </td>" +
                               "</tr>";
            }
            if (!string.IsNullOrEmpty(transition.CustomField3))
            {
                htmlContent += "</tr>" +
                               "<tr>" +
                               "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> CustomField3 </td>" +
                               $"<td> {transition.CustomField3} </td>" +
                               "</tr>";
            }
            if (!string.IsNullOrEmpty(transition.CustomField4))
            {
                htmlContent += "</tr>" +
                               "<tr>" +
                               "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> CustomField4 </td>" +
                               $"<td> {transition.CustomField4} </td>" +
                               "</tr>";
            }
            if (!string.IsNullOrEmpty(transition.CustomField5))
            {
                htmlContent += "</tr>" +
                               "<tr>" +
                               "<td style = \"margin-left: 8.5pt; font-size: 10.0pt; color: #595959;\"> CustomField5 </td>" +
                               $"<td> {transition.CustomField5} </td>" +
                               "</tr>";
            }

            htmlContent += "</table>";

            return htmlContent;
        }

        #endregion
    }
}