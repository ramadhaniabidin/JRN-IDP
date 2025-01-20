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
        private readonly string workflowURL = "/workflows/v1/designs/586aa111-54a5-4fad-98ff-74a50627997f/instances";

        public string GetToken()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            var requestBody = new
            {
                client_id = client_id,
                client_secret = client_secret,
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

        public async Task CallNotificationWorkflow_Production()
        {
            NintexWorkflowCloud nwcModel = new NintexWorkflowCloud();
            nwcModel.url = NACBaseURL;
            string token = GetToken();
            string workflowEndpoint = ConfigurationManager.AppSettings["NotificationWorkflow_Url_Production"];
            string requestBody = System.Text.Json.JsonSerializer.Serialize(nwcModel.param);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                AuthenticationHeaderValue("Bearer", token);
            client.BaseAddress = new Uri(nwcModel.url);
            client.DefaultRequestHeaders.Accept.
                Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, workflowEndpoint);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
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
