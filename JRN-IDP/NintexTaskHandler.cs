using JRN_IDP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace JRN_IDP
{
    public static class NintexTaskHandler
    {
        public static string GetToken_DaikinNAC()
        {
            try
            {
                string url = "https://au.nintex.io/authentication/v1/token";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpClient client = new HttpClient();
                var requestBody = new
                {
                    client_id = "dcc05cd6-10d7-4af1-8b68-0f0ac7dd77f5",
                    client_secret = "sLQKQtRSsNtRUsLROI2HtTsQLtTsO2GsPOJK2HsRRtWsQtPsMLItTRsNRtVsFRtTsNtUsFMOtUsOFtRsQRJFtTUsPtUsItRsOtSVsO2N",
                    grant_type = "client_credentials"
                };
                var jsonBody = new JavaScriptSerializer().Serialize(requestBody);
                var HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, HttpContent).Result;
                var responseJson = response.Content.ReadAsStringAsync().Result;
                var responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
                string accessToken = responseObject["access_token"];
                return accessToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<TaskResponseModel> GetTask_ByInstanceID_Async(string Instance_ID)
        {
            try
            {
                string url = $"https://au.nintex.io/workflows/v2/tasks?from=2025-02-01&workflowInstanceId={Instance_ID}";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GetToken_DaikinNAC()}");
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var TaskResponse = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit=100}.
                        Deserialize<TaskResponseModel>(responseJson);
                    return TaskResponse;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
