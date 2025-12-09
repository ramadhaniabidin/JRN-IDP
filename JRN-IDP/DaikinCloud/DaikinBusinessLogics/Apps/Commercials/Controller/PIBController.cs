using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class PIBController
    {
        DataTable dt = new DataTable();
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        private readonly NintexCloudManager nintexCloudManager = new NintexCloudManager();

        public List<PIBModel> ListData(FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_PIBHeader_ListData";
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
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<PIBModel>(dt) : new List<PIBModel>();
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public CommonResponseModel ApprovalAction(string ApprovalOutcome, string ListName, string Module_Code, string Form_No, int ItemID, int HeaderID, string Comment)
        {
            try
            {
                CurrentApproverModel taskResponder = nintexCloudManager.Commercial_GetTaskResponder(Module_Code, HeaderID, Form_No);
                if (taskResponder == null)
                {
                    return new CommonResponseModel { Success = false, Message = "Error occurred when retrieving Task Responder on Commercial_GetTaskResponder method" };
                }
                var transactionData = Aprroval.GetListDataIDByHeaderID_New(ListName, HeaderID);
                var taskAssignmentResponse = nintexCloudManager.GetTaskAssignment(transactionData[0].NAC_Guid, Form_No);
                if (!taskAssignmentResponse.Success || taskAssignmentResponse.TaskAssignments == null)
                {
                    return new CommonResponseModel { Success = false, Message = taskAssignmentResponse.Message };
                }
                return NintexCloudManager.ProcessNACTask(taskAssignmentResponse.TaskAssignments, taskResponder.Email, ApprovalOutcome);
            }
            catch (Exception ex)
            {
                return new CommonResponseModel
                {
                    Success = false,
                    Message = $"Error in ApprovalAction method in ServiceCostController | {ex.Message}"
                };
            }
        }

        public CommonResponseModel SubmitReviseLog(int Header_ID, string CurrentLogin, string CurrentLoginName, string Comment)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[usp_Commercial_InsertHistoryLog]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                db.AddInParameter(db.cmd, "Module_Code", "M025");
                db.AddInParameter(db.cmd, "Action", 19);
                db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrLoginName", CurrentLoginName);
                db.AddInParameter(db.cmd, "Comment", string.IsNullOrEmpty(Comment) ? "Submit Revise" : Comment);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = true, Message = "OK" };
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = false, Message = ex.Message };
            }
        }
    }
}
