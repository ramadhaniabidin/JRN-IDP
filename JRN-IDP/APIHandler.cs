using JRN_IDP.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class APIHandler
    {
        NACHandler NAC = new NACHandler();
        SPOHandler spo = new SPOHandler();
        private readonly string connString = ConfigurationManager.AppSettings["connString"];
        string url = "https://fa-exke-test-saasfaprod1.fa.ocs.oraclecloud.com/fscmRestApi/resources/11.13.18.05/invoices";
        string getSupplierUrl = "https://fa-exke-test-saasfaprod1.fa.ocs.oraclecloud.com/fscmRestApi/resources/11.13.18.05/suppliers";
        //string getSupplierSiteURL = "https://fa-exke-test-saasfaprod1.fa.ocs.oraclecloud.com/fscmRestApi/resources/11.13.18.05/suppliers/SupplierID/child/sites";
        string username = "elistec@jresources.com";
        string password = "Bky\\_J0x5A?9";

        public static string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Range(0, 4)
                .Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        public List<IDModel> GetVerifiedTransactionHeaderIDList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "usp_TransactionHeaders_GetVerifiedList";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return Utility.ConvertDataTableToList<IDModel>(dt);
        }

        public void SyncTransactionToDataTable(int HeaderID)
        {
            Console.WriteLine("Begin Sync Transaction to Data Table");
            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                string query = "[usp_Transaction_SyncToDataTable]";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Success\n");
        }

        public long GetSupplierID(string SupplierName)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrWhiteSpace(SupplierName))
            {
                return -1;
            }
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                string param = $"q=Supplier={SupplierName}";
                string endpoint = $"{getSupplierUrl}?{param}";
                HttpResponseMessage response = client.GetAsync(endpoint).Result;

                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        // Parse the JSON response
                        JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                        JsonElement root = jsonDocument.RootElement;

                        // Access the SupplierId from the first item
                        long supplierId = root.GetProperty("items")[0].GetProperty("SupplierId").GetInt64();

                        //Console.WriteLine($"SupplierId: {supplierId}");
                        return supplierId;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("Error details:");
                        Console.WriteLine(errorContent);
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            }          
        }

        public SupplierModel GetSupplierInfo(long supplierID)
        {
            if (supplierID == -1)
            {
                return new SupplierModel
                {
                    SupplierSite = "",
                    SupplierBusinessUnit = ""
                };
            }

            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                string param = $"limit=1";
                string endpoint = $"https://fa-exke-test-saasfaprod1.fa.ocs.oraclecloud.com/fscmRestApi/resources/11.13.18.05/suppliers/{supplierID}/child/sites?{param}";
                HttpResponseMessage response = client.GetAsync(endpoint).Result;
                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                        // Parse the JSON response
                        JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                        JsonElement root = jsonDocument.RootElement;

                        // Access the SupplierId from the first item
                        string SupplierSite = root.GetProperty("items")[0].GetProperty("SupplierSite").GetString();
                        string SupplierBU = root.GetProperty("items")[0].GetProperty("ProcurementBU").GetString();
                        return new SupplierModel
                        {
                            SupplierSite = string.IsNullOrEmpty(SupplierSite) ? "" : SupplierSite,
                            SupplierBusinessUnit = string.IsNullOrEmpty(SupplierBU) ? "" : SupplierBU
                        };
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("Error details:");
                        Console.WriteLine(errorContent);
                        return new SupplierModel
                        {
                            SupplierSite = "",
                            SupplierBusinessUnit = ""
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return new SupplierModel
                    {
                        SupplierSite = "",
                        SupplierBusinessUnit = ""
                    };
                }
            }
        }


        public InvoiceHeaderModel GetInvoiceHeader(int HeaderID)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "[usp_TransactionInvoiceHeader_GetByHeaderID]";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, "Success", "OK", "InvoiceHeader");
                InvoiceHeaderModel invoiceHeader = Utility.ConvertDataTableToList<InvoiceHeaderModel>(dt)[0];
                invoiceHeader.InvoiceNumber += $"-Test-{GenerateRandomString()}";
                return invoiceHeader;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Header Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, "Failed", ex.Message, "InvoiceHeader");
                return new InvoiceHeaderModel();
            }
        }

        public void OnSuccessCreateInvoice(string InvoiceID, int HeaderID, string Payload, string Status)
        {
            Console.WriteLine("Begin Insert Successfully Created Invoice");
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "usp_Insert_Created_Invoice";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    cmd.Parameters.AddWithValue("@InvoiceID", InvoiceID);
                    cmd.Parameters.AddWithValue("@Payload", Payload);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Insert data success!");
        }

        public void PostCreateInvoice_InsertLog(string InvoiceID, int HeaderID, string Payload, string Status, string Message, string ResponseBody)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "[usp_PostCreateInvoice_InsertLog]";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    cmd.Parameters.AddWithValue("@InvoiceID", InvoiceID);
                    cmd.Parameters.AddWithValue("@Payload", Payload);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@Message", Message);
                    cmd.Parameters.AddWithValue("@ResponseBody", ResponseBody);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void GeneratePayload_InsertLog(int HeaderID, string Status, string Message, string Type)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "[usp_GeneratePayload_InsertLog]";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    cmd.Parameters.AddWithValue("@Type", Type);
                    cmd.Parameters.AddWithValue("@Message", Message);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTransactionHeaders(int id)
        {
            using(var con = new SqlConnection(connString))
            {
                con.Open();
                string query = $"UPDATE TransactionHeaders SET Is_Trying_ToPost = 1 WHERE ID = {id}";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<InvoiceHeaderModel> GetInvoiceHeaderList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("usp_GetInvoiceHeaderList", conn))
                {
                    cmd.Parameters.Clear();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return Utility.ConvertDataTableToList<InvoiceHeaderModel>(dt);
        }

        public List<InvoiceLineModel> PopulateInvoiceLine(int HeaderID)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "[usp_TransactionInvoiceLine_GetByHeaderID]";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, "Success", "OK", "InvoiceLine");
                return Utility.ConvertDataTableToList<InvoiceLineModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Line Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, "Failed", ex.Message, "InvoiceLine");
                return new List<InvoiceLineModel>();
            }
        }

        public AttachmentModel GetDefaultAttachment(int HeaderID)
        {
            DataTable dt = new DataTable();
            using(var con = new SqlConnection(connString))
            {
                con.Open();
                string query = $"SELECT List_Name, Document_Name, Document_Url FROM P2PDocuments WHERE ProSnap_FileID = {HeaderID}";
                using(var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using(var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            var Document = Utility.ConvertDataTableToList<SPOFileModel>(dt)[0];
            //string siteName = "p2pdocumentation";
            string siteUrl = $"https://jresourcesid.sharepoint.com";
            string title = Document.Document_Name.ToUpper().Replace("- INV.PDF", "").Trim();
            //string fileURL = $"/sites/{siteName}/{Document.List_Name}/{title}/{Document.Document_Name.ToUpper().Replace(" - INV", "").Trim()}";
            string fileRelativeURL = Document.Document_Url.Replace(siteUrl, "").Trim();
            string base64Content = spo.GenerateBase64Attachment(fileRelativeURL);
            return new AttachmentModel
            {
                Type = "File",
                FileName = Document.Document_Name,
                Title = title,
                Description = "Purchase Order",
                FileContents = base64Content
            };
        }

        public List<InvoiceDistributionModel> PopulateInvoiceDistributions(int HeaderID, int LineNumber)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("[usp_TransactionInvoiceDistribution_GetByHeaderID]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                        cmd.Parameters.AddWithValue("@RowNumber", LineNumber);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, "Success", "OK", "InvoiceDistribution");
                return Utility.ConvertDataTableToList<InvoiceDistributionModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Distribution Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, "Failed", ex.Message, "InvoiceDistribution");
                return new List<InvoiceDistributionModel>();
            }
        }

        public void NotifTeamIT(int HeaderID, string payload, string message)
        {
            Console.WriteLine("Notif IT Team - Begin");
            try
            {
                NintexWorkflowCloud nwc = new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData
                        {
                            se_headerid = HeaderID,
                            se_payload = payload,
                            se_message = message
                        }
                    }
                };
                Task.Run(async () =>
                {
                    await NAC.TriggerWorkflowAsync(nwc);
                }).Wait();
                Console.WriteLine("Notif IT Team - Success");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Notif IT Team - Error - Message: {ex.Message}");
            }
        }

        public void SuccessCreateInvoice_NotifUser(string InvoiceID, string InvoiceNumber, int HeaderID)
        {
            #region Get User
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = $"SELECT TOP 1 Created_By, Document_Name FROM P2PDocuments WHERE ProSnap_FileID = {HeaderID}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
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
                        se_type = "3",
                        se_invoiceid = InvoiceID,
                        se_invoicenumber = InvoiceNumber,
                        se_filename = fileName,
                        se_useremail = UserEmail
                    }
                }
            };
            Task.Run(async () =>
            {
                await NAC.TriggerWorkflowAsync(nwc);
            }).Wait();
        }
    

        public async Task CreateInvoiceBulkAsync_New()
        {
            List<IDModel> HeaderIDs = GetVerifiedTransactionHeaderIDList();
            foreach (var data in HeaderIDs)
            {
                // Sync Transaction to DataTable
                int ID = data.ID;
                Console.WriteLine($"HeaderID: {ID}");
                SyncTransactionToDataTable(ID);

                // Get Invoice Header
                InvoiceHeaderModel header = GetInvoiceHeader(ID);
                //// Alter BusinessUnit to make error request
                //header.BusinessUnit = "Test";
                header.invoiceLines = PopulateInvoiceLine(ID);
                header.attachments = new List<AttachmentModel>();
                header.attachments.Add(GetDefaultAttachment(ID));
                if (header.InvoiceAmount.Contains(",00"))
                {
                    header.InvoiceAmount = header.InvoiceAmount.Replace(",00", "");
                }
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };

                // API Request
                using (var client = new HttpClient(handler))
                {
                    // Set up Basic Authentication
                    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    // Serialize payload to JSON
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };
                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(header, options);
                    //Console.WriteLine(jsonPayload);
                    Console.WriteLine($"Payload ready for Header ID {ID}, begin process create invoice");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var PayloadToStore = header;
                    foreach(var p in PayloadToStore.attachments)
                    {
                        p.FileContents = "";
                    }
                    string jsonPayloadToStore = JsonSerializer.Serialize(PayloadToStore, options);
                    try
                    {
                        // Make the POST request
                        HttpResponseMessage response = await client.PostAsync(url, content);

                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Success!");
                            //Console.WriteLine(responseContent);
                            JObject jsonObject = JObject.Parse(responseContent);
                            string invoiceId = jsonObject["InvoiceId"].ToString();
                            Console.WriteLine($"InvoiceId: {invoiceId}\nHeaderID: {ID}");
                            PostCreateInvoice_InsertLog(invoiceId, ID, jsonPayloadToStore, "Success", "Created", responseContent);
                            SuccessCreateInvoice_NotifUser(invoiceId, header.InvoiceNumber, ID);
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                            string errorContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Error details:");
                            Console.WriteLine(errorContent);
                            PostCreateInvoice_InsertLog("", ID, jsonPayloadToStore, "Failed", "Bad Request", errorContent);
                            NotifTeamIT(ID, jsonPayloadToStore, errorContent);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        PostCreateInvoice_InsertLog("", ID, jsonPayloadToStore, "Failed", ex.Message, "");
                    }
                    UpdateTransactionHeaders(ID);
                }
                
                
            }
        }
    }
}
