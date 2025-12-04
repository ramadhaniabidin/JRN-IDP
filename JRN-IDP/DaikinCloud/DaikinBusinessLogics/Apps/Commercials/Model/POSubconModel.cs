using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Model
{
    public class POSubconDetailModel
    {
        public int No { get; set; }
        public string Item_Text { get; set; }
        public string Item_Code { get; set; }
        public string Delivery_Date { get; set; }
        public string UoM { get; set; }
        public string Material_Code { get; set; }
        public decimal PO_Qty { get; set; }
        public decimal Base_Amount { get; set; }
        public decimal Net_Price { get; set; }
        public string DDL_VAT { get; set; }
        public int VAT_ID { get; set; }
        public string VAT_Code { get; set; }
        public decimal VAT_Percentage { get; set; }
        public decimal VAT_Amount { get; set; }
        public string DDL_WHT { get; set; }
        public decimal WHT_Amount { get; set; }
        public string WHT_Description { get; set; }
        public decimal WHT_Percent { get; set; }

    }
    public class POSubconAttachmentModel
    {
        public int ID { get; set; }
        public string Title { get; set; }

    }
    public class POSubconModel
    {
        public int ID { get; set; }
        public string Form_No { get; set; }
        public string Purchasing_Document { get; set; }
        public string Document_Date { get; set; }
        public string Created_Date { get; set; }
        public string Requester_Name { get; set; }
        public string Requester_Email { get; set; }
        public string Requester_Branch { get; set; }
        public string Requester_Account { get; set; }
        public string Vendor_Number { get; set; }
        public string Vendor_Name { get; set; }
        public string Bank_Account_Name { get; set; }
        public string Bank_Key_Name { get; set; }
        public string Account_Number { get; set; }
        public string MIGO_Date { get; set; }
        public string MIRO_Date { get; set; }
        public string Scheduled_Payment_Date { get; set; }
        public string Actual_Payment_Date { get; set; }
        public string Approval_Status { get; set; }
        public string Approval_Status_Name { get; set; }
        public int Subcon_Category_ID { get; set; }
        public string SVO_No { get; set; }
        public int No { get; set; }
        public int Item_ID { get; set; }
        public decimal Grand_Total { get; set; }
        public int Can_Edit { get; set; }
        public string Attachment_Url { get; set; }
        public string Purchasing_Group { get; set; }
        public int Released { get; set; }
        public string Pending_Approver_Name { get; set; }
        public string Pending_Approver_Role { get; set; }
        public string Approval_Date { get; set; }
        public string Subcon_Category_Name { get; set; }
        public string Posting_Date { get; set; }
        public string PO_Number_Sales_Force { get; set; }
        public string Is_New { get; set; }
    }

    public class VendorSubconModel
    {
        public int No { get; set; }
        public string Vendor_Code { get; set; }
        public string Vendor_Name { get; set; }
    }
}