using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class LogModel
    {
        public string ListName { get; set; }
        public int ListItemId { get; set; }
        public string EventName { get; set; }
        public string CompiledQuery { get; set; }
        public string SysMessage { get; set; }
    }

    public class CheckApproval
    {
        public string ModuleCode { get; set; }
        public string ListName { get; set; }
        public string SQLTableName { get; set; }
        public string ViewPageUrl { get; set; }
        public bool RequireApproval { get; set; }
    }

    public class GetApprovalData
    {
        public string ModuleCode { get; set; }
        public string ListName { get; set; }
        public string SQLTableName { get; set; }
        public string ViewPageUrl { get; set; }
        public bool RequireApproval { get; set; }
        public string FormNo { get; set; }
        public int HeaderID { get; set; }
        public int ListID { get; set; }
        public string CreatedUserName { get; set; }
        public string CreatedName { get; set; }
        public string CreatedRole { get; set; }
        public string CreatedEmail { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ApproverGroup { get; set; }
    }

    public class ListHeaderReportApproval
    {
        public string Module { get; set; }
        public string ModuleCategory { get; set; }
        public string ModuleCode { get; set; }
        public string Branch { get; set; }
        public string BranchOther { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int No { get; set; }
        public string FormatGenerateCode { get; set; }
        public int Draft { get; set; }
        public int Col1 { get; set; }
        public int Col2 { get; set; }
        public int Col3 { get; set; }
        public int Col4 { get; set; }
        public int Col5 { get; set; }
        public int Col6 { get; set; }
        public int Col7 { get; set; }
        public int Col8 { get; set; }
        public int Col9 { get; set; }
        public int Revise { get; set; }
        public string ProcDept { get; set; }
    }
}
