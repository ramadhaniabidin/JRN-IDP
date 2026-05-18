using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daikin.BusinessLogics.Apps.Master.Model;
using Microsoft.SharePoint;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class ListController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        private readonly SharePointManager sp = new SharePointManager();
        private readonly NintexCloudManager ntx = new NintexCloudManager();

        private Common.Model.CurrentApproverModel GetCurrentApprover(string List_Name, int Item_ID)
        {
            var web = SPContext.Current.Web;
            if (List_Name.ToUpper().Contains("CONTRACT"))
            {
                var currentUser = web.CurrentUser;
                return new Common.Model.CurrentApproverModel
                {
                    UserName = currentUser.LoginName, // e.g. domain\jdoe
                    Email = currentUser.Email.ToLowerInvariant(),
                    FullName = currentUser.Name // e.g. John Doe
                };
            }
            SPListItem item = web.Lists[List_Name].GetItemById(Item_ID);
            var key = "Approver_x0020_Login_x0020_Accou";
            if (item[key] != null)
            {
                SPUser targetUser = web.EnsureUser(item[key].ToString());
                return new Common.Model.CurrentApproverModel
                {
                    UserName = targetUser.LoginName,
                    Email = targetUser.Email,
                    FullName = targetUser.Name,
                    CurrentLayer = item["Current Layer"].ToString()
                };
            }
            return null;
        }

        private static int ConvertApprovalValue(string ApprovalValue)
        {
            if (ApprovalValue.ToUpper() == "APPROVE") return 7;
            if (ApprovalValue.ToUpper() == "REVISE") return 5;
            if (ApprovalValue.ToUpper() == "REJECT") return 6;
            return 0;
        }

        public Common.Model.CurrentApproverModel ApproveRequest(string ApprovalValue, string ListName, int ListItemID, int HeaderID, string Comments)
        {
            // 1. Get data and Close DB connection quickly. Don't hold locks during API calls.
            Common.Model.CurrentApproverModel approver = GetCurrentApprover(ListName, ListItemID);
            Common.Model.ListDataID transactionData;

            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                // Just a simple read, no transaction needed here usually
                transactionData = Aprroval.GetListDataIDByHeaderID(ListName, HeaderID, _conn, null);
            }

            // 2. Perform External API Call (Nintex) OUTSIDE of the SQL Transaction.
            // This prevents the 12,000 threads issue if Nintex is slow.
            var task = Aprroval.GetTasks(transactionData.NAC_Guid);
            var assignments = task.Tasks[0].TaskAssignments;
            var targetAssignment = assignments.FirstOrDefault(a =>
                a.Assignee.ToLower().Contains(approver.Email.ToLower()));

            if (targetAssignment == null)
                throw new NullReferenceException("Approver assignment not found in Nintex.");

            var response = ntx.CompleteNACTask(ApprovalValue, task.Tasks[0].Id, targetAssignment.Id);

            if (!response.Success)
            {
                throw new NullReferenceException($"Error completing NAC task: {response.Message}");
            }

            // 3. Now start a SHORT Transaction only for the final Database updates.
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    if (ListName.ToUpper().Contains("CONTRACT"))
                    {
                        CustomFormUpdateApprover(HeaderID, ListName, approver.UserName, approver.FullName, Comments, _conn, trans);
                    }

                    InsertLog(new ClaimReimbursement.Model.InsertHistoryLogModel
                    {
                        List_Name = ListName,
                        Comments = Comments,
                        Action_Id = ConvertApprovalValue(ApprovalValue),
                        Item_ID = ListItemID,
                        Personal_Account = approver.UserName,
                        Personal_Name = approver.FullName
                    }, _conn, trans);

                    // CRITICAL: Finalize the work
                    trans.Commit();
                }
            }

            return approver;
        }

        private void InsertLog(ClaimReimbursement.Model.InsertHistoryLogModel Log, SqlConnection Conn, SqlTransaction Trans)
        {
            using (var cmd = new SqlCommand("usp_NonComm_InsertApprovalLog", Conn, Trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(db.AddInParameter("ListName", Log.List_Name));
                cmd.Parameters.Add(db.AddInParameter("ListItemID", Log.Item_ID));
                cmd.Parameters.Add(db.AddInParameter("Action", Log.Action_Id));
                cmd.Parameters.Add(db.AddInParameter("CurrentLogin", Log.Personal_Account));
                cmd.Parameters.Add(db.AddInParameter("CurrLoginName", Log.Personal_Name));
                cmd.Parameters.Add(db.AddInParameter("Comment", Log.Comments));
                cmd.ExecuteNonQuery();
            }
        }

        public List<MasterModuleOptionModel> GetModules(string List_Name)
        {
            List<MasterModuleOptionModel> listOption = new List<MasterModuleOptionModel>();

            try
            {
                var dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "SPList", List_Name); //Parameter List Name

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new MasterModuleOptionModel
                    {
                        Code = row["Code"].ToString(),
                        Name = row["Name"].ToString(),
                        List_Name = row["List_Name"].ToString(),
                        Table_Name = row["Table_Name"].ToString()
                    });
                }

                #endregion
                return listOption;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<OptionModel> GetPendingApproverRoles(string Module_ID)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NonCommercials_ListPendingApproverRoles";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_ID", Module_ID);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<OptionModel>(dt) : new List<OptionModel>();
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<OptionModel> GetMasterUserProcDept()
        {
            List<OptionModel> listOption = new List<OptionModel>();
            try
            {
                var dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterUserProcDept_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                //db.AddInParameter(db.cmd, "SPList", List_Name); //Parameter List Name

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                OptionModel data = new OptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new OptionModel
                    {
                        Code = row["Code"].ToString(),
                        Name = row["Name"].ToString()
                    });
                }

                #endregion
                return listOption;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            var dt = new DataTable();
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_ListDataNonCommercials";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", model.TableName);
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", model.PageSize);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "ModuleId", model.ModuleId);
                db.AddInParameter(db.cmd, "MIGO", model.MIGO);
                //db.AddInParameter(db.cmd, "PendingApproverRoleID", model.PendingApproverRoleID);
                db.AddInParameter(db.cmd, "PendingApproverRoleName", model.PendingApproverRole);
                db.AddInParameter(db.cmd, "Procurement_Department", model.Procurement_Department);

                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);
                //db.AddOutParameter(db.cmd, "@SQL", SqlDbType.NVarChar);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<GeneralHeaderModel>(dt) : new List<GeneralHeaderModel>();
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public async Task<List<GeneralHeaderModel>> ListDataPORelease(FilterHeaderSearchModel model)
        {
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _conn.OpenAsync();
                using (var cmd = new SqlCommand("GetPOReleaseData_Filtered", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(db.AddInParameter("Branch", model.BranchName));
                    cmd.Parameters.Add(db.AddInParameter("SearchBy", model.SearchBy));
                    cmd.Parameters.Add(db.AddInParameter("Keywords", model.Keywords));
                    cmd.Parameters.Add(db.AddInParameter("FilterDate", model.FilterBy));
                    cmd.Parameters.Add(db.AddInParameter("StartDate", model.StartDate));
                    cmd.Parameters.Add(db.AddInParameter("EndDate", model.EndDate));
                    cmd.Parameters.Add(db.AddInParameter("MIGO", model.MIGO));
                    cmd.Parameters.Add(db.AddInParameter("ApprovalStatus", model.PostingStatus));
                    cmd.Parameters.Add(db.AddInParameter("PageIndex", model.PageIndex));
                    cmd.Parameters.Add(db.AddInParameter("PageSize", model.PageSize));
                    cmd.Parameters.Add(db.AddInParameter("ProcurementDepartment", model.Procurement_Department));
                    cmd.Parameters.Add(db.AddInParameter("PendingApproverRole", model.PendingApproverRole));
                    cmd.Parameters.Add(db.AddInParameter("CurrentUserAccount", model.CurrentLogin));
                    using (var _reader = await cmd.ExecuteReaderAsync())
                    {
                        var _dt = new DataTable();
                        _dt.Load(_reader);
                        return Utility.ConvertDataTableToList<GeneralHeaderModel>(_dt);
                    }
                }
            }
        }

        public List<MasterModuleOptionModel> CreateNewFormGetUrl(int Procurement_Department_ID, string Module_Code)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Procurement_Department_ID", Procurement_Department_ID);
                db.AddInParameter(db.cmd, "SPList", "Non Commercials");


                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<MasterModuleOptionModel>(dt) : new List<MasterModuleOptionModel>();
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public string GetNonCommVendorBankName(string Bank_Key)
        {
            string Bank_Name = "";
            using (var con = new SqlConnection(Utility.GetSqlConnection()))
            {
                con.Open();
                string query = $"SELECT TOP 1 [Description] FROM MasterBank WHERE Code = @bank_key";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "bank_key", Value = Bank_Key, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    Bank_Name = cmd.ExecuteScalar().ToString();
                }
                con.Close();
                return Bank_Name;
            }
        }

        public static void CustomFormUpdateApprover(int HeaderID, string ListName, string ApproverAccount, string ApproverName, string Comments)
        {
            using (SqlConnection conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@List_Name", ListName);
                    cmd.Parameters.AddWithValue("@Header_ID", HeaderID);
                    cmd.Parameters.AddWithValue("@Comments", Comments);
                    cmd.Parameters.AddWithValue("@Approver_Name", ApproverName);
                    cmd.Parameters.AddWithValue("@Approver_Account", ApproverAccount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CustomFormUpdateApprover(int HeaderID, string ListName, string ApproverAccount, string ApproverName, string Comments, SqlConnection Conn, SqlTransaction Trans)
        {
            using (var cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", Conn, Trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@List_Name", ListName);
                cmd.Parameters.AddWithValue("@Header_ID", HeaderID);
                cmd.Parameters.AddWithValue("@Comments", Comments);
                cmd.Parameters.AddWithValue("@Approver_Name", ApproverName);
                cmd.Parameters.AddWithValue("@Approver_Account", ApproverAccount);
                cmd.ExecuteNonQuery();
            }
        }

        public static string GetApprovalComment(SPListItem item, string ListName)
        {
            if (item["Comments"] == null)
            {
                return "";
            }
            return item["Comments"] == null ? "" : item["Comments"].ToString();
        }
    }
}