using Daikin.BusinessLogics.Common;
//using Daikin.BusinessLogics.Apps.NonCommercials.Model;
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
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        public string SPList = "Non Commercials";

        public List<MasterModuleOptionModel> GetModules(string List_Name)
        {
            List<MasterModuleOptionModel> listOption = new List<MasterModuleOptionModel>();

            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "SPList", List_Name); //Parameter List Name

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                MasterModuleOptionModel data = new MasterModuleOptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    data = new MasterModuleOptionModel();
                    data.Code = row["Code"].ToString();
                    data.Name = row["Name"].ToString();
                    data.List_Name = row["List_Name"].ToString();
                    data.Table_Name = row["Table_Name"].ToString();

                    listOption.Add(data);
                }

                #endregion
                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<OptionModel> GetPendingApproverRoles(string Module_ID)
        {
            //usp_NonCommercials_ListPendingApproverRoles
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NonCommercials_ListPendingApproverRoles";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_ID", Module_ID);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<OptionModel>(dt) : new List<OptionModel>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<OptionModel> GetMasterUserProcDept()
        {
            List<OptionModel> listOption = new List<OptionModel>();
            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterUserProcDept_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                //db.AddInParameter(db.cmd, "SPList", List_Name); //Parameter List Name

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                OptionModel data = new OptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    data = new OptionModel();
                    data.Code = row["Code"].ToString();
                    data.Name = row["Name"].ToString();

                    listOption.Add(data);
                }

                #endregion
                return listOption;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
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

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<GeneralHeaderModel>(dt) : new List<GeneralHeaderModel>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<MasterModuleOptionModel> CreateNewFormGetUrl(int Procurement_Department_ID, string Module_Code)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Procurement_Department_ID", Procurement_Department_ID);
                db.AddInParameter(db.cmd, "SPList", "Non Commercials");


                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<MasterModuleOptionModel>(dt) : new List<MasterModuleOptionModel>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public string GetNonCommVendorBankName(string Bank_Key)
        {
            string Bank_Name = "";
            using(var con = new SqlConnection(Utility.GetSqlConnection()))
            {
                con.Open();
                string query = $"SELECT TOP 1 [Description] FROM MasterBank WHERE Code = '{Bank_Key}'";
                using(SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    Bank_Name = cmd.ExecuteScalar().ToString();
                }
                con.Close();
                return Bank_Name;
            }
        }

        public void NonCommercial_InsertHistoryLog(int Item_ID, string List_Name)
        {
            SPWeb web = SPContext.Current.Web;
            SPList list = web.Lists[List_Name];
            SPListItem item = list.GetItemById(Item_ID);
            string currLogin = "";
            string currLoginName = "";
            Dictionary<string, string> mapping = new Dictionary<string, string>
            {
                {"QCF GA", "Purpose" }, {"QCF MKT", "Requisition_x0020_Purpose" }, {"PURCHASE ORDER", "Requestor_x0020_Notes" }
            };
            string userAccount = item["Submitted_x0020_By"] == null ? "" : item["Submitted_x0020_By"].ToString();
            string comment = "";
            if (!List_Name.ToUpper().Contains("RELEASE"))
            {
                comment = item[mapping[List_Name.ToUpper()]] == null ? "" : item[mapping[List_Name.ToUpper()]].ToString();
            }
            if (!string.IsNullOrEmpty(userAccount))
            {
                currLogin = web.EnsureUser(userAccount).LoginName;
                currLoginName = web.EnsureUser(userAccount).Name;
            }
            new CommonLogic().InsertApprovalLog(List_Name, Item_ID, 1, currLogin, currLoginName, "0", comment);
        }

        public void CustomFormUpdateApprover(int HeaderID, string ListName, string ApproverAccount, string ApproverName, string Comments)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Utility.GetSqlConnection()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@List_Name", ListName);
                        cmd.Parameters.AddWithValue("@Header_ID", HeaderID);
                        cmd.Parameters.AddWithValue("@Comments", Comments);
                        cmd.Parameters.AddWithValue("@Approver_Name", ApproverName);
                        cmd.Parameters.AddWithValue("@Approver_Account", ApproverAccount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetApprovalComment(SPListItem item, string ListName)
        {
            if(item["Comments"] == null)
            {
                return "";
            }
            return item["Comments"] == null ? "" : item["Comments"].ToString();
        }
    }
}