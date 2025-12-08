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

        #region SPDEV
        public List<ListHeaderReportApproval> SPDEV_ListDataApproval(ListHeaderReportApproval model)
        {
            try
            {
                var dt = new DataTable();
                var storedProcedures = new Dictionary<string, string>
                {
                    {"Claim Reimbursement","usp_Approval_ListData"},
                    {"Commercials","usp_Approval_ListDataCommercials"},
                    {"Non Commercials","usp_Approval_ListDataNonCommercials"}
                };
                using (var conn = new SqlConnection(SP3_CONNSTRING))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(storedProcedures[model.ModuleCategory], conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("Module_Category", model.ModuleCategory);
                        cmd.Parameters.AddWithValue("Module", model.Module.ToUpperInvariant().Contains("SUBCON") ? "PO Subcon" : model.Module);
                        cmd.Parameters.AddWithValue("Branch", model.Branch);
                        cmd.Parameters.AddWithValue("StartDate", model.StartDate);
                        cmd.Parameters.AddWithValue("EndDate", model.EndDate);
                        if(model.ModuleCategory.ToUpperInvariant() == "NON COMMERCIALS")
                        {
                            cmd.Parameters.AddWithValue("ProcDept", model.ProcDept);
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            reader.Close();
                        }
                    }
                    conn.Close();
                }
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch(Exception)
            {
                throw;
            }
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
                db.AddInParameter(db.cmd, "Module_Category", model.ModuleCategory);
                db.AddInParameter(db.cmd, "Module", model.Module);
                db.AddInParameter(db.cmd, "Branch", model.Branch);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
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
                db.AddInParameter(db.cmd, "Module_Category", model.ModuleCategory);
                db.AddInParameter(db.cmd, "Module", model.Module);
                db.AddInParameter(db.cmd, "Branch", model.Branch);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "ProcDept", model.ProcDept);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
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
                db.AddInParameter(db.cmd, "Module_Category", model.ModuleCategory);
                db.AddInParameter(db.cmd, "Module", model.Module);
                db.AddInParameter(db.cmd, "Branch", model.Branch);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ListHeaderReportApproval>(dt) : new List<ListHeaderReportApproval>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}
