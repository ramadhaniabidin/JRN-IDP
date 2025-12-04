using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daikin.BusinessLogics.Apps.Master.Model;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Model
{
    public class POContractHeader
    {
        public int ID { get; set; } = 0;

        public string Form_No { get; set; }

        public string Requester_Name { get; set; }

        public string Requester_Email { get; set; }

        public string Requester_Department { get; set; }

        public string Procurement_Department { get; set; }

        public string Procurement_Department_ID { get; set; }

        public string Procurement_Department_Code { get; set; }

        public string Procurement_Department_Code_PO { get; set; }

        public int? Marketing_Category_ID { get; set; }

        public string Marketing_Category_Name { get; set; }

        public string Vendor_Code { get; set; }

        public string Vendor_Name { get; set; }

        public string Branch { get; set; }

        public string Cost_Center { get; set; }

        public decimal? Grand_Total { get; set; }

        public int? Item_ID { get; set; }

        public bool? Document_Received { get; set; }

        public DateTime? Received_Date { get; set; }

        public int? Is_DigiSign_Processed { get; set; }

        public DateTime? DigiSign_Processed_Date { get; set; }

        public string DigiSign_Attachment_Url { get; set; }

        public string Release_Strategy { get; set; }

        public string Release_Group { get; set; }

        public string Purchasing_Document { get; set; }

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

        public int Contract_ID { get; set; }

        public bool? Submitted { get; set; } = false;

        public bool? Edited { get; set; } = false;

        public List<POContractDetail> Detail { get; set; }
    }

    public class POContractDetail
    {
        public int ID { get; set; } = 0;

        public int? Header_ID { get; set; } = 0;

        public string Form_No { get; set; } = "";

        public int? No { get; set; } = 0;

        public int? Contract_ID { get; set; } = 0;

        public string Contract_No { get; set; }

        public string Remarks_Contract { get; set; }

        public DateTime? Create_PO_From_Period { get; set; }

        public DateTime? Create_PO_To_Period { get; set; }

        public DateTime? Created_Date { get; set; }

        public List<OptionModel> Remark { get; set; }

        public DateTime? Period_Start { get; set; }

        public DateTime? Period_End { get; set; }

        public string Internal_Order_Code { get; set; }

        public string Internal_Order_Name { get; set; }

        public bool? Show { get; set; }

        public decimal? Grand_Total { get; set; }

        public List<POContractMaterial> Materials { get; set; }

        public List<POContractAttachment> Attachments { get; set; }
    }

    public class POContractMaterial
    {
        public int ID { get; set; } = 0;

        public int? Header_ID { get; set; } = 0;

        public int? Detail_ID { get; set; } = 0;

        public string Form_No { get; set; } = "";

        public int? Contract_Detail_Id { get; set; }

        public int? Contract_ID { get; set; } = 0;

        public string Contract_No { get; set; }

        public string Remarks_Contract { get; set; }

        public int? No { get; set; } = 0;

        public string Material_Number { get; set; }

        public string Material_Name { get; set; }

        public string Material_Description { get; set; }

        public string GL { get; set; }

        public string GL_Description { get; set; }

        public int? Qty { get; set; } = 1;

        public string Text { get; set; }

        public string Cost_Center { get; set; }

        public bool? WHT { get; set; } = false;

        public bool? Variable_Amount { get; set; }

        public decimal? Contract_Amount { get; set; }

        public MasterMappingCostCenter CostCenter { get; set; } = new MasterMappingCostCenter();
    }

    public class POContractAttachment
    {
        public int ID { get; set; }

        public int? Header_ID { get; set; }

        public string Form_No { get; set; }

        public string Attachment_Url { get; set; }

        public string Attachment_FileName { get; set; }
    }

    public class MasterMappingCostCenter
    {
        public int? ID { get; set; }

        public string Cost_Center { get; set; }

        public string Description { get; set; }

        public string Business_Area { get; set; }

        public string Branch { get; set; }

        public string Combine { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool Selected { get; set; } = false;

    }

    public class POReleaseHeader
    {
        public int ID { get; set; }

        public string Form_No { get; set; }

        public DateTime? Request_Date { get; set; }

        public int? Procurement_Department_ID { get; set; }

        public string Procurement_Department { get; set; }

        public string Branch { get; set; }

        public string Vendor_Code { get; set; }

        public string Vendor_Name { get; set; }

        public string Requester_Name { get; set; }

        public string Requester_Email { get; set; }

        public string Requester_Department { get; set; }

        public string Company_Code { get; set; }

        public int? PO_Header_ID { get; set; }

        public int? PO_Item_ID { get; set; }

        public string PO_No { get; set; }

        public string Purchasing_Document { get; set; }

        public string Release_Group { get; set; }

        public string Release_Strategy { get; set; }

        public string Purchasing_Doc_Type { get; set; }

        public string PIC_Team { get; set; }

        public DateTime? Created_Date { get; set; }

        public string Created_By { get; set; }

        public DateTime? Modified_Date { get; set; }

        public string Modified_By { get; set; }

        public int? Item_ID { get; set; }
    }


    public class SAPNonCommercialPODataHeader
    {
        public int ID { get; set; }

        public string Form_No { get; set; }

        public string Purchasing_Document { get; set; }

        public string Company_Code { get; set; }

        public string Purchasing_Doc_Type { get; set; }

        public string Release_Group { get; set; }

        public string Release_Strategy { get; set; }
    }
}