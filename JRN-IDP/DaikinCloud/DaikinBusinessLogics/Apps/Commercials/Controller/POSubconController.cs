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
        DataTable dt = new DataTable();
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        private readonly NintexCloudManager ntxManager = new NintexCloudManager();
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();
        private readonly POSubconRepository repo;

        public POSubconController()
        {
            repo = new POSubconRepository(db);
        }

        public List<string> GetBranchCurrentLogin(string CurrentLogin)
        {
            List<string> listBranch = new List<string>();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterPOSubconCreator_GetBranchByCurrentLogin";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);

                db.CloseConnection(ref conn);
                foreach (DataRow row in dt.Rows)
                {
                    listBranch.Add(Utility.GetStringValue(row, "Branch_Name"));
                }
                return listBranch;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POSubconModel> ListData(ClaimReimbursement.Model.FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            var data = repo.ListData(model, out RecordCount, out GrandTotal);
            return data;
        }

    }
}