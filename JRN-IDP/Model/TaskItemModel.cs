using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class TaskItemModel
    {
        public string AssignmentBehavior { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletionCriteria { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Id { get; set; }
        public string Initiator { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }
        public DateTime? Modified { get; set; }
        public string Name { get; set; }
        public List<string> Outcomes { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public List<TaskAssignmentModel> TaskAssignments { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowId { get; set; }
        public string WorkflowInstanceId { get; set; }
    }
}
