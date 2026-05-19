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
        private readonly DatabaseManager db;
        private readonly PendingTaskRepository repo;

        public PendingTaskController()
        {
            db = new DatabaseManager();
            repo = new PendingTaskRepository(db);
        }

        public async Task<List<PendingTaskModel>> GetPendingTaskListAsync(string CurrentUserEmail, string CurrentLogin, string CurrentUsername, FilterHeaderSearchModel model)
        {
            return await repo.GetPendingTaskAsync(CurrentUserEmail, CurrentLogin, CurrentUsername, model).ConfigureAwait(false);
        }

        public int GetCountPendingTask(string CurrentLogin, string CurrentUsername)
        {
            var result = repo.GetCountPendingTaskAsync(CurrentLogin, CurrentUsername).GetAwaiter().GetResult();
            return result;
        }
    }
}
