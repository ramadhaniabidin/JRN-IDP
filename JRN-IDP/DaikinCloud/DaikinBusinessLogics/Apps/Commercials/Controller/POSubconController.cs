using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.Commercials.Repository;
using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class POSubconController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        private readonly NintexCloudManager ntxManager = new NintexCloudManager();
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();
        private readonly POSubconRepository repo;

        public POSubconController()
        {
            repo = new POSubconRepository(db);
        }

        public List<string> GetBranchCurrentLogin(string currentLogin)
        {
            return repo.GetBranchCurrentLogin(currentLogin);
        }

        public List<POSubconModel> ListData(ClaimReimbursement.Model.FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            var data = repo.ListData(model, out RecordCount, out GrandTotal);
            return data;
        }

    }
}