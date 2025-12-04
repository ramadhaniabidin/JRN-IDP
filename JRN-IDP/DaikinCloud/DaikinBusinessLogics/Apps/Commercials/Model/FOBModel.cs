using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Model
{
    public class FOBModel
    {

    }
    public class FOBRemarksModel
    {
        public int ID { get; set; }
        public string Remarks { get; set; }
        public string Outcome { get; set; }
        public string Reason { get; set; }
    }
    public class FOBHeaderModel
    {
        public string Form_No { get; set; }
        public int ID { get; set; }
        public string Requester_Name { get; set; }
        public string Requester_Email { get; set; }
        public string Factory { get; set; }
        public string Due_On { get; set; }
        public string Currency { get; set; }

        public string Plant_Code { get; set; }
        public string Plant_Name { get; set; }

        public decimal Grand_Total { get; set; }
        public decimal Grand_Total_Curr { get; set; }
        public int Item_ID { get; set; }
        public string Approval_Status { get; set; }
        public string Approval_Status_Name { get; set; }
        public string Modified_By { get; set; }
        public string Requester_Account { get; set; }
        public string Pending_Approver_Role_ID { get; set; }
        public string OA_Summary_Attachment { get; set; }
        public string OA_Summary_FileName { get; set; }
    }
    public class FOBDetailModel
    {
        public bool check { get; set; }
        public string Document_No { get; set; }
        public string Business_Place_Code { get; set; }
        public string Business_Place_Name { get; set; }
        public string Text { get; set; }
        public string Reference { get; set; }
        public string Document_Date { get; set; }
        public string Posting_Date { get; set; }
        public string Net_Due_Date { get; set; }
        public decimal Amount_In_Local_Curr { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string File_Name { get; set; }
        public string Attachment_URL { get; set; }
        public int Header_ID { get; set; }
        public string Form_No { get; set; }
    }
}