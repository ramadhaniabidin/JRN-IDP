using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class ListDataID
    {
        public int ID
        {
            get; set;
        }

        public string NAC_Guid
        {
            get; set;
        }
        public string Form_No { get; set; }
    }
    public class ApproverRoleModel
    {
        public int Position_ID { get; set; }
        public string Position_Name { get; set; }
        public int Order_ID { get; set; }
        public string Module_Code { get; set; }
    }
    public class ResponseWorkflow
    {
        public string id { get; set; }
    }
    public class NintexWorkflowCloud
    {
        public string url { get; set; }
        public string endpoint { get; set; }
        public NWCParamModel param { get; set; }
    }

    public class NWCParamModel
    {
        public StartData startData { get; set; }
        //public Options options { get; set; }
    }
    public class StartData
    {
        public int se_headerid { get; set; }
        public int se_itemid { get; set; }
        public string se_tablename { get; set; }
        public string se_modulecode { get; set; }
        public string se_listname { get; set; }
        public string se_ponumber { get; set; }
    }

    public class CurrentApproverModel
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}