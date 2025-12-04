using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Master.Model
{
    public class OptionModel
    {
        public string Code
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public string Short_x0020_Name
        {
            get;
            set;
        }

        public string Extra
        {
            get;
            set;
        }

        public string Active
        {
            get;
            set;
        }

        public bool Selected
        {
            get;
            set;
        } = false;
    }

    public class MasterModuleOptionModel
    {
        public string Code
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;

        }
        public string List_Name
        {
            get;
            set;
        }

        public string List_Approval
        {
            get; set;
        }

        public string Table_Name
        {
            get;
            set;
        }

        public string Module_Url { get; set; }

    }
}