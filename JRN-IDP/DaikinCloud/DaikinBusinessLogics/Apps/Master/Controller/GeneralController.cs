using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Apps.Master.Repository;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
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
        private readonly DatabaseManager db;
        private readonly MasterRepository repo;
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public GeneralController()
        {
            db = new DatabaseManager();
            repo = new MasterRepository(db);
        }

        public void ExecuteNonQuery(string query)
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

        public List<GeneralHistoryLogModel> GetHistoryLog(string Form_No, string ModuleCode, int Transaction_ID)
        {
            var logs = repo.GetHistoryLog(Form_No, ModuleCode, Transaction_ID);
            return logs;
        }

        public int GetTransactionIDByFormNo(string Form_No, string Module_Code)
        {
            try
            {
                int Transaction_ID = 0;
                db.OpenConnection(ref conn);
                string query = Module_Code == "M014" ?
                    "SELECT TOP 1 ID FROM ContractHeader WHERE Form_No = @form_no" :
                    "SELECT TOP 1 ID FROM POContractHeader WHERE Form_No = @form_no";
                db.cmd.CommandText = query;
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
            finally
            {
                db.CloseConnection(ref conn);
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
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<GeneralHeaderModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            return repo.ListData(model, out RecordCount, out GrandTotal);
        }


        public List<GeneralHeaderModel> SAPBusinssPartner_ListData(FilterHeaderSearchModel model, out int RecordCount)
        {
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                using (var cmd = new SqlCommand("usp_SAPBusinessPartner_ListData", _conn))
                {
                    #region Define parameters
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@TableHeader", SqlDbType = SqlDbType.VarChar, Size = -1, Value = model.TableName ?? "", Direction = ParameterDirection.Input });
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

    }
}