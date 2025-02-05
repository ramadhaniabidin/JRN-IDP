using JRN_IDP.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class NACHandler
    {
        private readonly string client_id = ConfigurationManager.AppSettings["client_id"];
        private readonly string client_secret = ConfigurationManager.AppSettings["client_secret"];
        private readonly string get_token_url = "https://au.nintex.io/authentication/v1/token";
        private readonly string NACBaseURL = "https://au.nintex.io";
        private readonly string workflowURL = ConfigurationManager.AppSettings["NotificationWorkflow_Url_Production"];

        public string GetToken()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            var requestBody = new
            {
                client_id,
                client_secret,
                grant_type = "client_credentials"
            };
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = client.PostAsync(get_token_url, httpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            using (JsonDocument document = JsonDocument.Parse(responseJson))
            {
                JsonElement root = document.RootElement;
                string access_token = root.GetProperty("access_token").GetString();
                //Console.WriteLine($"Token: {access_token}");
                return access_token == null ? "" : access_token;
            }
        }

        public void CallWorkflow()
        {
            NintexWorkflowCloud nwc = new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_headerid = 0,
                        se_payload = "Test"
                    }
                }
            };
            Task.Run(async () =>
            {
                await TriggerWorkflowAsync(nwc);
            }).Wait();
        }

        public void CallNotificationWorkflow_Production(string type)
        {
            NintexWorkflowCloud CreateInvoiceFailedModel = new NintexWorkflowCloud
            {
                url = NACBaseURL,
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_type = "0",
                        se_filename = "Invoice 1 - inv.pdf",
                        se_useremail = "elistec@jresources.com",
                        se_message = "	You must provide a valid value for the Purchase Order Line Number attribute. (AP-810667)",
                        se_payload = "{\"BusinessUnit\":\"SRSB BU\",\"InvoiceNumber\":\"95811305 - Test - Vc1o\",\"InvoiceAmount\":\"111625\",\"InvoiceDate\":\"2024 - 11 - 06\"}"
                    }
                }
            };

            NintexWorkflowCloud ScanCompleteModel = new NintexWorkflowCloud
            {
                url = NACBaseURL,
                param = new NWCParamModel
                {
                    startData =  new StartData
                    {
                        se_type = "1",
                        se_filename = "INV1-inv.pdf",
                        se_headerid = 137,
                        se_useremail = "elistec@jresources.com"
                    }
                }
            };

            NintexWorkflowCloud CreateInvoiceSuccessModel = new NintexWorkflowCloud
            {
                url = NACBaseURL,
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_invoicenumber = "0037242 - Test - ahvU",
                        se_invoiceid = "300000085332709",
                        se_filename = "0037242-inv.pdf",
                        se_useremail = "elistec@jresources.com",
                        se_type = "3"
                    }
                }
            };

            NintexWorkflowCloud nwcModel = new NintexWorkflowCloud();
            if(type == "1")
            {
                nwcModel = ScanCompleteModel;
            }
            else if(type == "0")
            {
                nwcModel = CreateInvoiceFailedModel;
            }
            else if(type == "3")
            {
                nwcModel = CreateInvoiceSuccessModel;
            }
            try
            {
                Console.WriteLine("Begin Trigger Notification Workflow - Production");
                Task.Run(async () =>
                {
                    await TriggerWorkflowAsync(nwcModel);
                }).Wait();
                Console.WriteLine("Trigger Notification Workflow Production Success");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Trigger notification workflow production error : {ex.Message}");
            }
        }

        public async Task TriggerWorkflowAsync(NintexWorkflowCloud nwcModel)
        {
            nwcModel.url = NACBaseURL;
            string token = GetToken();
            string requestBody = System.Text.Json.JsonSerializer.Serialize(nwcModel.param);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                AuthenticationHeaderValue("Bearer", token);
            client.BaseAddress = new Uri(nwcModel.url);
            client.DefaultRequestHeaders.Accept.
                Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, workflowURL);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
        }
    }
}
