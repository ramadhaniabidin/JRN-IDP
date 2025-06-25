using JRN_IDP.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class ProsnapHandler
    {
        private readonly string baseURL = ConfigurationManager.AppSettings["PROSNAP_BASEURL"];
        private readonly string getTokenURL = "/token";
        private readonly string uploadURL = "/api/transaction/upload";
        private readonly string scanURL = "/api/transaction/Scan_V2?";
        private readonly string connString = ConfigurationManager.AppSettings["connString"];
        readonly NACHandler NAC = new NACHandler();

        public void UpdateStatus_SPOFile(int Item_ID, int FileID)
        {
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                string query = $"UPDATE [dbo].[P2PDocuments] SET ProSnap_Status = 1, ProSnap_FileID = @FileID WHERE Item_ID = @Item_ID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Item_ID", Item_ID);
                    cmd.Parameters.AddWithValue("@FileID", FileID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public string GetToken()
        {
            var formData = new Dictionary<string, string>
            {
                { "username", "zulfikar@elistec.com" },
                { "password", "Pa55word" },
                { "grant_type", "password" }
            };
            var content = new FormUrlEncodedContent(formData);
            using (var client = new HttpClient())
            {
                string endpoint = $"{baseURL}{getTokenURL}";
                HttpResponseMessage response = client.PostAsync(endpoint, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    JObject jsonObject = JObject.Parse(responseContent);
                    string token = jsonObject["access_token"].ToString();
                    return token;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Error details:");
                    Console.WriteLine(errorContent);
                    return "";
                }
            }
        }

        public void UploadFile(SPOFileModel file, string base64)
        {
            string token = GetToken();
            string url = $"{baseURL}{uploadURL}";
            var payload = new
            {
                FileName = file.Document_Name,
                DocumentID = "142",
                DocumentModelID = "JRN-Invoices-V0.4",
                Active = true,
                Base64 = base64
            };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            try
            {
                // Make the POST request
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Success!");
                    Console.WriteLine(responseContent);
                    JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
                    JsonElement root = jsonDocument.RootElement;
                    int ID = root.GetProperty("Item").GetProperty("ID").GetInt32();
                    Console.WriteLine($"ID: {ID}");
                    UpdateStatus_SPOFile(file.Item_ID, ID);
                    LoopScanDocument(ID);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Error details:");
                    Console.WriteLine(errorContent);
                }
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public void NotifVerificator(int HeaderID)
        {
            #region Get User
            DataTable dt = new DataTable();
            using(SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = $"SELECT TOP 1 Created_By, Document_Name FROM P2PDocuments WHERE ProSnap_FileID = @HeaderID";
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            var spoFile = Utility.ConvertDataTableToList<SPOFileModel>(dt)[0];
            string UserEmail = spoFile.Created_By;
            string fileName = spoFile.Document_Name;
            #endregion
            NintexWorkflowCloud nwc = new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_headerid = HeaderID,
                        se_type = "1",
                        se_useremail = UserEmail,
                        se_filename = fileName
                    }
                }

            };
            Task.Run(async () =>
            {
                await NAC.TriggerWorkflowAsync(nwc);
            }).Wait();
        }

        public void LoopScanDocument(int DocumentID)
        {
            int try_scan = 0;
            string status = "";            
            while(status != "1")
            {
                Console.WriteLine($"Scan ke: {try_scan + 1}");
                try
                {
                    status = ScanDocument(DocumentID);
                    Console.WriteLine($"Scan process: {status}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Scan process: {ex.Message}");
                    status = "";
                }
                try_scan += 1;
            }
            if(status == "1")
            {
                NotifVerificator(DocumentID);
            }
        }

        public string ScanDocument(int DocumentID)
        {
            string endpoint = $"{baseURL}{scanURL}ID={DocumentID}&TenantID=2";
            using (var client = new HttpClient())
            {
                var token = GetToken();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync(endpoint).Result;
                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        JsonDocument document = JsonDocument.Parse(responseBody);

                        // Get the value of "Status"
                        string status = document.RootElement.GetProperty("Status").GetString();

                        Console.WriteLine($"Status: {status}");
                        return status;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("Error details:");
                        Console.WriteLine(errorContent);
                        return "";
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "";
                }
            }
        }

        public static void TestParsingJSON()
        {
            string json = "{ \"ProcessSuccess\":true,\"InfoMessage\":\"OK\",\"Item\":{ \"ID\":173,\"Url\":\"http://8.215.35.106:3030/Archives/597cf135-bcfe-4af4-93dd-8bdded8f095d.pdf\"}";
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            int id = root.GetProperty("Item").GetProperty("ID").GetInt32();

            Console.WriteLine($"ID: {id}");
        }
    
    }
}
