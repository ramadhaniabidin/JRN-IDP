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
using System.Web.Script.Serialization;

namespace JRN_IDP
{
    public class APIHandler
    {
        private readonly NACHandler NAC = new NACHandler();
        private readonly SPOHandler spo = new SPOHandler();
        private readonly string connString = ConfigurationManager.AppSettings["connString"];
        private readonly string url = ConfigurationManager.AppSettings["CreateInvoiceEndpoint_Production"];
        private readonly string successStatus = "Success";
        private readonly string failedStatus = "Failed";
        private readonly string idParam = "@HeaderID";
        private readonly HttpClientHandler httpHandler = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate };
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

        public static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        public EncryptionModel GetEncryption()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("usp_Encryption", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<EncryptionModel>(dt)[0];
                    }
                }
            }

        }

        public DecryptionModel Decrypt(string type_)
        {
            DataTable dt = new DataTable();
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("usp_Creds_GetByType", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@type", type_);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            var decryption = Utility.ConvertDataTableToList<DecryptionModel>(dt)[0];
            string username = Utility.Decrypt(Convert.FromBase64String(decryption.username_), key, iv);
            string password = Utility.Decrypt(Convert.FromBase64String(decryption.password_), key, iv);
            decryption.username_ = username;
            decryption.password_ = password;
            return decryption;
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
                    cmd.Parameters.AddWithValue(idParam, HeaderID);
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Success\n");
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
                        cmd.Parameters.AddWithValue(idParam, HeaderID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, successStatus, "OK", "InvoiceHeader");
                InvoiceHeaderModel invoiceHeader = Utility.ConvertDataTableToList<InvoiceHeaderModel>(dt)[0];
                return invoiceHeader;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Header Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, failedStatus, ex.Message, "InvoiceHeader");
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
                    cmd.Parameters.AddWithValue(idParam, HeaderID);
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
                    cmd.Parameters.AddWithValue(idParam, HeaderID);
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
                    cmd.Parameters.AddWithValue(idParam, HeaderID);
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
                string query = "UPDATE TransactionHeaders SET Is_Trying_ToPost = 1 WHERE ID = @id";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", id);
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
                        cmd.Parameters.AddWithValue(idParam, HeaderID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, successStatus, "OK", "InvoiceLine");
                return Utility.ConvertDataTableToList<InvoiceLineModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Line Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, failedStatus, ex.Message, "InvoiceLine");
                return new List<InvoiceLineModel>();
            }
        }

        public AttachmentModel GetDefaultAttachment(int HeaderID)
        {
            try
            {
                var Document = spo.GetP2PDocument(HeaderID);
                string siteUrl = $"https://jresourcesid.sharepoint.com";
                string fileRelativeURL = Document.Document_Url.Replace(siteUrl, "").Trim();
                string attachmentFileName = spo.GetAttachmentFileName(Document.Document_Name);
                string base64Content = spo.GenerateBase64Attachment(fileRelativeURL, Document.Document_Name, HeaderID);
                return new AttachmentModel
                {
                    Type = "File",
                    FileName = attachmentFileName + ".pdf",
                    Title = attachmentFileName + ".pdf",
                    Description = "Purchase Order",
                    FileContents = base64Content
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return new AttachmentModel();
            }

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
                        cmd.Parameters.AddWithValue(idParam, HeaderID);
                        cmd.Parameters.AddWithValue("@RowNumber", LineNumber);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                GeneratePayload_InsertLog(HeaderID, successStatus, "OK", "InvoiceDistribution");
                return Utility.ConvertDataTableToList<InvoiceDistributionModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Invoice Distribution Message: {ex.Message}");
                GeneratePayload_InsertLog(HeaderID, failedStatus, ex.Message, "InvoiceDistribution");
                return new List<InvoiceDistributionModel>();
            }
        }

        public void NotifTeamIT(int HeaderID, string payload, string message)
        {
            var spoFile = spo.SPOFile_GetByProSnapID(HeaderID);
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
                            se_message = message,
                            se_useremail = spoFile.Created_By,
                            se_filename = spoFile.Document_Name,
                            se_attachmenturl = spoFile.Attachment_URL
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
            var spoFile = spo.SPOFile_GetByProSnapID(HeaderID);
            NintexWorkflowCloud nwc = new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_type = "3",
                        se_invoiceid = InvoiceID,
                        se_invoicenumber = InvoiceNumber,
                        se_filename = spoFile.Document_Name,
                        se_useremail = spoFile.Created_By
                    }
                }
            };
            Task.Run(async () =>
            {
                await NAC.TriggerWorkflowAsync(nwc);
            }).Wait();
        }
    
        public InvoiceHeaderModel GenerateInvoicePayload(int ID)
        {
            var header = GetInvoiceHeader(ID);
            header.invoiceLines = PopulateInvoiceLine(ID);
            header.attachments = new List<AttachmentModel> { GetDefaultAttachment(ID) };
            if (header.InvoiceAmount.Contains(",00"))
            {
                header.InvoiceAmount = header.InvoiceAmount.Replace(",00", string.Empty);
            }
            return header;
        }

        public async Task CreateInvoiceBulkAsync_New()
        {
            List<IDModel> HeaderIDs = GetVerifiedTransactionHeaderIDList();
            foreach (var data in HeaderIDs)
            {
                SyncTransactionToDataTable(data.ID);
                var header = GenerateInvoicePayload(data.ID);
                if (header.invoiceLines.Count == 0 || header.attachments[0].Type == null) continue;
                using (var client = new HttpClient(httpHandler))
                {
                    DecryptionModel decryption = Decrypt("Oracle");
                    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{decryption.username_}:{decryption.password_}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    string jsonPayload = JsonSerializer.Serialize(header, serializerOptions);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var PayloadToStore = header;
                    foreach(var p in PayloadToStore.attachments)
                    {
                        p.FileContents = "";
                    }
                    string jsonPayloadToStore = JsonSerializer.Serialize(PayloadToStore, serializerOptions);
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(url, content);
                        CreateInvoice_ProcessResponse(response, data.ID, jsonPayloadToStore, header.InvoiceNumber);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        PostCreateInvoice_InsertLog("", data.ID, jsonPayloadToStore, failedStatus, ex.Message, "");
                    }
                }
            }
        }

        public void CreateInvoice_ProcessResponse(HttpResponseMessage response, int HeaderID, string jsonPayload, string invoiceNumber)
        {
            if (response.IsSuccessStatusCode)
            {
                string responseContent = response.Content.ReadAsStringAsync().Result;
                JObject jsonObject = JObject.Parse(responseContent);
                string invoiceId = jsonObject["InvoiceId"].ToString();
                PostCreateInvoice_InsertLog(invoiceId, HeaderID, jsonPayload, successStatus, "Created", responseContent);
                SuccessCreateInvoice_NotifUser(invoiceId, invoiceNumber, HeaderID);
            }
            else
            {
                string errorContent = response.Content.ReadAsStringAsync().Result;
                PostCreateInvoice_InsertLog("", HeaderID, jsonPayload, failedStatus, "Bad Request", errorContent);
                NotifTeamIT(HeaderID, jsonPayload, errorContent);
            }
        }

        public void InsertOracleSupplier(string SupplierName, string SupplierNumber)
        {
            try
            {
                using(var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using(var cmd  = new SqlCommand("usp_InsertMasterSuppplier_Oracle", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@SupplierName", SupplierName);
                        cmd.Parameters.AddWithValue("@SupplierNumber", SupplierNumber);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task GetOracleMasterSuppliers()
        {
            try
            {
                int offset = 100;
                int page = 1;
                while(offset <= 1400)
                {
                    string endpoint = $"https://fa-exke-saasfaprod1.fa.ocs.oraclecloud.com/fscmRestApi/resources/11.13.18.05/suppliers?limit=100&offset={offset}";
                    string username = "fin.impl";
                    string pass = "FIN@prod12";
                    var credentials = Encoding.ASCII.GetBytes($"{username}:{pass}");
                    var base64Creds = Convert.ToBase64String(credentials);
                    Console.WriteLine($"Page Number: {page}");
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Creds);
                        HttpResponseMessage response = await client.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            string result = await response.Content.ReadAsStringAsync();
                            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(result);
                            foreach (var sup in root.Items)
                            {
                                Console.WriteLine($"Supplier: {sup.Supplier}");
                                Console.WriteLine($"Supplier Number: {sup.SupplierNumber}\n");
                                InsertOracleSupplier(sup.Supplier, sup.SupplierNumber);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                            string error = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(error);
                        }
                    }
                    offset += 100;
                    page++;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
