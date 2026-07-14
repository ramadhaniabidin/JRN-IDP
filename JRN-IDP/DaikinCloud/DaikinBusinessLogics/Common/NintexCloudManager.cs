using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Common
{
    public class NintexCloudManager
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        private readonly DataTable dt = null;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
        private readonly CommonLogic _func = new CommonLogic();
        private readonly string NACBaseURL = ConfigurationManager.AppSettings["NAC_BASE_URL"];
        private readonly string NAC_TASK_URL = ConfigurationManager.AppSettings["NAC_TASKS_URL"];
        private readonly string GetAttachmentWorkflowURL = "/workflows/v1/designs/91ad22e2-f7bc-4853-864f-0720a2b7eb19/instances";
        private readonly string PAL_WORKFLOW_DEV = "34450d42-417e-4e68-987b-6649f25ed62d";
        private readonly string NON_COMMERCIAL_WORKFLOW_ID = ConfigurationManager.AppSettings["NONCOMM_WORKFLOW_ID"];
        private readonly string COMMERCIAL_WORKFLOW_ID = ConfigurationManager.AppSettings["COMMERCIAL_WORKFLOW_ID"];
        private readonly string CLAIM_REIMBURSEMENT_WORKFLOW_ID = ConfigurationManager.AppSettings["CLAIM_REIMBURSEMENT_WORKFLOW_ID"];
        private readonly string AFFILIATE_CLAIM_WORKFLOW_ID = ConfigurationManager.AppSettings["AFFILIATE_CLAIM_WORKFLOW_ID"];
        private readonly string SCHEDULE_PAYMENT_WORKFLOW_ID = ConfigurationManager.AppSettings["SCHEDULE_PAYMENT_WORKFLOW_ID"];
        private readonly string PO_SUBCON_WORKFLOW_ID = ConfigurationManager.AppSettings["PO_SUBCON_WORKFLOW_ID"];
        private readonly string BP_WORKFLOW_ID = ConfigurationManager.AppSettings["BP_WORKFLOW_ID"];
        private readonly string GET_ATTACHMENT_WORKFLOW_ID = ConfigurationManager.AppSettings["GET_ATTACHMENT_WORKFLOW_ID"];
        private readonly string CONTENT_TYPE = "application/json";
        private readonly string HEADERS_AUTHORIZATION = "authorization";
        private readonly string TOKEN_TYPE = "Bearer";
        private static readonly HttpClient client = new HttpClient();
        private const bool configureAwait = false;

        private string GetTokenRequestBody()
        {
            var requestBody = new
            {
                client_id = ConfigurationManager.AppSettings["NAC_CLIENT_ID"],
                client_secret = ConfigurationManager.AppSettings["NAC_CLIENT_SECRET"],
                grant_type = ConfigurationManager.AppSettings["NAC_GRANT_TYPE"]
            };
            return serializer.Serialize(requestBody);
        }

        public string GetNACWorfklowID(string Module_Code)
        {
            string Workflow_ID = string.Empty;
            using (var _con = new SqlConnection(Utility.GetSqlConnection()))
            {
                _con.Open();
                var query = "SELECT NAC_Workflow_Id FROM NWC_MasterModule WHERE Module_Code = @Module_Code";
                using (var cmd = new SqlCommand(query, _con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@Module_Code", Value = Module_Code, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Workflow_ID = reader.GetString(0);
                        }
                    }
                }
            }
            return Workflow_ID;
        }

        public async Task<string> GetNACWorkflowIDAsync(string Module_Code, SqlConnection _conn, SqlTransaction _trans)
        {
            string query = "SELECT NAC_Workflow_Id FROM NWC_MasterModule WHERE Module_Code = @Module_Code";
            using (SqlCommand cmd = new SqlCommand(query, _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(db.AddInParameter("Module_Code", Module_Code));
                object result = await cmd.ExecuteScalarAsync().ConfigureAwait(configureAwait);
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToString(result);
                }
            }
            return string.Empty;
        }

        public async Task<string> GetNACWorkflowIDAsync(string Module_Code)
        {
            using (SqlConnection _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _conn.OpenAsync().ConfigureAwait(configureAwait);
                string query = "SELECT NAC_Workflow_Id FROM NWC_MasterModule WHERE Module_Code = @Module_Code";
                using (SqlCommand cmd = new SqlCommand(query, _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(db.AddInParameter("Module_Code", Module_Code));
                    object result = await cmd.ExecuteScalarAsync().ConfigureAwait(configureAwait);
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToString(result);
                    }
                }
            }
            return string.Empty;
        }

        public string GetToken()
        {
            string url = ConfigurationManager.AppSettings["NAC_TOKEN_URL"];
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var requestBody = GetTokenRequestBody();
            byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var responseJson = reader.ReadToEnd();
                    var responseObject = serializer.Deserialize<dynamic>(responseJson);
                    return (string)responseObject["access_token"];
                }
            }
            catch (WebException webEx)
            {
                string errorDetails = string.Empty;
                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(errorStream))
                    {
                        errorDetails = reader.ReadToEnd();
                    }
                }
                throw new Exception($"Token API failed with status: {webEx.Status} | Details: {errorDetails}", webEx);
            }
        }

        public async Task<string> GetTokenAsync()
        {
            string url = ConfigurationManager.AppSettings["NAC_TOKEN_URL"];
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var httpContent = new StringContent(GetTokenRequestBody(), Encoding.UTF8, CONTENT_TYPE))
            {
                var response = await client.PostAsync(url, httpContent);
                if (!response.IsSuccessStatusCode) throw new Exception($"Token API failed with status: {response.StatusCode}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = serializer.Deserialize<dynamic>(responseJson);
                return (string)responseObject["access_token"];
            }
        }

        public static bool IsCurrentApprover(string FullName, string ModuleCode, string TransactionNumber)
        {
            int count = 0;
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                string query = "usp_CountAppointedTask";
                using (var cmd = new SqlCommand(query, _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ModuleCode", ModuleCode);
                    cmd.Parameters.AddWithValue("@FormNo", TransactionNumber);
                    cmd.Parameters.AddWithValue("@FullName", FullName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count = reader.GetInt32(0);
                        }
                        return count >= 1;
                    }
                }
            }
        }

        public StringContent GenerateApprovalPayload(string approval_value)
        {
            var payload = new { outcome = approval_value };
            var jsonPayload = serializer.Serialize(payload);
            return new StringContent(jsonPayload, Encoding.UTF8, CONTENT_TYPE);
        }

        public async Task<CommonResponseModel> CompleteNACTaskAsync(string approval_value, string task_id, string assignment_id)
        {
            try
            {
                string token = await GetTokenAsync();
                var stringContent = GenerateApprovalPayload(approval_value);
                string url = $"https://au.nintex.io/workflows/v2/tasks/{task_id}/assignments/{assignment_id}";
                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), url))
                {
                    request.Content = stringContent;
                    request.Headers.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, token);
                    using (var response = await client.SendAsync(request))
                    {
                        return new CommonResponseModel
                        {
                            Success = response.IsSuccessStatusCode,
                            Message = response.IsSuccessStatusCode ? "OK" : $"Failed to complete NAC Task | {await response.Content.ReadAsStringAsync()}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new CommonResponseModel { Success = false, Message = $"Error occurred at CompleteNACTask method | {ex.Message}" };
            }
        }

        public CommonResponseModel CompleteNACTask(string approval_value, string task_id, string assignment_id)
        {
            try
            {
                string url = $"{NAC_TASK_URL}/{task_id}/assignments/{assignment_id}";
                var payload = new { outcome = approval_value };
                string jsonPayload = serializer.Serialize(payload);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonPayload);

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PATCH";
                request.ContentType = CONTENT_TYPE;
                request.Headers.Add(HttpRequestHeader.Authorization, $"{TOKEN_TYPE} {GetToken()}");
                request.ContentLength = byteArray.Length;

                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    bool isSuccess = ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299);
                    return new CommonResponseModel
                    {
                        Success = isSuccess,
                        Message = isSuccess ? "OK" : $"Failed to complete NAC Task | Status: {response.StatusCode}"
                    };
                }
            }
            catch (WebException webEx)
            {
                string errorDetails = webEx.Message;
                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(errorStream))
                    {
                        errorDetails = reader.ReadToEnd();
                    }
                }
                return new CommonResponseModel { Success = false, Message = $"Failed to complete NAC Task | {errorDetails}" };
            }
            catch (Exception ex)
            {
                return new CommonResponseModel { Success = false, Message = $"Error occurred at CompleteNACTask method | {ex.Message}" };
            }
        }

        public static CurrentApproverModel NonCommercial_GetCurrentApprover(string listName, int itemID)
        {
            SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
            if (listName.ToUpper().Contains("CONTRACT"))
            {
                return new CurrentApproverModel
                {
                    Email = web.CurrentUser.Email.ToLower(),
                    UserName = web.CurrentUser.Name
                };
            }
            SPList list = web.Lists[listName];
            SPListItem listItem = list.GetItemById(itemID);
            string CurrApproverLogin = listItem["Approver_x0020_Login_x0020_Accou"].ToString();
            return new CurrentApproverModel
            {
                Email = web.EnsureUser(CurrApproverLogin).Email.ToLower(),
                UserName = web.EnsureUser(CurrApproverLogin).Name
            };
        }

        public CurrentApproverModel Commercial_GetTaskResponder(string Module_Code, int Transaction_ID, string Form_No)
        {
            try
            {
                SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
                return new CurrentApproverModel
                {
                    Email = web.CurrentUser.Email.ToLower(),
                    UserName = web.CurrentUser.LoginName,
                    FullName = web.CurrentUser.Name
                };
            }
            catch (Exception ex)
            {
                _func.SaveDataLog(Module_Code, Transaction_ID, Form_No, $"Error retrieving task responder | Error message: {ex.Message}");
                return null;
            }
        }

        public GetTaskResponseModel GetTasks_(string NAC_Guid, string Module_Code, int Transaction_ID, string Form_No)
        {
            try
            {
                string token = GetToken();
                string queryParam = $"?from=2024-01-01&workflowInstanceId={NAC_Guid}";
                string url = $"{NAC_TASK_URL}{queryParam}";
                HttpClient client1 = new HttpClient();
                client1.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
                var response = client1.GetAsync(url);
                var responseJson = response.Result.Content.ReadAsStringAsync().Result;
                dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
                var tasks = responseObject["tasks"];
                if (tasks == null)
                {
                    return new GetTaskResponseModel { Success = false, Message = $"There is no tasks found with instance id: {NAC_Guid}", Tasks = null };
                }
                return new GetTaskResponseModel
                {
                    Success = true,
                    Message = "OK",
                    Tasks = tasks
                };
            }
            catch (Exception ex)
            {
                _func.SaveDataLog(Module_Code, Transaction_ID, Form_No, $"Error retrieving tasks with instance id: {NAC_Guid} | Error Message: {ex.Message}");
                return new GetTaskResponseModel
                {
                    Success = false,
                    Message = $"Error occurred at GetTasks_ method | {ex.Message}",
                    Tasks = null
                };
            }
        }

        public async Task<TaskResponse> GetTask_ByInstanceID_Async(string Instance_ID)
        {
            string queryParam = $"?from=2025-02-01&workflowInstanceId={Instance_ID}";
            string endpoint = $"{NAC_TASK_URL}{queryParam}";
            using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, await GetTokenAsync());
                using (var response = await client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return serializer.Deserialize<TaskResponse>(responseJson);
                }
            }
        }

        public TaskResponse GetTasks(string Instance_ID)
        {
            string queryParam = $"?from=2025-02-01&workflowInstanceId={Instance_ID}";
            string url = $"{NAC_TASK_URL}{queryParam}";

            // 1. Initialize the request synchronously
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {GetToken()}");

            try
            {
                // 2. Execute the request synchronously
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string responseJson = reader.ReadToEnd();
                    var taskResponse = serializer.Deserialize<TaskResponse>(responseJson);
                    return taskResponse;
                }
            }
            catch (WebException webEx)
            {
                string errorDetails = string.Empty;
                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(errorStream))
                    {
                        errorDetails = reader.ReadToEnd();
                    }
                }

                // Re-throwing as HttpRequestException (or similar) to match your legacy exception contract
                throw new HttpRequestException($"Error: {webEx.Status} - {errorDetails}", webEx);
            }
        }

        public TaskAssignmentResponseModel GetTaskAssignment(string Instance_ID, string Transaction_No)
        {
            try
            {
                var tasks = Task.Run(async () =>
                {
                    return await GetTask_ByInstanceID_Async(Instance_ID);
                }).GetAwaiter().GetResult();
                var task = tasks.Tasks.FirstOrDefault(t => t.Name.Contains(Transaction_No));
                return new TaskAssignmentResponseModel
                {
                    Success = true,
                    Message = "OK",
                    TaskAssignments = task
                };
            }
            catch (Exception ex)
            {
                return new TaskAssignmentResponseModel
                {
                    Success = false,
                    Message = $"Error at TaskAssignmentResponseModel method | {ex.Message}",
                    TaskAssignments = null
                };
            }
        }

        public async Task<TaskAssignmentResponseModel> GetTaskAssignmentAsync(string Instance_ID, string Transaction_No)
        {
            try
            {
                var tasks = await GetTask_ByInstanceID_Async(Instance_ID).ConfigureAwait(false);
                var task = tasks.Tasks.FirstOrDefault(t => t.Name.Contains(Transaction_No));
                return new TaskAssignmentResponseModel
                {
                    Success = true,
                    Message = "OK",
                    TaskAssignments = task
                };
            }
            catch (Exception ex)
            {
                return new TaskAssignmentResponseModel
                {
                    Success = false,
                    Message = $"Error at TaskAssignmentResponseModel method | {ex.Message}",
                    TaskAssignments = null
                };
            }
        }

        public dynamic GetTask_ByFormNo(string Form_No, string Module_Code, int Transaction_ID, IEnumerable<dynamic> Tasks)
        {
            try
            {
                return Tasks.FirstOrDefault(t => t["name"].Contains($"{Form_No}"));
            }
            catch (Exception ex)
            {
                _func.SaveDataLog(Module_Code, Transaction_ID, Form_No, $"Error retrieving task for transaction: {Form_No} | Error Message: {ex.Message}");
                return null;
            }
        }

        public static async Task<CommonResponseModel> ProcessNACTaskAsync(TaskItem task, string assignee, string approvalValue)
        {
            try
            {
                var targetAssignment = task.TaskAssignments.FirstOrDefault(ta => ta.Assignee.ToLowerInvariant().Contains(assignee.ToLowerInvariant()));
                #region Early return
                if (targetAssignment == null || string.IsNullOrEmpty(targetAssignment.Id))
                {
                    return new CommonResponseModel
                    {
                        Success = false,
                        Message = $"There is no active task for {assignee}"
                    };
                }
                #endregion
                return await new NintexCloudManager().CompleteNACTaskAsync(approvalValue, task.Id, targetAssignment.Id);
            }
            catch (Exception ex)
            {
                return new CommonResponseModel { Message = $"Error at ProcessNACTask method | {ex.Message}" };
            }
        }

        public static CommonResponseModel ProcessNACTask(TaskItem task, string assignee, string approvalValue)
        {
            try
            {
                var targetAssignment = task.TaskAssignments.FirstOrDefault(ta => ta.Assignee.ToLowerInvariant().Contains(assignee.ToLowerInvariant()));
                if (targetAssignment == null || string.IsNullOrEmpty(targetAssignment.Id))
                {
                    return new CommonResponseModel
                    {
                        Success = false,
                        Message = $"There is no active task for {assignee}"
                    };
                }
                return new NintexCloudManager().CompleteNACTask(approvalValue, task.Id, targetAssignment.Id);
            }
            catch (Exception ex)
            {
                return new CommonResponseModel { Message = $"Error at ProcessNACTask method | {ex.Message}" };
            }
        }

        public CommonResponseModel ProcessNACTask(dynamic task, string assigneeEmail, string approvalValue)
        {
            try
            {
                string task_id = Convert.ToString(task["id"]);
                IEnumerable<dynamic> assignments = task["taskAssignments"];
                var targetAssignment = assignments.FirstOrDefault(t => t["assignee"].ToLower().Contains(assigneeEmail));
                string assignmentID = Convert.ToString(targetAssignment["id"]);
                if (string.IsNullOrEmpty(assignmentID))
                {
                    return new CommonResponseModel
                    {
                        Success = false,
                        Message = $"There is no active task for {assigneeEmail}"
                    };
                }
                var CompleteTaskResult = CompleteNACTask(approvalValue, task_id, assignmentID);
                if (!CompleteTaskResult.Success)
                {
                    return new CommonResponseModel { Success = false, Message = CompleteTaskResult.Message };
                }
                return new CommonResponseModel { Success = true, Message = "OK" };
            }
            catch (Exception ex)
            {
                return new CommonResponseModel { Success = false, Message = $"Error occurred at ProcessNACTask method | {ex.Message}" };
            }
        }

        public string GetNacInfo(string key)
        {
            string info = "";
            db.OpenConnection(ref conn);
            db.cmd.CommandText = $"SELECT [value] FROM dbo.NAC_Info WHERE [key] = @key";
            db.cmd.CommandType = System.Data.CommandType.Text;
            db.cmd.Parameters.Clear();
            db.AddInParameter(db.cmd, "key", key);
            SqlDataReader reader = db.cmd.ExecuteReader();
            while (reader.Read())
            {
                info = reader.GetString(0);
            }
            db.CloseConnection(ref conn);
            db.CloseDataReader(reader);
            return info;
        }

        public NintexWorkflowCloud NonCommercial_GenerateNACPayload(int Item_ID, int Header_ID, string Module_Code, string List_Name)
        {
            NintexWorkflowCloud nwc = new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_itemid = Item_ID,
                        se_headerid = Header_ID,
                        se_modulecode = Module_Code,
                        se_listname = List_Name
                    }
                },
                url = NACBaseURL
            };
            return nwc;
        }

        public string NonCommercial_StartWorkflow(int Item_ID, int Header_ID, string Module_Code, string List_Name)
        {
            var payload = NonCommNACPayload(Header_ID, Item_ID, Module_Code, List_Name);
            TriggerWorkflow(payload);
            return "OK";
        }

        public async Task Commercial_StartWorkflow(int Item_ID, int Header_ID, string Module_Code, string WorkflowId)
        {
            try
            {
                NintexWorkflowCloud nwc = new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData
                        {
                            se_itemid = Item_ID,
                            se_headerid = Header_ID,
                            se_modulecode = Module_Code
                        }
                    },
                    url = NACBaseURL
                };
                string endpoint = "/workflows/v1/designs/" + WorkflowId + "/instances";
                var client1 = new HttpClient();
                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, await GetTokenAsync());
                client1.BaseAddress = new Uri(nwc.url);
                client1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(new JavaScriptSerializer().Serialize(nwc.param), Encoding.UTF8, CONTENT_TYPE);
                using (var response = await client1.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    string SysMessage = response.IsSuccessStatusCode ? "OK" : result;
                    string InstanceID = response.IsSuccessStatusCode ? result : "-";
                    int TriggerStatus = response.IsSuccessStatusCode ? 1 : -1;
                    StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, InstanceID, SysMessage, TriggerStatus);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Request error: {httpEx.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", $"Request error: {httpEx.Message}", -1);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", $"General error: {ex.Message}", -1);
                throw;
            }
        }

        public NintexWorkflowCloud GenerateNACPayload(int HeaderID, int ItemID, string ModuleCode, string ListName, string PoNumber = "")
        {
            var nonCommercials = new List<string> { "M014", "M015", "M016", "M017", "M018", "M020" };
            if (nonCommercials.Contains(ModuleCode)) return NonCommNACPayload(HeaderID, ItemID, ModuleCode, ListName);
            else if (ModuleCode == "M019") return POSubconPayload(ItemID, ModuleCode);
            else if (ModuleCode == "M029" || ModuleCode == "M030") return BPPayload(ItemID, ModuleCode);
            else if ((ModuleCode == "M027" || ModuleCode == "M028")) return AffiliatePayload(ItemID, ModuleCode);
            else if (ModuleCode == "M019-01") return GetAttachmentPayload(ItemID, PoNumber);
            return ClaimReimbursementPayload(ItemID, ModuleCode);
        }

        public async Task<NintexWorkflowCloud> GenerateNACPayloadAsync(int HeaderID, int ItemID, string ModuleCode, string ListName, string PoNumber = "")
        {
            List<string> nonCommercials = new List<string> { "M014", "M015", "M016", "M017", "M018", "M020" };
            if (nonCommercials.Contains(ModuleCode)) return await NonCommNACPayloadAsync(HeaderID, ItemID, ModuleCode, ListName).ConfigureAwait(configureAwait);
            else if (ModuleCode == "M019") return await SubconPayloadAsync(ItemID, ModuleCode).ConfigureAwait(configureAwait);
            else if (ModuleCode == "M029" || ModuleCode == "M030") return await BPPayloadAsync(ItemID, ModuleCode).ConfigureAwait(configureAwait);
            else if ((ModuleCode == "M027" || ModuleCode == "M028")) return await AffiliatePayloadAsync(ItemID, ModuleCode).ConfigureAwait(configureAwait);
            else if (ModuleCode == "M019-01") return await GetAttachmentPayloadAsync(ItemID, PoNumber, ModuleCode).ConfigureAwait(configureAwait);
            return await ClaimReimbursementPayloadAsync(ItemID, ModuleCode).ConfigureAwait(configureAwait);
        }

        private async Task<NintexWorkflowCloud> NonCommNACPayloadAsync(int HeaderID, int ItemID, string ModuleCode, string ListName)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_headerid = HeaderID,
                        se_itemid = ItemID,
                        se_modulecode = ModuleCode,
                        se_listname = ListName
                    }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud NonCommNACPayload(int HeaderID, int ItemID, string ModuleCode, string ListName)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_headerid = HeaderID,
                        se_itemid = ItemID,
                        se_modulecode = ModuleCode,
                        se_listname = ListName
                    }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{NON_COMMERCIAL_WORKFLOW_ID}/instances"
            };
        }

        private async Task<NintexWorkflowCloud> SubconPayloadAsync(int ItemID, string ModuleCode)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud POSubconPayload(int ItemID, string ModuleCode)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{PO_SUBCON_WORKFLOW_ID}/instances"
            };
        }

        private async Task<NintexWorkflowCloud> BPPayloadAsync(int ItemID, string ModuleCode)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud BPPayload(int ItemID, string ModuleCode)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{BP_WORKFLOW_ID}/instances"
            };
        }

        private async Task<NintexWorkflowCloud> AffiliatePayloadAsync(int ItemID, string ModuleCode)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_headerid = ItemID, se_tablename = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud AffiliatePayload(int ItemID, string ModuleCode)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_headerid = ItemID, se_tablename = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{AFFILIATE_CLAIM_WORKFLOW_ID}/instances"
            };
        }

        private async Task<NintexWorkflowCloud> GetAttachmentPayloadAsync(int ItemID, string PoNumber, string ModuleCode)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_ponumber = PoNumber }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud GetAttachmentPayload(int ItemID, string PoNumber)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_ponumber = PoNumber }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{GET_ATTACHMENT_WORKFLOW_ID}/instances"
            };
        }

        private async Task<NintexWorkflowCloud> ClaimReimbursementPayloadAsync(int ItemID, string ModuleCode)
        {
            string workflowId = await GetNACWorkflowIDAsync(ModuleCode).ConfigureAwait(configureAwait);
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_tablename = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{workflowId}/instances"
            };
        }

        private NintexWorkflowCloud ClaimReimbursementPayload(int ItemID, string ModuleCode)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData { se_itemid = ItemID, se_tablename = ModuleCode }
                },
                url = NACBaseURL,
                endpoint = $"/workflows/v1/designs/{CLAIM_REIMBURSEMENT_WORKFLOW_ID}/instances"
            };
        }

        public async Task HandleResponse(int Header_ID, int Item_ID, string Module_Code, HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            string message = response.IsSuccessStatusCode ? "OK" : responseContent;
            string instanceID = response.IsSuccessStatusCode ? responseContent : "-";
            int triggerStatus = response.IsSuccessStatusCode ? 1 : -1;
            StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, instanceID, message, triggerStatus);
        }

        public async Task PAL_StartWorkflow(int Item_ID, string Module_Code)
        {
            var param = await GenerateNACPayloadAsync(0, Item_ID, Module_Code, "");
            await StartNWC(param);
        }

        public void TriggerWorkflow(NintexWorkflowCloud Payload)
        {
            string result = "";
            string message = "";
            int status = 0;
            try
            {
                string endpoint = Payload.url.TrimEnd('/') + "/" + Payload.endpoint.TrimStart('/');
                string requestBody = serializer.Serialize(Payload.param);
                byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);

                var request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "POST";
                request.ContentType = CONTENT_TYPE;
                request.Accept = CONTENT_TYPE;
                request.Headers.Add(HttpRequestHeader.Authorization, $"{TOKEN_TYPE} {GetToken()}");
                request.ContentLength = byteArray.Length;

                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string responseBody = reader.ReadToEnd();
                    result = responseBody;
                    message = "OK";
                    status = 1;
                }
            }
            catch (WebException webEx)
            {
                status = -1;
                result = "-";

                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(errorStream))
                    {
                        string responseBody = reader.ReadToEnd();
                        var statusCode = (int)((HttpWebResponse)webEx.Response).StatusCode;
                        message = $"API Error ({statusCode}): {responseBody}";
                    }
                }
                else
                {
                    message = webEx.Message;
                }
                throw;
            }
            catch (Exception ex)
            {
                result = "-";
                message = ex.Message;
                status = -1;
                throw;
            }
            finally
            {
                StartNAC_InsertLog(Payload.param.startData.se_modulecode, Payload.param.startData.se_itemid,
                    Payload.param.startData.se_headerid, result, message, status);
            }
        }

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {
            string result = "";
            string message = "";
            int status = 0;
            try
            {
                string endpoint = nwc.url.TrimEnd('/') + "/" + nwc.endpoint.TrimStart('/');
                string requestBody = serializer.Serialize(nwc.param);
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                    request.Headers.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, await GetTokenAsync().ConfigureAwait(configureAwait));
                    using (var content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE))
                    {
                        request.Content = content;
                        using (var response = await client.SendAsync(request).ConfigureAwait(configureAwait))
                        {
                            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(configureAwait);
                            if (response.IsSuccessStatusCode)
                            {
                                result = responseBody;
                                message = "OK";
                                status = 1;
                            }
                            else
                            {
                                result = "-";
                                message = $"API Error ({(int)response.StatusCode}): {responseBody}";
                                status = -1;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                result = "-";
                message = httpEx.Message;
                status = -1;
                throw;
            }
            catch (Exception ex)
            {
                result = "-";
                message = ex.Message;
                status = -1;
                throw;
            }
            finally
            {
                await StartNAC_InsertLogAsync(new NACLogModel
                {
                    Module_Code = nwc.param.startData.se_modulecode,
                    Instance_ID = result,
                    Item_ID = nwc.param.startData.se_itemid,
                    Sys_Message = message,
                    Trigger_Status = status,
                    Transaction_ID = nwc.param.startData.se_headerid
                }).ConfigureAwait(configureAwait);
            }
        }

        public async Task StartNWC(NintexWorkflowCloud nwc, SqlConnection _conn, SqlTransaction _trans)
        {
            string result = "";
            string message = "";
            int status = 0;
            try
            {
                string endpoint = nwc.url.TrimEnd('/') + "/" + nwc.endpoint.TrimStart('/');
                string requestBody = serializer.Serialize(nwc.param);
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                    request.Headers.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, await GetTokenAsync().ConfigureAwait(configureAwait));
                    using (var content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE))
                    {
                        request.Content = content;
                        using (var response = await client.SendAsync(request).ConfigureAwait(configureAwait))
                        {
                            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(configureAwait);
                            if (response.IsSuccessStatusCode)
                            {
                                result = responseBody;
                                message = "OK";
                                status = 1;
                            }
                            else
                            {
                                result = "-";
                                message = $"API Error ({(int)response.StatusCode}): {responseBody}";
                                status = -1;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                result = "-";
                message = httpEx.Message;
                status = -1;
                throw;
            }
            catch (Exception ex)
            {
                result = "-";
                message = ex.Message;
                status = -1;
                throw;
            }
            finally
            {
                await StartNAC_InsertLogAsync(new NACLogModel
                {
                    Module_Code = nwc.param.startData.se_modulecode,
                    Instance_ID = result,
                    Item_ID = nwc.param.startData.se_itemid,
                    Sys_Message = message,
                    Trigger_Status = status,
                    Transaction_ID = nwc.param.startData.se_headerid
                }, _conn, _trans).ConfigureAwait(configureAwait);
            }
        }

        public void StartNAC_InsertLog(string Module_Code, int Item_ID, int Transaction_ID, string Instance_ID, string Sys_Message, int Trigger_Status)
        {
            using (var _conn = new SqlConnection(db.GetSQLConnectionString()))
            using (var cmd = new SqlCommand("usp_TriggerNAC_InsertLog", _conn))
            {
                _conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(cmd, "Module_Code", Module_Code);
                db.AddInParameter(cmd, "Item_ID", Item_ID);
                db.AddInParameter(cmd, "Transaction_ID", Transaction_ID);
                db.AddInParameter(cmd, "Instance_ID", Instance_ID);
                db.AddInParameter(cmd, "Sys_Message", Sys_Message);
                db.AddInParameter(cmd, "Trigger_Status", Trigger_Status);
                cmd.ExecuteNonQuery();
            }
        }

        public async Task StartNAC_InsertLogAsync(NACLogModel model)
        {
            using (SqlConnection _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _conn.OpenAsync().ConfigureAwait(configureAwait);
                using (var cmd = new SqlCommand("usp_TriggerNAC_InsertLog", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(db.AddInParameter("Module_Code", model.Module_Code));
                    cmd.Parameters.Add(db.AddInParameter("Item_ID", model.Item_ID));
                    cmd.Parameters.Add(db.AddInParameter("Transaction_ID", model.Transaction_ID));
                    cmd.Parameters.Add(db.AddInParameter("Instance_ID", model.Instance_ID));
                    cmd.Parameters.Add(db.AddInParameter("Sys_Message", model.Sys_Message));
                    cmd.Parameters.Add(db.AddInParameter("Trigger_Status", model.Trigger_Status));
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(configureAwait);
                }
            }

        }

        public async Task StartNAC_InsertLogAsync(NACLogModel model, SqlConnection _conn, SqlTransaction _trans)
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync().ConfigureAwait(configureAwait);
            }
            using (var cmd = new SqlCommand("usp_TriggerNAC_InsertLog", _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(db.AddInParameter("Module_Code", model.Module_Code));
                cmd.Parameters.Add(db.AddInParameter("Item_ID", model.Item_ID));
                cmd.Parameters.Add(db.AddInParameter("Transaction_ID", model.Transaction_ID));
                cmd.Parameters.Add(db.AddInParameter("Instance_ID", model.Instance_ID));
                cmd.Parameters.Add(db.AddInParameter("Sys_Message", model.Sys_Message));
                cmd.Parameters.Add(db.AddInParameter("Trigger_Status", model.Trigger_Status));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(configureAwait);
            }
        }

        #region PO Subcon Approval Action
        public dynamic POSubconGetTask(string NAC_Guid, string Form_No)
        {
            string token = GetToken();
            string queryParam = $"?from=2024-01-01&workflowInstanceId={NAC_Guid}";
            string url = $"{NAC_TASK_URL}{queryParam}";
            HttpClient client1 = new HttpClient();
            client1.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
            var response = client1.GetAsync(url);
            var responseJson = response.Result.Content.ReadAsStringAsync().Result;
            dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            var tasks = responseObject["tasks"];
            dynamic task = null;
            foreach (var t in tasks)
            {
                if (t["name"].Contains(Form_No))
                {
                    task = t;
                    break;
                }
            }
            return task;
        }
        #endregion
    }
}