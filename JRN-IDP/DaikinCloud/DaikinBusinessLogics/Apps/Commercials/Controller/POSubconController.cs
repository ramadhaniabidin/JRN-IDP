using Daikin.BusinessLogics.Apps.Commercials.Model;
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
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        private readonly NintexCloudManager ntxManager = new NintexCloudManager();
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();

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

        public List<BusinessLogics.Apps.Commercials.Model.POSubconModel> ListData(ClaimReimbursement.Model.FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            //usp_POSubconHeader_ListData
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_POSubconHeader_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", 10);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "PendingApproverRoleID", model.PendingApproverRoleID);

                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                return dt.Rows.Count > 0 ?
                    Utility.ConvertDataTableToList<BusinessLogics.Apps.Commercials.Model.POSubconModel>(dt) :
                    new List<BusinessLogics.Apps.Commercials.Model.POSubconModel>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }

        }

    }
}