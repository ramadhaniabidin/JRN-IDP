using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly CommonLogic _func = new CommonLogic();
        private readonly string NACBaseURL = ConfigurationManager.AppSettings["NAC_BASE_URL"];
        private readonly string GetAttachmentWorkflowURL = "/workflows/v1/designs/91ad22e2-f7bc-4853-864f-0720a2b7eb19/instances";
        private readonly string PAL_WORKFLOW_DEV = "34450d42-417e-4e68-987b-6649f25ed62d";
        private readonly string CONTENT_TYPE = "application/json";
        private readonly string HEADERS_AUTHORIZATION = "authorization";
        private readonly string TOKEN_TYPE = "Bearer";

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

        public string GetToken()
        {
            string url = ConfigurationManager.AppSettings["NAC_TOKEN_URL"];
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
            var HttpContent = new StringContent(jsonBody, Encoding.UTF8, CONTENT_TYPE);
            var response = client.PostAsync(url, HttpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            string accessToken = responseObject["access_token"];
            return accessToken;

        }

        public IEnumerable<dynamic> GetTasks()
        {
            string url = System.Configuration.ConfigurationManager.AppSettings["NAC:task_url"].ToString();
            string token = GetToken();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
            var response = client.GetAsync(url).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            var tasks = responseObject.tasks;
            return tasks;
        }

        public static bool IsCurrentApprover(string FullName, string ModuleCode, string TransactionNumber)
        {
            int count = 0;
            using(var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                string query = "usp_CountAppointedTask";
                using(var cmd = new SqlCommand(query, _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ModuleCode", ModuleCode);
                    cmd.Parameters.AddWithValue("@FormNo", TransactionNumber);
                    cmd.Parameters.AddWithValue("@FullName", FullName);
                    using(var reader = cmd.ExecuteReader())
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
            var jsonPayload = new JavaScriptSerializer().Serialize(payload);
            return new StringContent(jsonPayload, Encoding.UTF8, CONTENT_TYPE);
        }

        public CommonResponseModel CompleteNACTask(string approval_value, string task_id, string assignment_id)
        {
            try
            {
                string token = GetToken();
                var stringContent = GenerateApprovalPayload(approval_value);
                string url = $"https://au.nintex.io/workflows/v2/tasks/{task_id}/assignments/{assignment_id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, token);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                request.Content = stringContent;
                var response = client.SendAsync(request).Result;
                return new CommonResponseModel
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode ? "OK" : $"Failed to complete NAC Task | {response.Content.ReadAsStringAsync().Result}"
                };
            }
            catch(Exception ex)
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
                SPWeb web = new SPSite(Utility.SpSiteUrl_DEV).OpenWeb();
                return new CurrentApproverModel
                {
                    Email = web.CurrentUser.Email.ToLower(),
                    UserName = web.CurrentUser.LoginName,
                    FullName = web.CurrentUser.Name
                };
            }
            catch(Exception ex)
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
                string url = $"{ConfigurationManager.AppSettings["NAC_TASKS_URL"]}&workflowInstanceId={NAC_Guid}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
                var response = client.GetAsync(url);
                var responseJson = response.Result.Content.ReadAsStringAsync().Result;
                dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
                var tasks = responseObject["tasks"];
                if(tasks == null)
                {
                    return new GetTaskResponseModel { Success = false, Message = $"There is no tasks found with instance id: {NAC_Guid}", Tasks = null };
                }
                return new GetTaskResponseModel
                {
                    Success = true, Message = "OK", Tasks = tasks
                };
            }
            catch(Exception ex)
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
            string url = $"https://au.nintex.io/workflows/v2/tasks?from=2025-02-01&workflowInstanceId={Instance_ID}";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {GetToken()}");
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                var TaskResponse = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 }.Deserialize<TaskResponse>(responseJson);
                return TaskResponse;
            }
        }

        public TaskAssignmentResponseModel GetTaskAssignment(string Instance_ID, string Transaction_No)
        {
            try
            {
                var tasks = GetTask_ByInstanceID_Async(Instance_ID).Result;
                var task = tasks.Tasks.FirstOrDefault(t => t.Name.Contains(Transaction_No));
                return new TaskAssignmentResponseModel
                {
                    Success = true, Message = "OK", TaskAssignments = task
                };
            }
            catch(Exception ex)
            {
                return new TaskAssignmentResponseModel
                {
                    Success = false, Message = $"Error at TaskAssignmentResponseModel method | {ex.Message}", TaskAssignments = null
                };
            }
        }

        public dynamic GetTask_ByFormNo(string Form_No, string Module_Code, int Transaction_ID, IEnumerable<dynamic> Tasks)
        {
            try
            {
                return Tasks.FirstOrDefault(t => t["name"].Contains($"{Form_No}"));
            }
            catch(Exception ex)
            {
                _func.SaveDataLog(Module_Code, Transaction_ID, Form_No, $"Error retrieving task for transaction: {Form_No} | Error Message: {ex.Message}");
                return null;
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
            catch(Exception ex)
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
                        Success = false, Message = $"There is no active task for {assigneeEmail}"
                    };
                }
                var CompleteTaskResult = CompleteNACTask(approvalValue, task_id, assignmentID);
                if (!CompleteTaskResult.Success)
                {
                    return new CommonResponseModel { Success = false, Message = CompleteTaskResult.Message };
                }
                return new CommonResponseModel { Success = true, Message = "OK" };
            }
            catch(Exception ex)
            {
                return new CommonResponseModel { Success = false, Message = $"Error occurred at ProcessNACTask method | {ex.Message}" };
            }
        }

        public IEnumerable<dynamic> GetFailedInstanceByDocNo(string DocNo)
        {
            string token = GetToken1();
            string workflow_id = GetNacInfo("PO_Subcon_WF_ID");
            string url = $"https://au.nintex.io/workflows/v2/designs/{workflow_id}/instances?status=Failed&order=ASC";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
                var response = client.GetAsync(url).Result;
                var responseJson = response.Content.ReadAsStringAsync().Result;
                dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
                return responseObject;
            }
        }

        public string GetToken1()
        {
            string url = GetNacInfo("token_url");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();

            var requestBody = new
            {
                client_id = GetNacInfo("client_id"),
                client_secret = GetNacInfo("client_secret"),
                grant_type = GetNacInfo("grant_type")
            };
            var jsonBody = new JavaScriptSerializer().Serialize(requestBody);
            var HttpContent = new StringContent(jsonBody, Encoding.UTF8, CONTENT_TYPE);
            var response = client.PostAsync(url, HttpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            string accessToken = responseObject["access_token"];


            return accessToken;
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

        public Dictionary<string, object> GenerateNACRequest(NintexWorkflowCloud nwc, string endpoint)
        {
            string token = GetToken();
            string requestBody = new JavaScriptSerializer().Serialize(nwc.param);
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(nwc.url)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, token);
            client.BaseAddress = new Uri(nwc.url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE)
            };
            return new Dictionary<string, object>
                {
                    {"Request", request},
                    {"Client", client }
                };
        }

        public async Task<string> NonCommercial_StartWorkflow(int Item_ID, int Header_ID, string Module_Code, string List_Name)
        {
            try
            {
                NintexWorkflowCloud nwc = NonCommercial_GenerateNACPayload(Item_ID, Header_ID, Module_Code, List_Name);
                string endpoint = "/workflows/v1/designs/a8091cb6-6bd4-42e8-b8b9-be00e066574f/instances";
                var clientRequestPair = GenerateNACRequest(nwc, endpoint);
                HttpClient client = (HttpClient)clientRequestPair["Client"];
                HttpRequestMessage request = (HttpRequestMessage)clientRequestPair["Request"];
                using (client)
                using(request)
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, result, "OK", 1);
                        return result;
                    }
                    else
                    {
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", errorContent, -1);
                        return "{}";
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Request error: {httpEx.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", httpEx.Message, -1);
                return httpEx.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", ex.Message, -1);
                return ex.Message;
            }
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
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, GetToken());
                client.BaseAddress = new Uri(nwc.url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(new JavaScriptSerializer().Serialize(nwc.param), Encoding.UTF8, CONTENT_TYPE);
                using (var response = await client.SendAsync(request))
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

        public NintexWorkflowCloud NonComm_GenerateNACPayload(int Header_ID, int Item_ID, string Module_Code, string List_Name)
        {
            return new NintexWorkflowCloud
            {
                param = new NWCParamModel
                {
                    startData = new StartData
                    {
                        se_headerid = Header_ID,
                        se_itemid = Item_ID,
                        se_modulecode = Module_Code,
                        se_listname = List_Name
                    }
                },
                url = NACBaseURL
            };
        }

        public NintexWorkflowCloud GenerateNACPayload(int HeaderID, int ItemID, string ModuleCode, string ListName, string PoNumber = "")
        {
            List<string> nonCommercials = new List<string> { "M014", "M015", "M016", "M017", "M018", "M020" };
            #region Non Commercials
            if (nonCommercials.Contains(ModuleCode))
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData
                        {
                            se_headerid = HeaderID, se_itemid = ItemID, se_modulecode = ModuleCode, se_listname = ListName
                        }
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
            #region Commercial Subcon
            else if (ModuleCode == "M019")
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
            #region SAP Business Partners
            else if (ModuleCode == "M029" || ModuleCode == "M030")
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData { se_itemid = ItemID, se_modulecode = ModuleCode }
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
            #region Affiliate Claim & Not Claim
            else if((ModuleCode == "M027" || ModuleCode == "M028"))
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData { se_headerid = ItemID, se_tablename = ModuleCode }
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
            #region PO Subcon Get Attachment From SF
            else if(ModuleCode == "M019-01")
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData { se_itemid = ItemID, se_ponumber = PoNumber }
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
            #region Claim Reimbursement
            else
            {
                return new NintexWorkflowCloud
                {
                    param = new NWCParamModel
                    {
                        startData = new StartData { se_itemid = ItemID, se_tablename = ModuleCode}
                    },
                    url = NACBaseURL,
                    endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(ModuleCode)}/instances"
                };
            }
            #endregion
        }

        public async Task HandleResponse(int Header_ID, int Item_ID, string Module_Code, HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            string message = response.IsSuccessStatusCode ? "OK" : responseContent;
            string instanceID = response.IsSuccessStatusCode ? responseContent : "-";
            int triggerStatus = response.IsSuccessStatusCode ? 1 : -1;
            StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, instanceID, message, triggerStatus);
        }

        public async Task NonComm_StartNACWorkflow(int Header_ID, int Item_ID, string Module_Code, string List_Name)
        {
            try
            {
                string endpoint = $"/workflows/v1/designs/{GetNACWorfklowID(Module_Code)}/instances";
                var param = NonCommercial_GenerateNACPayload(Item_ID, Header_ID, Module_Code, List_Name);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, GetToken());
                client.BaseAddress = new Uri(param.url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(serializer.Serialize(param.param), Encoding.UTF8, CONTENT_TYPE);
                var response = await client.SendAsync(request);
                await HandleResponse(Header_ID, Item_ID, Module_Code, response);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Request error: {httpEx.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, 0, "-", $"Request error: {httpEx.Message}", -1);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                StartNAC_InsertLog(Module_Code, Item_ID, 0, "-", $"General error: {ex.Message}", -1);
                throw;
            }
        }

        public async Task PAL_StartWorkflow(int Item_ID, string Module_Code)
        {
            var param = GenerateNACPayload(0, Item_ID, Module_Code, "");
            await StartNWC(param);
        }

        public async Task NonCommercial_StartWorkflow_V2(int Item_ID, int Header_ID, string Module_Code, string List_Name)
        {
            try
            {
                NintexWorkflowCloud nwc = new NintexWorkflowCloud();
                nwc.param = new NWCParamModel();
                nwc.param.startData = new StartData();
                nwc.param.startData.se_itemid = Item_ID;
                nwc.param.startData.se_headerid = Header_ID;
                nwc.param.startData.se_modulecode = Module_Code;
                nwc.param.startData.se_listname = List_Name;

                nwc.url = NACBaseURL;
                //string endpoint = "/workflows/v1/designs/a8091cb6-6bd4-42e8-b8b9-be00e066574f/instances";         // prod
                string endpoint = "/workflows/v1/designs/d6f0b3f9-50d1-46b6-abdc-46b8252dd3b7/instances";         // dev
                string token = GetToken();
                Console.WriteLine(token);
                string requestBody = new JavaScriptSerializer().Serialize(nwc.param);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, token);
                client.BaseAddress = new Uri(nwc.url);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE);
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, result, "OK", 1);
                    }
                    else
                    {
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        StartNAC_InsertLog(Module_Code, Item_ID, Header_ID, "-", errorContent, -1);
                    }
                    //return result; //instance guid
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

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {
            try
            {
                string sBody = new JavaScriptSerializer().Serialize(nwc.param);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(nwc.url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, GetToken());
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.endpoint);
                request.Content = new StringContent(sBody, Encoding.UTF8, CONTENT_TYPE);
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    string SysMessage = response.IsSuccessStatusCode ? "OK" : result;
                    string InstanceID = response.IsSuccessStatusCode ? result : "-";
                    int TriggerStatus = response.IsSuccessStatusCode ? 1 : -1;
                    StartNAC_InsertLog(nwc.param.startData.se_modulecode, nwc.param.startData.se_itemid, nwc.param.startData.se_headerid, InstanceID, SysMessage, TriggerStatus);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Request error: {httpEx.Message}");
                StartNAC_InsertLog(nwc.param.startData.se_modulecode, nwc.param.startData.se_itemid, nwc.param.startData.se_headerid, "-", $"Request error: {httpEx.Message}", -1);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                StartNAC_InsertLog(nwc.param.startData.se_modulecode, nwc.param.startData.se_itemid, nwc.param.startData.se_headerid, "-", $"General error: {ex.Message}", -1);
                throw;
            }
        }

        public async Task Trigger_GetAttachment_Workflow(NintexWorkflowCloud nwcModel)
        {
            Console.WriteLine("Begin retriggering stopped approval workflow");
            nwcModel.url = NACBaseURL;
            string token = GetToken();
            string requestBody = serializer.Serialize(nwcModel.param);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TOKEN_TYPE, token);
            client.BaseAddress = new Uri(nwcModel.url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetAttachmentWorkflowURL);
            request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE);
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public void StartNAC_InsertLog(string Module_Code, int Item_ID, int Transaction_ID, string Instance_ID, string Sys_Message, int Trigger_Status)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_TriggerNAC_InsertLog";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", Module_Code);
                db.AddInParameter(db.cmd, "Item_ID", Item_ID);
                db.AddInParameter(db.cmd, "Transaction_ID", Transaction_ID);
                db.AddInParameter(db.cmd, "Instance_ID", Instance_ID);
                db.AddInParameter(db.cmd, "Sys_Message", Sys_Message);
                db.AddInParameter(db.cmd, "Trigger_Status", Trigger_Status);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch(Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        #region PO Subcon Approval Action
        public dynamic POSubconGetTask(string NAC_Guid, string Form_No)
        {
            string token = GetToken();
            string url = $"{ConfigurationManager.AppSettings["NAC_TASKS_URL"]}&workflowInstanceId={NAC_Guid}";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(HEADERS_AUTHORIZATION, $"Bearer {token}");
            var response = client.GetAsync(url);
            var responseJson = response.Result.Content.ReadAsStringAsync().Result;
            dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            var tasks = responseObject["tasks"];
            dynamic task = null;
            foreach(var t in tasks)
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