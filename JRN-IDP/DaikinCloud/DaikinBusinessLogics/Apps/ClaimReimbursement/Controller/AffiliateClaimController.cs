using Daikin.BusinessLogics.Apps.Batch.Controller;
using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Controller
{
    public class AffiliateClaimController
    {
        private readonly SharePointManager sp = new SharePointManager();
        private readonly CommonLogic func = new CommonLogic();
        private readonly BatchController batch = new BatchController();
        private readonly NintexCloudManager ntx = new NintexCloudManager();
        private readonly string serverPath = HttpContext.Current.Server.MapPath("~/Commercials/");
        private readonly string urlSite = SPContext.Current.Web.Url;
        private readonly Type typeString = typeof(string);
        private readonly Type typeInt = typeof(int);
        private readonly Type typeBool = typeof(bool);
        private readonly string connectionString = Utility.GetSqlConnection();
        private readonly string formUrl = "/_layouts/15/Daikin.Application/Modules/ClaimReimbursement/affiliateclaim.aspx?ID=";
        private readonly string MODULE_CODE = "M027";
        private readonly string MODULE_CODE_KEY = "@Module_Code";
        private readonly string ORDER_ID_KEY = "@Order_ID";
        private readonly string BRANCH_KEY = "@Branch";
        private readonly string ITEM_ID_KEY = "@ItemID";
        private readonly string TRANSACTION_ID_KEY = "@Transaction_ID";
        private readonly string TASK_ID_KEY = "@Task_ID";
        private readonly string FORM_NO_KEY = "@Form_No";
        private readonly string HEADER_ID_KEY = "@Header_ID";
        private readonly string COMMENTS_KEY = "@Comments";
        private const string MODULE_NAME = "Affiliate Fully Claim";
        private readonly string MANAGER_EMAIL_KEY = "Manager Email";
        private readonly string MANAGER_NAME_KEY = "Manager Name";
        private readonly string TABLE_HEADER = "AffiliateClaimHeader";

        private List<ApproverRoleModel> GetListApprover(string Module_Code, int Order_ID, string Branch, string Claim_Category)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaim_GetTaskAssignee", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, Module_Code));
                    cmd.Parameters.Add(CreateSQLParam("@Claim_Category", typeString, Claim_Category));
                    cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Order_ID));
                    cmd.Parameters.Add(CreateSQLParam(BRANCH_KEY, typeString, Branch));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<ApproverRoleModel>(dt);
                    }
                }
            }
        }

        private List<ApproverRoleModel> GetListApprover(string Module_Code, int Order_ID, string Branch, string Claim_Category, SqlConnection conn, SqlTransaction trans)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand("usp_AffiliateClaim_GetTaskAssignee", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, Module_Code));
                cmd.Parameters.Add(CreateSQLParam("@Claim_Category", typeString, Claim_Category));
                cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Order_ID));
                cmd.Parameters.Add(CreateSQLParam(BRANCH_KEY, typeString, Branch));
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    return Utility.ConvertDataTableToList<ApproverRoleModel>(dt);
                }
            }
        }

        private List<ApprovalListModel> GetListApprover(string Module_Code, int Item_ID)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaim_GetTaskAssignee", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@ModuleCode", typeString, Module_Code));
                    cmd.Parameters.Add(CreateSQLParam(ITEM_ID_KEY, typeInt, Item_ID));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<ApprovalListModel>(dt);
                    }
                }
            }
        }

        private CustomTaskModel GetPreviousDirectHeadTask(int Order_ID, string Module_Code, int Transaction_ID)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_GetPreviousDirectHeadTask", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Order_ID));
                    cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, Module_Code));
                    cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Transaction_ID));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        var list = Utility.ConvertDataTableToList<CustomTaskModel>(dt);
                        return list.Count > 0 ? list[0] : new CustomTaskModel();
                    }
                }
            }
        }

        private CustomTaskModel GetPreviousDirectHeadTask(int Order_ID, string Module_Code, int Transaction_ID, SqlConnection conn, SqlTransaction trans)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand("usp_GetPreviousDirectHeadTask", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Order_ID));
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, Module_Code));
                cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Transaction_ID));
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    var list = Utility.ConvertDataTableToList<CustomTaskModel>(dt);
                    return list.Count > 0 ? list[0] : new CustomTaskModel();
                }
            }
        }

        private Dictionary<string, object> GetManagerData(int Order_ID, int Header_ID, string User_Email)
        {
            var previousTask = GetPreviousDirectHeadTask(Order_ID, MODULE_CODE, Header_ID);
            string getManagerParam = string.IsNullOrEmpty(previousTask.Assignee_Emails) ? User_Email : previousTask.Assignee_Emails;
            var managerDistinguish = func.GetManagerDistinguishedName(getManagerParam);
            var managerData = func.GetManagerData(managerDistinguish);
            return managerData;
        }

        private Dictionary<string, object> GetManagerData(int Order_ID, int Header_ID, string User_Email, SqlConnection conn, SqlTransaction trans)
        {
            var previousTask = GetPreviousDirectHeadTask(Order_ID, MODULE_CODE, Header_ID, conn, trans);
            string getManagerParam = string.IsNullOrEmpty(previousTask.Assignee_Emails) ? User_Email : previousTask.Assignee_Emails;
            var managerDistinguish = func.GetManagerDistinguishedName(getManagerParam);
            var managerData = func.GetManagerData(managerDistinguish);
            return managerData;
        }

        private ApproverRoleModel GetDirectHeadPIC(AffiliateClaimHeaderModel Header)
        {
            var managerData = GetManagerData(Header.Index_Approver, Header.ID, Header.Requester_Email);
            return new ApproverRoleModel
            {
                Position_ID = 5,
                Position_Name = "Direct Head",
                Order_ID = Header.Index_Approver,
                Module_Code = MODULE_CODE,
                User_Email = managerData[MANAGER_EMAIL_KEY].ToString(),
                User_FullName = managerData[MANAGER_NAME_KEY].ToString()
            };
        }

        private ApproverRoleModel GetDirectHeadPIC(AffiliateClaimHeaderModel Header, SqlConnection conn, SqlTransaction trans)
        {
            var managerData = GetManagerData(Header.Index_Approver, Header.ID, Header.Requester_Email, conn, trans);
            return new ApproverRoleModel
            {
                Position_ID = 5,
                Position_Name = "Direct Head",
                Order_ID = Header.Index_Approver,
                Module_Code = MODULE_CODE,
                User_Email = managerData[MANAGER_EMAIL_KEY].ToString(),
                User_FullName = managerData[MANAGER_NAME_KEY].ToString()
            };
        }


        private ApproverRoleModel GetOtherRolePIC(List<ApproverRoleModel> List_Approver, int Order_ID)
        {
            string names = List_Approver[0].User_FullName;
            string emails = List_Approver[0].User_Email;
            if (List_Approver.Count > 1)
            {
                for (int i = 1; i < List_Approver.Count; i++)
                {
                    var app = List_Approver[i];
                    names += $";{app.User_FullName}";
                    emails += $";{app.User_Email};";
                }
            }
            return new ApproverRoleModel
            {
                Position_ID = List_Approver[0].Position_ID,
                Position_Name = List_Approver[0].Position_Name,
                Order_ID = Order_ID,
                Module_Code = MODULE_CODE,
                User_Email = emails,
                User_FullName = names
            };
        }

        private ApproverRoleModel GetCurrentApprover(AffiliateClaimHeaderModel Header)
        {
            var listApprover = GetListApprover(MODULE_CODE, Header.Index_Approver, Header.Business_Area, Header.Category);
            int positionID = listApprover[0].Position_ID;
            return positionID == 5 ? GetDirectHeadPIC(Header) : GetOtherRolePIC(listApprover, Header.Index_Approver);
        }

        private ApproverRoleModel GetCurrentApprover(AffiliateClaimHeaderModel Header, SqlConnection conn, SqlTransaction trans)
        {
            var listApprover = GetListApprover(MODULE_CODE, Header.Index_Approver, Header.Business_Area, Header.Category, conn, trans);
            int positionID = listApprover[0].Position_ID;
            return positionID == 5 ? GetDirectHeadPIC(Header, conn, trans) : GetOtherRolePIC(listApprover, Header.Index_Approver);
        }

        public void UpdatePendingApprover(ApproverRoleModel Current_Approver, AffiliateClaimHeaderModel Header)
        {
            if (Header.Requester_Email.ToUpperInvariant().Contains("TEST1"))
            {
                Current_Approver.User_FullName = "Approver1";
                Current_Approver.User_Email = "Approver1@daikin.co.id";
            }
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaim_SetApprover", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Current_Index_Approver", typeInt, Header.Index_Approver));
                    cmd.Parameters.Add(CreateSQLParam("@Pending_Approver_Email", typeString, Current_Approver.User_Email));
                    cmd.Parameters.Add(CreateSQLParam("@Pending_Approver_Name", typeString, Current_Approver.User_FullName));
                    cmd.Parameters.Add(CreateSQLParam("@Pending_Approver_Role", typeString, Current_Approver.Position_Name));
                    cmd.Parameters.Add(CreateSQLParam("@Pending_Approver_Role_ID", typeInt, Current_Approver.Position_ID));
                    cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header.ID));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void StartApproval(string Module_Code, string Form_No)
        {
            var header = GetHeaderData(Form_No);
            var currentApprover = GetCurrentApprover(header);
            UpdatePendingApprover(currentApprover, header);
        }

        private void UpdateFlaggingApprovalQueue(string Task_ID, SqlConnection con, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_ApprovalQueue_UpdateFlag", con, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter { ParameterName = TASK_ID_KEY, Value = Task_ID, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar });
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertApprovalQueue(AffiliateClaimHeaderModel Header)
        {
            var currentApprover = GetCurrentApprover(Header);
            var taskDescription = $"{MODULE_NAME} - Item ID: {Header.Item_ID} - {Header.Form_No} - {currentApprover.Position_Name}";
            var taskUrl = $"http://spdev:3473{formUrl}{Header.Form_No}";
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_InsertApprovalQueue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(TASK_ID_KEY, typeString, Guid.NewGuid().ToString()));
                    cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, "M027"));
                    cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Header.ID));
                    cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeString, Header.Form_No));
                    cmd.Parameters.Add(CreateSQLParam("@Task_Description", typeString, taskDescription));
                    cmd.Parameters.Add(CreateSQLParam("@Assignee_Names", typeString, currentApprover.User_FullName));
                    cmd.Parameters.Add(CreateSQLParam("@Assignee_Emails", typeString, currentApprover.User_Email));
                    cmd.Parameters.Add(CreateSQLParam("@Assignee_Role", typeString, currentApprover.Position_Name));
                    cmd.Parameters.Add(CreateSQLParam("@Assignee_Role_ID", typeInt, currentApprover.Position_ID));
                    cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Header.Index_Approver));
                    cmd.Parameters.Add(CreateSQLParam("@Task_Url", typeString, taskUrl));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertApprovalQueue(AffiliateClaimHeaderModel Header, SqlConnection conn, SqlTransaction trans)
        {
            var currentApprover = GetCurrentApprover(Header, conn, trans);
            var taskDescription = $"{MODULE_NAME} - Item ID: {Header.Item_ID} - {Header.Form_No} - {currentApprover.Position_Name}";
            var taskUrl = $"http://spdev:3473{formUrl}{Header.Form_No}";
            using (var cmd = new SqlCommand("usp_InsertApprovalQueue", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(TASK_ID_KEY, typeString, Guid.NewGuid().ToString()));
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, "M027"));
                cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Header.ID));
                cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeString, Header.Form_No));
                cmd.Parameters.Add(CreateSQLParam("@Task_Description", typeString, taskDescription));
                cmd.Parameters.Add(CreateSQLParam("@Assignee_Names", typeString, currentApprover.User_FullName));
                cmd.Parameters.Add(CreateSQLParam("@Assignee_Emails", typeString, currentApprover.User_Email));
                cmd.Parameters.Add(CreateSQLParam("@Assignee_Role", typeString, currentApprover.Position_Name));
                cmd.Parameters.Add(CreateSQLParam("@Assignee_Role_ID", typeInt, currentApprover.Position_ID));
                cmd.Parameters.Add(CreateSQLParam(ORDER_ID_KEY, typeInt, Header.Index_Approver));
                cmd.Parameters.Add(CreateSQLParam("@Task_Url", typeString, taskUrl));
                cmd.ExecuteNonQuery();
            }
        }


        private int GetTotalApprovalLayer(string Module_Code, int Item_ID)
        {
            int totalLayer = 0;
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_NWC_GetApproverList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(ITEM_ID_KEY, typeInt, Item_ID));
                    cmd.Parameters.Add(CreateSQLParam("@ModuleCode", typeString, Module_Code));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            totalLayer++;
                        }
                    }
                }
            }
            return totalLayer;
        }

        public void UpdateTaskResponder(int Header_ID, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@List_Name", MODULE_NAME);
                cmd.Parameters.AddWithValue(HEADER_ID_KEY, Header_ID);
                cmd.Parameters.AddWithValue(COMMENTS_KEY, Action.Comment);
                cmd.Parameters.AddWithValue("@Approver_Name", Action.Approver_Name);
                cmd.Parameters.AddWithValue("@Approver_Account", Action.Approver_Account);
                cmd.Parameters.AddWithValue("@Approver_Email", Action.Approver_Email);
                cmd.ExecuteNonQuery();
            }
        }

        public async Task UpdateTaskResponderAsync(int Header_ID, TaskActionModel Action, SqlConnection Conn, SqlTransaction Trans)
        {
            using (SqlCommand cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", Conn, Trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@List_Name", MODULE_NAME);
                cmd.Parameters.AddWithValue(HEADER_ID_KEY, Header_ID);
                cmd.Parameters.AddWithValue(COMMENTS_KEY, Action.Comment);
                cmd.Parameters.AddWithValue("@Approver_Name", Action.Approver_Name);
                cmd.Parameters.AddWithValue("@Approver_Account", Action.Approver_Account);
                cmd.Parameters.AddWithValue("@Approver_Email", Action.Approver_Email);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public CommonResponseModel ExecuteApprovalAction_NAC(AffiliateClaimHeaderModel Header, TaskActionModel Action, VendorBankModel Vendor_Bank, List<AffiliateClaimDetail> Details)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    UpdateTaskResponder(Header.ID, Action, conn, trans);
                    if (Header.Pending_Approver_Role_ID == 1)        // Receiver Document
                    {
                        UpdateDocumentReceived(Header, Action.IsDocumentReceived, conn, trans);
                    }
                    if (Header.Pending_Approver_Role_ID == 48)       // Verifier Document
                    {
                        UpdatePartnerBankID(Header.ID, Vendor_Bank, conn, trans);
                    }
                    if (Header.Pending_Approver_Role_ID == 15)       // Tax verifier
                    {
                        UpdateWHT(Header.ID, Details, conn, trans);
                    }
                    var taskAssignmentResponse = ntx.GetTaskAssignment(Header.Task_ID, Header.Form_No);
                    var response = NintexCloudManager.ProcessNACTask(taskAssignmentResponse.TaskAssignments, Action.Approver_Email, Action.Action_Name);
                    trans.Commit();
                    return response;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task<CommonResponseModel> ExecuteApprovalAction_NACAsync(AffiliateClaimHeaderModel Header, TaskActionModel Action, VendorBankModel Vendor_Bank, List<AffiliateClaimDetail> Details)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (SqlTransaction _trans = _conn.BeginTransaction())
                {
                    var taskAssignmentResponse = await ntx.GetTaskAssignmentAsync(Header.Task_ID, Header.Form_No).ConfigureAwait(false);
                    await UpdateTaskResponderAsync(Header.ID, Action, _conn, _trans).ConfigureAwait(false);
                    await SwitchApprovalAction(Header, Action, Vendor_Bank, Details, _conn, _trans).ConfigureAwait(false);
                    var response = await NintexCloudManager.ProcessNACTaskAsync(taskAssignmentResponse.TaskAssignments, Action.Approver_Email, Action.Action_Name).ConfigureAwait(false);
                    if (response.Success) _trans.Commit();
                    return response;
                }
            }
        }

        private async Task SwitchApprovalAction(AffiliateClaimHeaderModel Header, TaskActionModel Action, VendorBankModel Vendor_Bank, List<AffiliateClaimDetail> Details, SqlConnection Conn, SqlTransaction Trans)
        {
            if (Header.Pending_Approver_Role_ID == 1) await UpdateDocumentReceivedAsync(Header, Action.IsDocumentReceived, Conn, Trans).ConfigureAwait(false);
            if (Header.Pending_Approver_Role_ID == 48) await UpdatePartnerBankIDAsync(Header.ID, Vendor_Bank, Conn, Trans).ConfigureAwait(false);
            if (Header.Pending_Approver_Role_ID == 15) await UpdateWHTAsync(Header.ID, Details, Conn, Trans).ConfigureAwait(false);
        }

        public void ExecuteApprovalAction(AffiliateClaimHeaderModel Header, TaskActionModel Action, VendorBankModel Vendor_Bank, List<AffiliateClaimDetail> Details)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    InsertHistoryLog(Header, Action, conn, trans);
                    ApprovalAction(Header, Action, conn, trans);
                    CompleteTaskApproval(Action.Approver_Email, Action.Action_Name, Action.Comment, Header.Task_ID, conn, trans);
                    if (Header.Pending_Approver_Role_ID == 1)        // Receiver Document
                    {
                        UpdateDocumentReceived(Header, Action.IsDocumentReceived, conn, trans);
                    }
                    if (Header.Pending_Approver_Role_ID == 48)       // Verifier Document
                    {
                        UpdatePartnerBankID(Header.ID, Vendor_Bank, conn, trans);
                    }
                    if (Header.Pending_Approver_Role_ID == 15)       // Tax verifier
                    {
                        UpdateWHT(Header.ID, Details, conn, trans);
                    }
                    if (Action.Action_ID == 7 && (Header.Index_Approver + 1) <= Header.Total_Layer_Approval)
                    {
                        Header.Index_Approver++;
                        InsertApprovalQueue(Header, conn, trans);
                    }
                    else if (Action.Action_ID == 7 && (Header.Index_Approver + 1) > Header.Total_Layer_Approval)
                    {
                        FinalizeApproval(Header.Item_ID, conn, trans);
                        UpdateSPListStatus(Header.Item_ID);
                        CreateBatchFile(Header.Form_No, Header.ID, conn, trans);
                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }



        private void FinalizeApproval(int Item_ID, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_NWC_completeApproval", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(ITEM_ID_KEY, typeInt, Item_ID));
                cmd.Parameters.Add(CreateSQLParam("@TableHeader", typeString, TABLE_HEADER));
                cmd.ExecuteNonQuery();
            }
        }

        private int GetBatchFileFolderID(SqlConnection conn, SqlTransaction trans)
        {
            string query = "SELECT TOP 1 CAST(ID AS INT) AS ID FROM MasterSAPFolderLocation WHERE Module_Code = @Module_Code ORDER BY ID DESC";
            int folderID = -1;
            using (var cmd = new SqlCommand(query, conn, trans))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, MODULE_CODE));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        folderID = reader.GetInt32(reader.GetOrdinal("ID"));
                    }
                    return folderID;
                }
            }
        }

        private void CreateBatchFile(string Form_No, int Header_ID, SqlConnection conn, SqlTransaction trans)
        {
            int folderID = GetBatchFileFolderID(conn, trans);
            batch.CreateBatchFileDynamic_V2("SAP.[usp_Utility_CreateBatchFile]", folderID.ToString(), Header_ID, Form_No, conn, trans);
        }

        private void UpdateSPListStatus(int Item_ID)
        {
            SPWeb web = new SPSite(urlSite).OpenWeb();
            SPList list = web.Lists[MODULE_NAME];
            web.AllowUnsafeUpdates = true;
            SPListItem item = list.GetItemById(Item_ID);
            item["Approval Status"] = "Approved";
            item["Approval Status ID"] = 7;
            item["Workflow Status"] = "Completed";
            item.Update();
        }

        private void CompleteTaskApproval(string Responder, string Outcome, string Comment, string Task_ID, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_CompleteTaskApproval", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@Responder_Email", typeString, Responder));
                cmd.Parameters.Add(CreateSQLParam("@Outcome", typeString, Outcome));
                cmd.Parameters.Add(CreateSQLParam("@Comment", typeString, Comment));
                cmd.Parameters.Add(CreateSQLParam(TASK_ID_KEY, typeString, Task_ID));
                cmd.ExecuteNonQuery();
            }
        }

        private void ApprovalAction(AffiliateClaimHeaderModel Header, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("[usp_NWC_ApprovalAction]", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(ITEM_ID_KEY, typeInt, Header.Item_ID));
                cmd.Parameters.Add(CreateSQLParam("@TableHeader", typeString, "AffiliateClaimHeader"));
                cmd.Parameters.Add(CreateSQLParam("@ApprovalIndex", typeInt, Header.Index_Approver));
                cmd.Parameters.Add(CreateSQLParam("@ApprovalAction", typeString, Action.Action_Name));
                cmd.Parameters.Add(CreateSQLParam("@ApproverName", typeString, Action.Approver_Name));
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateDocumentReceived(AffiliateClaimHeaderModel Header, bool IsDocumentReceived, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_AffiliateClaim_UpdateDocumentReceived", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header.ID));
                cmd.Parameters.Add(CreateSQLParam("@IsReceived", typeBool, IsDocumentReceived));
                cmd.ExecuteNonQuery();
            }
        }

        private async Task UpdateDocumentReceivedAsync(AffiliateClaimHeaderModel Header, bool IsDocumentReceived, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_AffiliateClaim_UpdateDocumentReceived", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header.ID));
                cmd.Parameters.Add(CreateSQLParam("@IsReceived", typeBool, IsDocumentReceived));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void UpdatePartnerBankID(int Header_ID, VendorBankModel Vendor_Bank, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_AffiliateClaim_UpdateVendorBank", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@Bank_Name", typeString, Vendor_Bank.Bank_Name));
                cmd.Parameters.Add(CreateSQLParam("@Partner_Bank", typeString, Vendor_Bank.Partner_Bank));
                cmd.Parameters.Add(CreateSQLParam("@Account_No", typeString, Vendor_Bank.Bank_Account_No));
                cmd.Parameters.Add(CreateSQLParam("@Account_Name", typeString, Vendor_Bank.Bank_Account_Name));
                cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header_ID));
                cmd.ExecuteNonQuery();
            }
        }

        private async Task UpdatePartnerBankIDAsync(int Header_ID, VendorBankModel Vendor_Bank, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_AffiliateClaim_UpdateVendorBank", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@Bank_Name", typeString, Vendor_Bank.Bank_Name));
                cmd.Parameters.Add(CreateSQLParam("@Partner_Bank", typeString, Vendor_Bank.Partner_Bank));
                cmd.Parameters.Add(CreateSQLParam("@Account_No", typeString, Vendor_Bank.Bank_Account_No));
                cmd.Parameters.Add(CreateSQLParam("@Account_Name", typeString, Vendor_Bank.Bank_Account_Name));
                cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header_ID));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void UpdateWHT(int Header_ID, List<AffiliateClaimDetail> Details, SqlConnection conn, SqlTransaction trans)
        {
            DataTable dTable = new DataTable();
            dTable.Columns.Add("Header_ID", typeof(int));
            dTable.Columns.Add("Customer_Type", typeof(string));
            dTable.Columns.Add("Customer_No", typeof(string));
            dTable.Columns.Add("Tax_Base", typeof(decimal));
            dTable.Columns.Add("VAT", typeof(string));
            dTable.Columns.Add("GL", typeof(string));
            dTable.Columns.Add("VAT_Amount", typeof(decimal));
            dTable.Columns.Add("Tax_Invoice_No", typeof(string));
            dTable.Columns.Add("Description", typeof(string));
            dTable.Columns.Add("WHT_Description", typeof(string));
            dTable.Columns.Add("WHT_Amount", typeof(decimal));
            dTable.Columns.Add("Total_Amount", typeof(decimal));
            foreach (var d in Details)
            {
                dTable.Rows.Add(Header_ID, d.Customer_Name, d.Customer_No,
                    d.Tax_Base, d.Tax_Code, "", d.VAT_Amount, d.Tax_Invoice_Number,
                    d.Texting, d.WHT_Type, d.WHT_Amount, d.Total_Amount);

            }
            using (SqlCommand cmd = new SqlCommand("[usp_AffiliateClaimDetailSaveWHT]", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Details", dTable);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimDetailType";
                cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeInt, Header_ID));
                cmd.ExecuteNonQuery();
            }
        }

        private async Task UpdateWHTAsync(int Header_ID, List<AffiliateClaimDetail> Details, SqlConnection Conn, SqlTransaction Trans)
        {
            DataTable t = new DataTable();
            t.Columns.Add("Header_ID", typeof(int));
            t.Columns.Add("Customer_Type", typeof(string));
            t.Columns.Add("Customer_No", typeof(string));
            t.Columns.Add("Tax_Base", typeof(decimal));
            t.Columns.Add("VAT", typeof(string));
            t.Columns.Add("GL", typeof(string));
            t.Columns.Add("VAT_Amount", typeof(decimal));
            t.Columns.Add("Tax_Invoice_No", typeof(string));
            t.Columns.Add("Description", typeof(string));
            t.Columns.Add("WHT_Description", typeof(string));
            t.Columns.Add("WHT_Amount", typeof(decimal));
            t.Columns.Add("Total_Amount", typeof(decimal));

            foreach (var d in Details)
            {
                t.Rows.Add(Header_ID, d.Customer_Name, d.Customer_No,
                    d.Tax_Base, d.Tax_Code, "", d.VAT_Amount, d.Tax_Invoice_Number,
                    d.Texting, d.WHT_Type, d.WHT_Amount, d.Total_Amount);
            }

            using (SqlCommand cmd = new SqlCommand("usp_AffiliateClaimDetailSaveWHT", Conn, Trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Details", t);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimDetailType";
                cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeInt, Header_ID));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void InsertHistoryLog(AffiliateClaimHeaderModel Header, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_InsertApprovalLog", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, "M027"));
                cmd.Parameters.Add(CreateSQLParam("@Module_Name", typeString, MODULE_NAME));
                cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Header.ID));
                cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeString, Header.Form_No));
                cmd.Parameters.Add(CreateSQLParam(BRANCH_KEY, typeString, Header.Business_Area));
                cmd.Parameters.Add(CreateSQLParam("@Personal_Name", typeString, Action.Approver_Name));
                cmd.Parameters.Add(CreateSQLParam("@Personal_Account", typeString, Action.Approver_Account));
                cmd.Parameters.Add(CreateSQLParam("@Position", typeString, Action.Approver_Role));
                cmd.Parameters.Add(CreateSQLParam(COMMENTS_KEY, typeString, Action.Comment));
                cmd.Parameters.Add(CreateSQLParam("@Action", typeInt, Action.Action_ID));
                cmd.ExecuteNonQuery();
            }
        }

        private async Task InsertHistoryLogAsync(AffiliateClaimHeaderModel Header, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_InsertApprovalLog", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(MODULE_CODE_KEY, typeString, "M027"));
                cmd.Parameters.Add(CreateSQLParam("@Module_Name", typeString, MODULE_NAME));
                cmd.Parameters.Add(CreateSQLParam(TRANSACTION_ID_KEY, typeInt, Header.ID));
                cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeString, Header.Form_No));
                cmd.Parameters.Add(CreateSQLParam(BRANCH_KEY, typeString, Header.Business_Area));
                cmd.Parameters.Add(CreateSQLParam("@Personal_Name", typeString, Action.Approver_Name));
                cmd.Parameters.Add(CreateSQLParam("@Personal_Account", typeString, Action.Approver_Account));
                cmd.Parameters.Add(CreateSQLParam("@Position", typeString, Action.Approver_Role));
                cmd.Parameters.Add(CreateSQLParam(COMMENTS_KEY, typeString, Action.Comment));
                cmd.Parameters.Add(CreateSQLParam("@Action", typeInt, Action.Action_ID));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public AffiliateClaimHeaderModel GetHeaderData(string Form_No)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaimHeader_GetData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeof(string), Form_No));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<AffiliateClaimHeaderModel>(dt)[0];
                    }
                }
            }
        }

        public async Task<AffiliateClaimHeaderModel> GetHeaderDataAsync(string Form_No)
        {
            using (var _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (var cmd = new SqlCommand("usp_AffiliateClaimHeader_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeString, Form_No));
                    using (var _r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        var listHeader = await Utility.MapReaderToList<AffiliateClaimHeaderModel>(_r).ConfigureAwait(false);
                        if (listHeader != null && listHeader.Count > 0)
                        {
                            return listHeader[0];
                        }
                        return null;
                    }
                }
            }
        }

        public List<AffiliateClaimDetail> GetDetailData(int Header_ID)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaimDetail_GetData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeof(int), Header_ID));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<AffiliateClaimDetail>(dt);
                    }
                }
            }
        }

        public async Task<List<AffiliateClaimDetail>> GetDetailDataAsync(int Header_ID)
        {
            using (var _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (var cmd = new SqlCommand("usp_AffiliateClaimDetail_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeInt, Header_ID));
                    using (var _r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return await Utility.MapReaderToList<AffiliateClaimDetail>(_r).ConfigureAwait(false);
                    }
                }
            }
        }

        public List<AffiliateClaimAttachment> GetAttachmentsData(int Header_ID)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_AffiliateClaimAttachment_GetData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeof(int), Header_ID));
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<AffiliateClaimAttachment>(dt);
                    }
                }
            }
        }

        public async Task<List<AffiliateClaimAttachment>> GetAttachmentsDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateClaimAttachment_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam(HEADER_ID_KEY, typeInt, Header_ID));
                    using (SqlDataReader _r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return await Utility.MapReaderToList<AffiliateClaimAttachment>(_r).ConfigureAwait(false);
                    }
                }
            }
        }

        private void TriggerNAC(string Module_Code, int Item_ID, int Transaction_ID, string List_Name)
        {
            Task.Run(async () =>
            {
                var nwc = ntx.GenerateNACPayload(Transaction_ID, Item_ID, Module_Code, List_Name);
                await ntx.StartNWC(nwc).ConfigureAwait(false);
            }).Wait();
        }

        public void Save(AffiliateClaimHeaderModel header, List<AffiliateClaimDetail> details, List<AffiliateClaimAttachment> attachments)
        {
            header.Item_ID = InsertToSPList(header);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int Header_ID = InsertHeader(conn, trans, header);
                    InsertDetails(conn, trans, details, Header_ID);
                    InsertAttachments(conn, trans, attachments, Header_ID, header.Item_ID);
                    trans.Commit();
                    if (header.Approval_Status == 5)
                    {
                        TriggerNAC(MODULE_CODE, header.Item_ID, header.ID, MODULE_NAME);
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        private void InsertDetails(SqlConnection conn, SqlTransaction trans, List<AffiliateClaimDetail> details, int Header_ID)
        {
            DataTable dTable = new DataTable();
            dTable.Columns.Add("Header_ID", typeof(int));
            dTable.Columns.Add("Customer_Type", typeof(string));
            dTable.Columns.Add("Customer_No", typeof(string));
            dTable.Columns.Add("Tax_Base", typeof(decimal));
            dTable.Columns.Add("VAT", typeof(string));
            dTable.Columns.Add("GL", typeof(string));
            dTable.Columns.Add("VAT_Amount", typeof(decimal));
            dTable.Columns.Add("Tax_Invoice_No", typeof(string));
            dTable.Columns.Add("Description", typeof(string));
            dTable.Columns.Add("WHT_Description", typeof(string));
            dTable.Columns.Add("WHT_Amount", typeof(decimal));
            dTable.Columns.Add("Total_Amount", typeof(decimal));
            foreach (var d in details)
            {
                dTable.Rows.Add(Header_ID, d.Customer_Name, d.Customer_No,
                    d.Tax_Base, d.Tax_Code, "", d.VAT_Amount, d.Tax_Invoice_Number,
                    d.Texting, d.WHT_Type, d.WHT_Amount, d.Total_Amount);

            }
            using (SqlCommand cmd = new SqlCommand("[usp_AffiliateClaimDetailSave]", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Details", dTable);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimDetailType";
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertAttachments(SqlConnection conn, SqlTransaction trans, List<AffiliateClaimAttachment> attachments, int Header_ID, int Item_ID)
        {
            DataTable dTable = new DataTable();
            dTable.Columns.Add("Header_ID", typeof(int));
            dTable.Columns.Add("Doc_Type", typeof(string));
            dTable.Columns.Add("Is_Mandatory", typeof(int));
            dTable.Columns.Add("Attachment_Url", typeof(string));
            dTable.Columns.Add("Attachment_Name", typeof(string));
            foreach (var att in attachments)
            {
                if (!string.IsNullOrEmpty(att.Attachment_Name))
                {
                    string attachment_url = $"/Lists/{MODULE_NAME}/Attachments/{Item_ID}/{att.Attachment_Name}";
                    dTable.Rows.Add(Header_ID, att.Doc_Type, att.Is_Mandatory, attachment_url, att.Attachment_Name);
                    sp.UploadFileInCustomList(MODULE_NAME, Item_ID, Path.Combine(serverPath, att.Attachment_Name), urlSite);
                }
            }
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateClaimSaveAttachment", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Attachments", dTable);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimAttachmentType";
                cmd.ExecuteNonQuery();
            }
        }

        private SqlParameter CreateSQLParam(string key, Type type, object value)
        {
            SqlParameter param = new SqlParameter { ParameterName = key, Value = value, Direction = ParameterDirection.Input };
            if (type == typeof(string))
            {
                param.SqlDbType = SqlDbType.VarChar;
            }
            else if (type == typeof(int))
            {
                param.SqlDbType = SqlDbType.Int;
            }
            else if (type == typeof(decimal))
            {
                param.SqlDbType = SqlDbType.Decimal;
            }
            else if (type == typeof(DateTime))
            {
                param.SqlDbType = SqlDbType.DateTime;
            }
            else if (type == typeof(bool))
            {
                param.SqlDbType = SqlDbType.Bit;
            }
            return param;
        }

        private int InsertHeader(SqlConnection conn, SqlTransaction trans, AffiliateClaimHeaderModel header)
        {
            string query = "usp_AffiliateClaimHeaderSaveUpdate";
            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam(FORM_NO_KEY, typeof(string), header.Form_No));
                cmd.Parameters.Add(CreateSQLParam("@Item_ID", typeof(int), header.Item_ID));
                cmd.Parameters.Add(CreateSQLParam("@Request_Date", typeof(DateTime), Convert.ToDateTime(header.Request_Date)));
                cmd.Parameters.Add(CreateSQLParam(BRANCH_KEY, typeof(string), header.Branch));
                cmd.Parameters.Add(CreateSQLParam("@Department", typeof(string), header.Department));
                cmd.Parameters.Add(CreateSQLParam("@Requester_Account", typeof(string), header.Requester_Account));
                cmd.Parameters.Add(CreateSQLParam("@Requester_Email", typeof(string), header.Requester_Email));
                cmd.Parameters.Add(CreateSQLParam("@Category", typeof(string), header.Category));
                cmd.Parameters.Add(CreateSQLParam("@Document_Date", typeof(DateTime), Convert.ToDateTime(header.Document_Date)));
                cmd.Parameters.Add(CreateSQLParam("@Vendor", typeof(string), header.Vendor));
                cmd.Parameters.Add(CreateSQLParam("@Vendor_Invoice_No", typeof(string), header.Vendor_Invoice_No));
                cmd.Parameters.Add(CreateSQLParam("@Assignment", typeof(string), header.Assignment));
                cmd.Parameters.Add(CreateSQLParam("@Claim_Type", typeof(string), header.Claim_Type));
                cmd.Parameters.Add(CreateSQLParam("@Total_Tax", typeof(decimal), header.Total_Tax));
                cmd.Parameters.Add(CreateSQLParam("@Total_VAT", typeof(decimal), header.Total_VAT));
                cmd.Parameters.Add(CreateSQLParam("@Total_WHT", typeof(decimal), header.Total_WHT));
                cmd.Parameters.Add(CreateSQLParam("@Grand_Total", typeof(decimal), header.Grand_Total));
                cmd.Parameters.Add(CreateSQLParam("@Requester_Name", typeof(string), header.Requester_Name));
                cmd.Parameters.Add(CreateSQLParam("@Business_Area", typeof(string), header.Business_Area));
                cmd.Parameters.Add(CreateSQLParam("@Cost_Center", typeof(string), header.Cost_Center));
                cmd.Parameters.Add(CreateSQLParam("@Requester_Business_Area", typeof(string), header.Requester_Business_Area));
                cmd.Parameters.Add(CreateSQLParam("@Vendor_Code", typeof(string), header.Vendor_Code));
                cmd.Parameters.Add(CreateSQLParam("@Vendor_Name", typeof(string), header.Vendor_Name));
                cmd.Parameters.Add(CreateSQLParam("@Recon_Account", typeof(string), header.Recon_Account));
                cmd.Parameters.Add(CreateSQLParam("@Currency", typeString, header.Currency));
                SqlParameter outId = new SqlParameter("@OutID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outId);
                cmd.ExecuteNonQuery();
                return (int)outId.Value;
            }
        }

        private int InsertToSPList(AffiliateClaimHeaderModel header)
        {
            int Item_ID = Convert.ToInt32(header.Item_ID);
            SPWeb web = new SPSite(urlSite).OpenWeb();
            SPList list = web.Lists[MODULE_NAME];
            web.AllowUnsafeUpdates = true;
            SPListItem item = Item_ID == 0 ? list.Items.Add() : list.GetItemById(Item_ID);
            item["Title"] = header.Form_No;
            item["Request Date"] = Convert.ToDateTime(header.Request_Date);
            item["Requester Branch"] = header.Branch;
            item["Requester Department"] = header.Department;
            item["Requester Account"] = header.Requester_Account;
            item["Requester Email"] = header.Requester_Email;
            item["Claim Category"] = header.Category;
            item["Document Date"] = Convert.ToDateTime(header.Document_Date);
            item["Vendor"] = header.Vendor;
            item["Vendor Invoice No"] = header.Vendor_Invoice_No;
            item["Assignment"] = header.Assignment;
            item["Claim Type"] = header.Claim_Type;
            item["Total Tax"] = header.Total_Tax;
            item["Total VAT"] = header.Total_VAT;
            item["Total WHT"] = header.Total_WHT;
            item["Grand Total"] = header.Grand_Total;
            item["Form Status"] = header.Form_Status;
            item["Approval Status"] = header.Approval_Status_Name;
            item["Approval Status ID"] = header.Approval_Status;
            item["Requester Name"] = header.Requester_Name;
            item["Requester Business Area"] = header.Requester_Business_Area;
            item["Cost Center"] = header.Cost_Center;
            item["Business Area"] = header.Business_Area_Item_ID;
            item["Vendor Code"] = header.Vendor_Code;
            item["Vendor Name"] = header.Vendor_Name;
            item["Recon Account"] = header.Recon_Account;
            item["Transaction ID"] = 0;
            item.Update();
            return item.ID;
        }

        private byte[] DownloadFileFromUrl(string fileUrl)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(fileUrl);
            }
        }

    }
}
