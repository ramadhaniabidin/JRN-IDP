using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
//using Nintex.Workflow.HumanApproval;
//using Nintex.Workflow.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Master.Controller
{
    public class GeneralController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public void ExecuteNonQuery(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.GetSQLConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<GeneralHistoryLogModel> GetHistoryLog(string Form_No, string ModuleCode, int Transaction_ID)
        {
            try
            {
                string query = ModuleCode == "M011" ? "dbo.usp_Utility_GetHistoryLogByTransId" : "dbo.usp_Utility_GetHistoryLogByTransId_New";
                db.OpenConnection(ref conn);
                db.cmd.CommandText = query;
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Form_No", Form_No);
                db.AddInParameter(db.cmd, "Module_Code", ModuleCode);
                db.AddInParameter(db.cmd, "Transaction_ID", Transaction_ID);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<GeneralHistoryLogModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public int GetTransactionIDByFormNo(string Form_No, string Module_Code)
        {
            try
            {
                int Transaction_ID = 0;
                db.OpenConnection(ref conn);
                string tableName = Module_Code == "M014" ? "ContractHeader" : "POContractHeader";
                db.cmd.CommandText = $"SELECT TOP 1 ID FROM {tableName} WHERE Form_No = @form_no";
                db.cmd.CommandType = CommandType.Text;
                db.AddInParameter(db.cmd, "form_no", Form_No);
                reader = db.cmd.ExecuteReader();
                while (reader.Read())
                {
                    Transaction_ID = reader.GetInt32(0);
                }

                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Transaction_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        
        public List<GeneralHistoryLogModel> GetHistoryLogByTransaction_ID(int Transaction_ID, string Module_Code)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_GetHistoryLogByTransaction_Id";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Transaction_ID", Transaction_ID);
                db.AddInParameter(db.cmd, "Module_Code", Module_Code);

                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<GeneralHistoryLogModel>(dt);


            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //public void Approval(SPWeb web, int spTaskItemID, int outcome)
        //{
        //    SPListCollection lists = web.Lists;
        //    lists.ListsForCurrentUser = true;

        //    SPList taskList = lists.TryGetList("Workflow Tasks");
        //    SPListItem spTask = taskList.GetItemById(spTaskItemID);

        //    SPSecurity.RunWithElevatedPrivileges(delegate ()
        //    {

        //        int decision = outcome;
        //        /*
        //            1	Approve
        //            2	Reject
        //            3	Revision
        //            4	Revise                  
        //         */

        //        NintexTask task = NintexTask.RetrieveTask(spTaskItemID, SPContext.Current.Web, taskList);

        //        Approver approver = task.Approvers.GetBySPId(spTaskItemID);
        //        ConfiguredOutcomeCollection outcomes = approver.AvailableOutcomeInfo.AvailableOutcomes;


        //        Guid commentsFieldId = Nintex.Workflow.Common.NWSharePointObjects.FieldComments;
        //        Guid decisionFieldId = Nintex.Workflow.Common.NWSharePointObjects.FieldDecision;
        //        spTask[decisionFieldId] = decision;
        //        spTask[commentsFieldId] = "";
        //        spTask.Update();
        //    });
        //}

        //public void UpdateDocumentReceived(string SiteUrl, string List_Name, string CurrentLogin, List<int> ItemIDs)
        //{
        //    try
        //    {
        //        int counter = 0;
        //        SPSite site = new SPSite(SiteUrl);
        //        SPWeb web = site.OpenWeb();
        //        SPList list = web.Lists.TryGetList(List_Name);
        //        web.AllowUnsafeUpdates = true;
        //        foreach (int id in ItemIDs)
        //        {
        //            int TaskItemId = 0;
        //            SPSecurity.RunWithElevatedPrivileges(delegate ()
        //            {

        //                SPListItem item = list.GetItemById(id);
        //                item["Document Received"] = true;
        //                item.Update();
        //                string TaskName = item["Title"].ToString();


        //                TaskItemId = new Utility().GetTaskItemId(CurrentLogin, TaskName);
        //            });

        //            Approval(web, TaskItemId, 1);

        //            counter++;
        //        }
        //        web.AllowUnsafeUpdates = false;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}
        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", model.TableName);
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", 10);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "ModuleId", model.ModuleId);
                db.AddInParameter(db.cmd, "Plant_Code", model.Plant_Code);
                db.AddInParameter(db.cmd, "PendingApproverRoleID", model.PendingApproverRoleID);
                db.AddInParameter(db.cmd, "PendingApproverRole", model.PendingApproverRole);

                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

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


        public List<GeneralHeaderModel> SAPBusinssPartner_ListData(FilterHeaderSearchModel model, out int RecordCount)
        {
            try
            {
                using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
                {
                    _conn.Open();
                    using(var cmd = new SqlCommand("usp_SAPBusinessPartner_ListData", _conn))
                    {
                        #region Define parameters
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter {ParameterName = "@TableHeader", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.TableName ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@StartDate", SqlDbType = SqlDbType.Date, Value = model.StartDate, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@EndDate", SqlDbType = SqlDbType.Date, Value = model.EndDate, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@SearchBy", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.SearchBy ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@FilterBy", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.FilterBy ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@Keywords", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.Keywords ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@CurrentLogin", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.CurrentLogin ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@BranchName", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.BranchName ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@ModuleCode", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.ModuleId ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@PendingApproverRoleID", SqlDbType = SqlDbType.Int, Value = model.PendingApproverRoleID, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@Status", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.PostingStatus ?? "", Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@PageIndex", SqlDbType = SqlDbType.Int, Value = model.PageIndex, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", SqlDbType = SqlDbType.Int, Value = model.PageSize, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@RecordCount", SqlDbType = SqlDbType.Int, Value = model.PageSize, Direction = ParameterDirection.Output });
                        #endregion
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            RecordCount = Convert.ToInt32(cmd.Parameters["@RecordCount"].Value);
                            return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<GeneralHeaderModel>(dt) : new List<GeneralHeaderModel>();
                        }
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

    }
}