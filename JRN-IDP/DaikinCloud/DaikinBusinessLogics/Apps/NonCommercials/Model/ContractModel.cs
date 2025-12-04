using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Model
{
    public class ContractHeader
    {

        public int ID { get; set; }

        public string Form_No { get; set; }

        public DateTime? Request_Date { get; set; }

        public string Requester_Name { get; set; }

        public string Requester_Email { get; set; }

        public string Requester_Department { get; set; }

        public string Procurement_Department { get; set; }

        public string Procurement_Department_Code { get; set; }

        public string Procurement_Department_Code_PO { get; set; }

        public string Internal_Order_Code { get; set; }

        public string Internal_Order_Name { get; set; }

        public string PO_Number { get; set; }

        public int? Contract_Status_ID { get; set; }

        public string Contract_Status_Name { get; set; }

        public string Branch { get; set; }

        public string Cost_Center { get; set; }

        public string Vendor_Code { get; set; }

        public string Vendor_Name { get; set; }

        public string Contract_No { get; set; }

        public int? Contract_Type_ID { get; set; }

        public string Contract_Type_Name { get; set; }

        public DateTime? Period_Start { get; set; }

        public DateTime? Period_End { get; set; }

        public string Remarks { get; set; }

        public decimal? Grand_Total { get; set; }

        public int? Item_ID { get; set; }

        public bool? Document_Received { get; set; }

        public DateTime? Received_Date { get; set; }

        public string Approval_Status { get; set; }

        public DateTime? Approval_Date { get; set; }

        public DateTime? Posting_Date { get; set; }

        public DateTime? Scheduled_Payment_Date { get; set; }

        public DateTime? Actual_Payment_Date { get; set; }

        public int? Current_Index_Approver { get; set; }

        public string Pending_Approver_Name { get; set; }

        public string Pending_Approver_Role { get; set; }

        public string Pending_Approver_Role_ID { get; set; }

        public DateTime? Last_Action_Date { get; set; }

        public string Last_Action_Name { get; set; }

        public string Last_Action_By { get; set; }

        public string PIC_Team { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Created_By { get; set; }

        public DateTime? Modified_Date { get; set; }

        public string Modified_By { get; set; }

        public string Contract_Period { get; set; }

        public bool? IsShow { get; set; } = true;

        public bool? IsDisabled { get; set; } = false;

        public bool? IsEdited { get; set; } = true;

        public List<ContractDetail> ContractDetail { get; set; }

        public List<ContractAttachment> ContractAttachment { get; set; }

    }

    public class ContractDetail
    {
        public int ID { get; set; }

        public int? Header_ID { get; set; }

        public string Form_No { get; set; }

        public string Contract_No { get; set; }

        public int? No { get; set; }

        public string Material_Number { get; set; }

        public string Material_No { get; set; }

        public string Material_Name { get; set; }

        public string Material_Description { get; set; }

        public decimal? Contract_Amount { get; set; }

        public bool? Variable_Amount { get; set; }

        public DateTime? Created_Date { get; set; }

    }

    public class ContractAttachment
    {
        public int ID { get; set; }

        public int? Header_ID { get; set; }

        public string Form_No { get; set; }

        public string Attachment_Url { get; set; }

        public string Attachment_FileName { get; set; }

        public DateTime? Created_Date { get; set; }

        public int? Size { get; set; }

    }

    public class MasterUserProcDept
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public int? Procurement_Department_ID { get; set; }

        public string Procurement_Department_Title { get; set; }

        public string Procurement_Department_Code { get; set; }

        public int? Branch_ID { get; set; }

        public string Branch_Title { get; set; }

        public string Branch_Code { get; set; }

        public string Branch_BusinessArea { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool Selected { get; set; } = false;

        public int? ContractCount { get; set; }

    }
}