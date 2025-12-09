using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class PopListModel
    {

    }
    public class PopList_Input
    {
        public string searchTabl { get; set; }

        public string searchCol { get; set; }

        public string searchVal { get; set; }

        public int searchLike { get; set; }

        public int pageIndx { get; set; }

        public int pageSize { get; set; }

        public string joinTable { get; set; }

        public string joinColH { get; set; }

        public string joinColD { get; set; }

        public string joinColDisp { get; set; }
    }
    public class PopList_Output
    {
        public int RecordCount { get; set; }
    }
}
