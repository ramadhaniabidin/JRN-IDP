using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Model
{
    public class PIBModel
    {
        //	   A.Form_No, A.Created_Time, A.Requester_Name, A.Billing_Code, A.PIB_Number, A.PEN_Number,A.Total, A.Approval_Status, A.Branch
        public int ID { get; set; }
        public Int64 No { get; set; }
        public int Can_Edit { get; set; }
        public string Form_No { get; set; }
        public int Item_ID { get; set; }
        public string Created_Time { get; set; }
        public string Requester_Name { get; set; }
        public string Billing_Code { get; set; }
        public string PIB_Number { get; set; }
        public string PEN_Number { get; set; }
        public string Approval_Status { get; set; }
        public string Branch { get; set; }
        public decimal Total { get; set; }
        public string Pending_Approver_Name { get; set; }
        public string Pending_Approver_Role { get; set; }
        public string Is_New { get; set; }

    }
}