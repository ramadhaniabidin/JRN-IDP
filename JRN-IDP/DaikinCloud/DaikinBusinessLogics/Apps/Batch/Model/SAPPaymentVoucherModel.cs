using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Batch.Model
{
    public class SAPPaymentVoucherModel
    {
        public string Nintex_No { get; set; }
        public string MIRO_No { get; set; }
        public string Payment_Date { get; set; }
        public string Payment_Reff_No { get; set; }
        public string Payment_No { get; set; }
        public string Doc_Payment_No { get; set; }
        public string Source_File { get; set; }
    }
}
