using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Controller
{
    public class ClaimReimbursementController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        private readonly CommonLogic func = new CommonLogic();
        private readonly SharePointManager sp = new SharePointManager();
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();

        private SqlCommand GenerateCommandListDataAffiliateClaim(FilterHeaderSearchModel model, SqlConnection Conn)
        {
            SqlCommand cmd = new SqlCommand("usp_AffiliateClaim_ListData_GetByGroup", Conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(db.AddInParameter("TableName", model.TableName));
            cmd.Parameters.Add(db.AddInParameter("PageIndex", model.PageIndex));
            cmd.Parameters.Add(db.AddInParameter("PageSize", model.PageSize));
            cmd.Parameters.Add(db.AddInParameter("FilterDate", model.FilterBy));
            cmd.Parameters.Add(db.AddInParameter("SearchBy", model.SearchBy));
            cmd.Parameters.Add(db.AddInParameter("Keywords", model.Keywords));
            cmd.Parameters.Add(db.AddInParameter("StartDate", model.StartDate));
            cmd.Parameters.Add(db.AddInParameter("EndDate", model.EndDate));
            cmd.Parameters.Add(db.AddInParameter("CurrentLogin", model.CurrentLogin));
            cmd.Parameters.Add(db.AddInParameter("PaymentStatus", model.PaymentStatus));
            cmd.Parameters.Add(db.AddInParameter("ApprovalStatus", model.PostingStatus));
            cmd.Parameters.Add(db.AddInParameter("PendingApproverRole", model.PendingApproverRole));
            cmd.Parameters.Add(db.AddInParameter("BranchName", model.BranchName));
            return cmd;
        }

        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
            bool isAffiliate = model.TableName == "AffiliateClaimHeader" || model.TableName == "AffiliateNotClaimHeader";
            string query = isAffiliate ? "[usp_AffiliateFullyClaim_ListData]" : "usp_ClaimReimbursement_ListData";
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = query;
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", model.TableName);
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

        public async Task<List<GeneralHeaderModel>> ListDataAffiliateClaim(FilterHeaderSearchModel model)
        {
            List<GeneralHeaderModel> list = new List<GeneralHeaderModel>();
            using (SqlConnection _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (SqlCommand cmd = GenerateCommandListDataAffiliateClaim(model, _conn))
                {
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return await Utility.MapReaderToList<GeneralHeaderModel>(r).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task<string> ReturnListDataAffiliateClaim(FilterHeaderSearchModel model)
        {
            List<GeneralHeaderModel> list = await ListDataAffiliateClaim(model);
            return js.Serialize(new
            {
                ProcessSuccess = true,
                InfoMessage = "OK",
                Items = list,
                RecordCount = list[0].RecordCount,
                GrandTotal = list[0].GrandTotal,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                CurrentLogin = model.CurrentLogin,
                model = model,
            });
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

                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new OptionModel
                    {
                        Code = Utility.GetStringValue(row, "Name"),
                        Name = Utility.GetStringValue(row, "Name")
                    });
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
