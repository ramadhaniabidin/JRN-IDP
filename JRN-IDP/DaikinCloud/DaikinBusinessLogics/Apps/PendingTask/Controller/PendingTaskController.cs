using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Apps.PendingTask.Model;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.PendingTask.Controller
{
    public class PendingTaskController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public List<PendingTaskModel> GetPendingTaskList(string SiteUrl, SPWeb oWeb, string CurrentUserEmail, string CurrentLogin, string CurrentUsername, FilterHeaderSearchModel model, out int RecordCount)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_NWC_ListPendingApproval";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "CurrentUserLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrentUsername", CurrentUsername);
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", model.PageSize);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "CurrentUserEmail", CurrentUserEmail);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                db.CloseConnection(ref conn);

                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<PendingTaskModel>(dt) : new List<PendingTaskModel>();
                
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public int GetCountPendingTask(string CurrentLogin, string CurrentUsername)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = "usp_Utility_CountTasks";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(db.cmd, "CurrentUserLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrentUsername", CurrentUsername);

                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn, true);

                var Count = dt.Rows[0].Field<int>(0);

                return Count;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}
