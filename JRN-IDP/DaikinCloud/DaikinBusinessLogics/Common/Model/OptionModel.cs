using Daikin.BusinessLogics.Apps.Commercials.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class OptionModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Short_x0020_Name { get; set; }
        public string Extra { get; set; }
        public string Active { get; set; }
        public bool Selected { get; set; } = false;
    }

    public class ServiceCostOptionModel
    {
        public bool Success { get; set; }
        public string Messsage { get; set; }
        public List<OptionModel> ListTradingPartner { get; set; }
        public List<OptionModel> ListPlant { get; set; }
        public List<OptionModel> ListBussPlace { get; set; }
        public List<OptionModel> ListExpenseType { get; set; }
        public List<PPJKOptionModel> ListPPJK { get; set; }
        public List<ServiceCostConditionModel> ListConditionSC { get; set; }
        public List<WHTOptionModel> ListWHT { get; set; }
        public List<VATOptionModel> ListVAT { get; set; }
        public List<OptionModel> ListMasterVendor { get; set; }
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
