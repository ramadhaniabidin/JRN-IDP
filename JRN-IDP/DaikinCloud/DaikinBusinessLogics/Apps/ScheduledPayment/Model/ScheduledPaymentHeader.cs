using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.ScheduledPayment.Model
{
    public class ScheduledPaymentHeader
    {
        public int ID { get; set; }
        public int Item_ID { get; set; }
        public string Form_No { get; set; }
        public DateTime Payment_Date { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Bank_Name { get; set; }
        public DateTime Approval_Date { get; set; }
        public string Approval_Status { get; set; }
    }

    public class FilterHeader
    {
        public string SearchBy { get; set; }
        public string Keywords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Payment_Date_Start { get; set; }
        public string Payment_Date_End { get; set; }
        public string BankName { get; set; }
    }
}
