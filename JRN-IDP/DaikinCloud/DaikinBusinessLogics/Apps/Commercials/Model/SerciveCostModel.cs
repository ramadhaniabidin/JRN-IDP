using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Model
{
    class ServiceCostModel
    {
    }
    public class SAPInboundModel
    {
        public string BL_No { get; set; }
        public string FOB_No { get; set; }
        public string Inbound_No { get; set; }
    }

    public class ServiceCostRemarks
    {
        public int ID { get; set; }
        public string Remarks { get; set; }
        public string Outcome { get; set; }
        public string Reason_Rejection { get; set; }
    }
    public class PopReferenceModel
    {
        public Int64 No { get; set; }
        public string Reference_No { get; set; }
        public string Document_Date { get; set; }
    }
    public class ServiceCostDetail
    {
        public int ID { get; set; }

        public int? No { get; set; }
        public string Trading_Partner_Code { get; set; }
        public string PPJK_Code { get; set; }

        public int? Header_ID { get; set; }

        public string Form_No { get; set; }
        public string Inbound_No { get; set; }

        public string Document_Date { get; set; }
        public string Ref_Type { get; set; }
        public string Ref_No { get; set; }

        public string BL_No { get; set; }

        public string FOB_No { get; set; }

        public string Vendor_No { get; set; }

        public string Vendor_Name { get; set; }

        public string Vendor_Invoice_No { get; set; }
        public int Condition_ID { get; set; }
        public string Condition_Code { get; set; }

        public string Condition_Name { get; set; }

        public string Text { get; set; }

        public string VAT_No { get; set; }
        public decimal VAT_Percent { get; set; }
        public string VAT_Type { get; set; }
        public decimal? Freight_Cost { get; set; }

        public decimal? Tax_Base_Amount { get; set; }

        public string WHT_Type_Code { get; set; }

        public string WHT_Type_Name { get; set; }
        public decimal WHT_Amount { get; set; }

        public decimal? VAT_Amount { get; set; }
        public decimal Total_Amount { get; set; }

        public string Created_Date { get; set; }
        public string File_Name { get; set; }

        public string Attachment_URL { get; set; }
        public string Business_Place_Code { get; set; }
        public string Partner_Bank_Type { get; set; }
        public string Currency { get; set; }
        public string Assignment { get; set; }
        public string Posting_Date { get; set; }
    }

    public class WHTOptionModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public bool Selected { get; set; }
    }

    public class VATOptionModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string VAT_Percent { get; set; }
        public int Order_x0020_Id { get; set; }
        public bool Selected { get; set; }
    }
    public class ServiceCostConditionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class PPJKOptionModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Curr { get; set; }
        public string Category { get; set; }
        public string Bank_Key { get; set; }
        public string Bank_Account_No { get; set; }
        public string Bank_Name { get; set; }
        public bool Selected { get; set; }
    }

    public class ServiceCostHeader
    {
        public int ID { get; set; }

        public string Form_No { get; set; }

        public string Requester_Name { get; set; }

        public string Requester_Email { get; set; }

        public string Requester_Department { get; set; }
        public string Requester_Account { get; set; }

        public string Branch { get; set; }

        public string Cost_Center { get; set; }

        public string Bank_Key_ID { get; set; }

        public string Bank_Key_Name { get; set; }

        public string Bank_Account_No { get; set; }

        public string Bank_Name { get; set; }

        public string Vendor_Code { get; set; }

        public string Trading_Partner_Code { get; set; }

        public string Trading_Partner_Name { get; set; }

        public string Plant_Code { get; set; }

        public string Plant_Name { get; set; }

        public string PPJK_Code { get; set; }

        public string PPJK_Name { get; set; }
        public string PPJK_Curr { get; set; }
        public string PPJK_Category { get; set; }

        public string Buss_Place_Code { get; set; }

        public string Buss_Place_Name { get; set; }

        public string Expense_Type_Code { get; set; }

        public string Expense_Type_Name { get; set; }

        public decimal? Grand_Total { get; set; }

        public int Item_ID { get; set; }

        public bool? Document_Received { get; set; }

        public DateTime? Received_Date { get; set; }

        public string Approval_Status { get; set; }
        public string Approval_Status_Name { get; set; }

        public DateTime? Approval_Date { get; set; }

        public DateTime? Posting_Date { get; set; }

        public DateTime? Actual_Payment_Date { get; set; }

        public int? Current_Index_Approver { get; set; }

        public string Pending_Approver_Name { get; set; }

        public string Pending_Approver_Role { get; set; }
        public string Pending_Approver_Role_ID { get; set; }
        public string OA_Summary_Attachment { get; set; }
        public string OA_Summary_FileName { get; set; }

        public DateTime? Last_Action_Date { get; set; }

        public string Last_Action_Name { get; set; }

        public string Last_Action_By { get; set; }

        public string PIC_Team { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Created_By { get; set; }

        public DateTime? Modified_Date { get; set; }

        public string Modified_By { get; set; }

    }
}
