using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class NintexWorkflowCloud
    {
        public string url { get; set; }
        public NWCParamModel param { get; set; }
    }

    public class NWCParamModel
    {
        public StartData startData { get; set; }
    }
    public class StartData
    {
        public int se_headerid { get; set; }
        public int se_itemid { get; set; }
        public string se_tablename { get; set; }
        public string se_payload { get; set; }
        public string se_type { get; set; }
        public string se_message { get; set; }
        public string se_useremail { get; set; }
        public string se_filename { get; set; }
        public string se_invoiceid { get; set; }
        public string se_invoicenumber { get; set; }
        public string se_attachmenturl { get; set; }
    }
}
