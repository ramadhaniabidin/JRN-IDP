using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Controller
{
    public class ClaimReimbursementController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ClaimReimbursement_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", model.TableName);
                db.AddInParameter(db.cmd, "PageIndex",Convert.ToInt32(model.PageIndex));
                db.AddInParameter(db.cmd, "PageSize", 10);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "PendingApproverRoleID", Convert.ToInt32 (model.PendingApproverRoleID));
                db.AddInParameter(db.cmd, "PendingApproverRole", model.PendingApproverRole);
                //db.AddInParameter(db.cmd, "LIstName", model.ListName);


                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<GeneralHeaderModel>(dt) : new List<GeneralHeaderModel>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<GeneralHeaderModel> ListDataAffilite(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ClaimReimbursement_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", "AffiliateClaimHeader");
                db.AddInParameter(db.cmd, "PageIndex", Convert.ToInt32(model.PageIndex));
                db.AddInParameter(db.cmd, "PageSize", 10);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "PendingApproverRoleID", Convert.ToInt32(model.PendingApproverRoleID));
                db.AddInParameter(db.cmd, "PendingApproverRole", model.PendingApproverRole);
                db.AddInParameter(db.cmd, "LIstName", model.ListName);
                //db.AddInParameter(db.cmd, "")


                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<GeneralHeaderModel>(dt) : new List<GeneralHeaderModel>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public List<GeneralHistoryLogModel> GetHistoryLog(string Form_No, string ModuleCode)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_GetHistoryLogByTransId";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Form_No", Form_No);
                db.AddInParameter(db.cmd, "Module_Code", ModuleCode);

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

        public List<MasterModuleOptionModel> ModuleOptions(string SPList)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "SPList", SPList);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<MasterModuleOptionModel>(dt);
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


        public List<OptionModel> GetOptions(string Table, string Code, string Name, string FilterBy, string FilterValue, string Extra)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Table", Table);
                db.AddInParameter(db.cmd, "Code", Code);
                db.AddInParameter(db.cmd, "Name", Name);
                db.AddInParameter(db.cmd, "FilterBy", FilterBy);
                db.AddInParameter(db.cmd, "FilterValue", FilterValue);
                db.AddInParameter(db.cmd, "Extra", Extra);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<OptionModel>(dt);
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
        public List<OptionModel> GetMasterRoleApproverCR()
        {
            try
            {
                List<OptionModel> listOption = new List<OptionModel>();
                dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterRoleApproverCR_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                OptionModel data = new OptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    data = new OptionModel();

                    data.Code = Utility.GetStringValue(row, "Name");
                    data.Name = Utility.GetStringValue(row, "Name");
                    listOption.Add(data);
                }

                return listOption.OrderBy(o => o.Name).ToList();
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


    }
}
