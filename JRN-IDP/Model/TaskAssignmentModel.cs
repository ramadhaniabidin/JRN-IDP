using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class TaskAssignmentModel
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Assignee { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CompletedBy { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Outcome { get; set; }
        public string CompletedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string EscalatedTo { get; set; }
    }
}
