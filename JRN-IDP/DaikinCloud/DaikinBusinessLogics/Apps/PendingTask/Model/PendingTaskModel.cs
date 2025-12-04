using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.PendingTask.Model
{
    public class PendingTaskModel
    {
        public string ApprovalUrl { get; set; }

        public string ItemTitle { get; set; }

        public string Module { get; set; }

        public int ItemId { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public string wfLink { get; set; }

        public string AssignedTo { get; set; }

        public string RequestorName { get; set; }
        public string PendingApproverRole { get; set; }
        public string Branch { get; set; } 

    }
}
