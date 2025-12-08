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

namespace Daikin.BusinessLogics.Apps.ReportApproval.Controller
{
    public class ReportApprovalController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        private readonly string SP3_CONNSTRING = Utility.GetSqlConnection();
        private readonly string MODULE_CATEGORY_KEY = "Module_Category";
        private readonly string MODULE_KEY = "Module";
        private readonly string BRANCH_KEY = "Branch";
        private readonly string START_DATE_KEY = "StartDate";
        private readonly string END_DATE_KEY = "EndDate";

        #region SPDEV
        public List<ListHeaderReportApproval> SPDEV_ListDataApproval(ListHeaderReportApproval model)
        {
            var dt = new DataTable();
            var storedProcedures = new Dictionary<string, string>
            {
                {"Claim Reimbursement","usp_Approval_ListData"},
                {"Commercials","usp_Approval_ListDataCommercials"},
                {"Non Commercials","usp_Approval_ListDataNonCommercials"}
            };
            using (var conn_ = new SqlConnection(SP3_CONNSTRING))
            {
                conn_.Open();
                using (var cmd = new SqlCommand(storedProcedures[model.ModuleCategory], conn_))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(MODULE_CATEGORY_KEY, model.ModuleCategory);
                    cmd.Parameters.AddWithValue(MODULE_KEY, model.Module.ToUpperInvariant().Contains("SUBCON") ? "PO Subcon" : model.Module);
                    cmd.Parameters.AddWithValue(BRANCH_KEY, model.Branch);
                    cmd.Parameters.AddWithValue(START_DATE_KEY, model.StartDate);
                    cmd.Parameters.AddWithValue(END_DATE_KEY, model.EndDate);
                    if (model.ModuleCategory.ToUpperInvariant() == "NON COMMERCIALS")
                    {
                        cmd.Parameters.AddWithValue("ProcDept", model.ProcDept);
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        reader.Close();
                    }
                }
                conn_.Close();
            }
            return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) :
                new List<ListHeaderReportApproval>();
        }

        #endregion

        public List<ListHeaderReportApproval> ListData(ListHeaderReportApproval model)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Approval_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CATEGORY_KEY, model.ModuleCategory);
                db.AddInParameter(db.cmd, MODULE_KEY, model.Module);
                db.AddInParameter(db.cmd, BRANCH_KEY, model.Branch);
                db.AddInParameter(db.cmd, START_DATE_KEY, model.StartDate);
                db.AddInParameter(db.cmd, END_DATE_KEY, model.EndDate);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<ListHeaderReportApproval> ListDataNonCommercials(ListHeaderReportApproval model)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Approval_ListDataNonCommercials";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CATEGORY_KEY, model.ModuleCategory);
                db.AddInParameter(db.cmd, MODULE_KEY, model.Module);
                db.AddInParameter(db.cmd, BRANCH_KEY, model.Branch);
                db.AddInParameter(db.cmd, START_DATE_KEY, model.StartDate);
                db.AddInParameter(db.cmd, END_DATE_KEY, model.EndDate);
                db.AddInParameter(db.cmd, "ProcDept", model.ProcDept);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<ListHeaderReportApproval> ListDataCommercials(ListHeaderReportApproval model)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Approval_ListDataCommercials";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CATEGORY_KEY, model.ModuleCategory);
                db.AddInParameter(db.cmd, MODULE_KEY, model.Module);
                db.AddInParameter(db.cmd, BRANCH_KEY, model.Branch);
                db.AddInParameter(db.cmd, START_DATE_KEY, model.StartDate);
                db.AddInParameter(db.cmd, END_DATE_KEY, model.EndDate);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }
    }
}
