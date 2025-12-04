using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Model
{
    public class FilterHeaderSearchModel
    {
        public int ID { get; set; }

        public string BranchName { get; set; }

        public string ModuleId { get; set; }

        public string ListName { get; set; }

        public string TableName { get; set; }

        public string FilterBy { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int PendingApproverRoleID { get; set; } = 0;

        public string PendingApproverRole { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string Plant_Code { get; set; }

        public string CurrentLogin { get; set; }

        public string SearchBy { get; set; }

        public string Keywords { get; set; }

        public string PaymentStatus { get; set; }

        public string PostingStatus { get; set; }

        public string SortBy { get; set; }

        public string SortType { get; set; }

        public string Procurement_Department { get; set; }
        public string MIGO { get; set; }
        public string RequestorDepartment { get; set; }
    }

    public class GeneralHistoryLogModel
    {
        public string Personal_Name { get; set; }

        public string Position { get; set; }

        public string Comments { get; set; }

        public string Action_Name { get; set; }

        public string Action_Date { get; set; }

        public string Pending_Approver_Name { get; set; }
    }

    public class GeneralHeaderModel
    {
        public int ID { get; set; }

        public bool check { get; set; }

        public int Is_Revise { get; set; }

        public int No { get; set; }

        public string Form_No { get; set; }

        public DateTime? Request_Date { get; set; }

        public string Requester_Name { get; set; }

        public string Requester_Email { get; set; }

        public string Requester_Account { get; set; }

        public string Department { get; set; }

        public string Branch { get; set; }

        public string Bank_Name { get; set; }

        public string Account_Number { get; set; }

        public decimal? Grand_Total { get; set; }

        public int? Item_ID { get; set; }

        public string Content_Type { get; set; }

        public string Pending_Approver_Name { get; set; }
        public string Pending_Approver_Role { get; set; }

        public string Form_Type { get; set; }

        public string Approval_Status { get; set; }

        public string Approval_Status_Name { get; set; }

        public DateTime? Approval_Date { get; set; }

        public string MIRO_No { get; set; }

        public string PR_No { get; set; }

        public string PO_No { get; set; }

        public string QCF_No { get; set; }

        public int? PR_Item_ID { get; set; }

        public int? PO_Item_ID { get; set; }

        public int? QCF_Item_ID { get; set; }

        public string DigiSign { get; set; }

        public string Purchasing_Document { get; set; }

        public DateTime? MIRO_Date { get; set; }

        public DateTime? MIGO_Date { get; set; }

        public DateTime? Scheduled_Payment_Date { get; set; }

        public DateTime? Actual_Payment_Date { get; set; }

        public DateTime? Posting_Date { get; set; }

        public int? Current_Index_Approver { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Created_By { get; set; }

        public DateTime? Modified_Date { get; set; }

        public string Modified_By { get; set; }

        public string Factory { get; set; }

        public decimal Grand_Total_Curr { get; set; }

        public string Currency { get; set; }
        public string Procurement_Department { get; set; }
        public string Material_Anaplan { get; set; }

        public string Form_Url { get; set; }
        public string Form_Desc { get; set; }
    }
}
