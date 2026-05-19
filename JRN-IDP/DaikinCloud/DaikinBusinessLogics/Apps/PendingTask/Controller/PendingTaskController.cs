using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Apps.PendingTask.Model;
using Daikin.BusinessLogics.Apps.PendingTask.Repository;
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
        private readonly PendingTaskRepository repo = new PendingTaskRepository();

        public async Task<List<PendingTaskModel>> GetPendingTaskListAsync(string CurrentUserEmail, string CurrentLogin, string CurrentUsername, FilterHeaderSearchModel model)
        {
            return await repo.GetPendingTaskAsync(CurrentUserEmail, CurrentLogin, CurrentUsername, model).ConfigureAwait(false);
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
