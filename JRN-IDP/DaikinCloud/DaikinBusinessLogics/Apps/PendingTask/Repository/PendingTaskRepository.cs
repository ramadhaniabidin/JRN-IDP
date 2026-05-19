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

namespace Daikin.BusinessLogics.Apps.PendingTask.Repository
{
    public class PendingTaskRepository
    {
        //private readonly DatabaseManager db = new DatabaseManager();
        //private readonly string connString = Utility.GetSqlConnection();
        private readonly DatabaseManager db;
        private readonly string connString;
        private readonly bool configAwait = false;

        public PendingTaskRepository(DatabaseManager _db)
        {
            db = _db;
            connString = _db.GetSQLConnectionString();
        }

        public async Task<List<PendingTaskModel>> GetPendingTaskAsync(string CurrentUserEmail, string CurrentLogin, string CurrentUsername, FilterHeaderSearchModel model)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_NWC_ListPendingApproval", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(db.AddInParameter("CurrentUserLogin", CurrentLogin));
                cmd.Parameters.Add(db.AddInParameter("CurrentUsername", CurrentUsername));
                cmd.Parameters.Add(db.AddInParameter("CurrentUserEmail", CurrentUserEmail));
                cmd.Parameters.Add(db.AddInParameter("PageIndex", model.PageIndex));
                cmd.Parameters.Add(db.AddInParameter("PageSize", model.PageSize));
                cmd.Parameters.Add(db.AddInParameter("SearchBy", model.SearchBy));
                cmd.Parameters.Add(db.AddInParameter("Keywords", model.Keywords));

                await conn.OpenAsync().ConfigureAwait(configAwait);
                using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait))
                {
                    return await Utility.MapReaderToList<PendingTaskModel>(r).ConfigureAwait(configAwait);
                }
            }
        }

        public async Task<int> GetCountPendingTaskAsync(string CurrentLogin, string CurrentUsername)
        {
            int result = 0;
            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand("usp_Utility_CountTasks", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(cmd, "CurrentUserLogin", CurrentLogin);
                db.AddInParameter(cmd, "CurrentUsername", CurrentUsername);
                await conn.OpenAsync().ConfigureAwait(configAwait);
                object scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(configAwait);

                if (scalar != null && scalar != DBNull.Value) result = Convert.ToInt32(scalar);
            }
            return result;
        }

    }
}
