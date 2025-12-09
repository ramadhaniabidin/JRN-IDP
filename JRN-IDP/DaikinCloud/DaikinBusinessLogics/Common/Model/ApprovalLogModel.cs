using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class ApprovalLogModel
    {
        public int Transaction_ID { get; set; }
        public string Module_Code { get; set; }
        public string Module_Name { get; set; }
        public string Form_No { get; set; }
        public string Personal_Name { get; set; }
        public string Comments { get; set; }
        public int Action { get; set; }
        public string Position { get; set; }
        public string Personal_Account { get; set; }
    }
}
