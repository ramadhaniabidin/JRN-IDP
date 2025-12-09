using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class ListItemModel
    {
        public string formStatus { get; set; }
        public string approvalStatus { get; set; }
        public string workflowStatus { get; set; }
        public string transID { get; set; }
        public string currentLogin { get; set; }
        public string currentLoginName { get; set; }
        public string vendorName { get; set; }
        public string formNo { get; set; }
        public string taskResponderName { get; set; }
        public string taskResponderEmail { get; set; }
        public string taskResponderLogin { get; set; }
        public string submittedBy { get; set; }
        public string reviseComment { get; set; }
    }
}
