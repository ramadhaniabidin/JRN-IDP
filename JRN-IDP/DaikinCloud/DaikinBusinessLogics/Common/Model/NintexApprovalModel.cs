using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class NintexApprovalModel
    {
        public string CurrentLogin { get; set; }
        public string Comment { get; set; }
        public int Outcome { get; set; }
        //public SPWeb oWeb { get; set; }
        public string FormNo { get; set; }
        public string Module { get; set; }
        public string Position_ID { get; set; }
        public int Transaction_ID { get; set; }
        public int Item_ID { get; set; }
        public string OutcomeName { get; set; }
    }

    public class TaskResponse
    {
        public List<TaskItem> Tasks { get; set; }
    }

    public class TaskItem
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
        public List<TaskAssignment> TaskAssignments { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowId { get; set; }
        public string WorkflowInstanceId { get; set; }
    }

    public class TaskAssignment
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
