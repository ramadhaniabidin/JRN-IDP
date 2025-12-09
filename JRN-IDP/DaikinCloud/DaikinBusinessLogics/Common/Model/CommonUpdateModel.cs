using Daikin.BusinessLogics.Apps.Commercials.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class CommonUpdateModel
    {
        public string ListName { get; set; }
        public int ListItemId { get; set; }
        public string ApprovalStatus { get; set; }
        public string WorkflowStatus { get; set; }
        public string FormStatus { get; set; }
        public string ApproverName { get; set; }
        public string ApproverEmail { get; set; }
        public string ApproverRole { get; set; }
        public string Status { get; set; }
    }

    public class AttachmentURLModel
    {
        public string PoNumber { get; set; }
        public string Url { get; set; }
    }

    public class CommonResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class GetTaskResponseModel: CommonResponseModel
    {
        public IEnumerable<dynamic> Tasks { get; set; }
        public dynamic Task { get; set; }
    }

    public class TaskAssignmentResponseModel: CommonResponseModel
    {
        public TaskItem TaskAssignments { get; set; }
    }

    public class CommonSaveResponseModel: CommonResponseModel
    {
        public int ID { get; set; }
    }

    public class SaveHeaderSCModel: CommonSaveResponseModel
    {
        public ServiceCostHeader Header { get; set; }
    }

    public class SaveHeaderFOBModel: CommonSaveResponseModel
    {
        public FOBHeaderModel Header { get; set; }
    }

}
