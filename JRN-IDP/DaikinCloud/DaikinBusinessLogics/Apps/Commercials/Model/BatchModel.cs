using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Model
{
    public class BatchModel
    {
        public string BatchFile
        {
            get; set;
        }
        public int no { get; set; }
    }

    public class FolderLocationModel
    {
        public string ModuleCode { get; set; }
        public string PathLocation { get; set; }
    }

    public class ProcBranchModel
    {
        public string BranchCode { get; set; }
        public string ProcDept { get; set; }
    }
}