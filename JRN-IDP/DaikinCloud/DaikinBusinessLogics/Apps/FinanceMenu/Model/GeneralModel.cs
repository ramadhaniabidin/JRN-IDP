using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.FinanceMenu.Model
{
    public class WebModel<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int CountItem { get; set; }
        public int RecordCount { get; set; }
        public decimal Total { get; set; }
        public decimal GrandTotal { get; set; }
        public static WebModel<T> DefaultOk(T data)
        {
            WebModel<T> res = new WebModel<T>();
            res.Code = 200;
            res.Message = "OK";
            res.Data = data;
            return res;
        }
        public static WebModel<T> DefaultError(string message)
        {
            WebModel<T> res = new WebModel<T>();
            res.Code = 500;
            res.Message = message;
            return res;
        }
    }

    public class FinanceIdentificationModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class FinanceMenuModel
    {
        public bool check { get; set; }
        public int ID { get; set; }
        public string Nintex_No { get; set; }
        public string Red_Receipt { get; set; }
        public DateTime Payment_Date { get; set; }
        public string Identification { get; set; }
        public int Transaction_List_Item_ID { get; set; }
        public int Sequence_Number { get; set; }
        public string Module_ID { get; set; }
        public string Document_Number { get; set; }
        public string Finance_Status { get; set; }
        public int Fiscal_Year { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Vendor_Number { get; set; }
        public string Account_Number { get; set; }
        public string Vendor_Name { get; set; }
        public string Account_Name { get; set; }
        public DateTime? Run_Date { get; set; }
        public string Due_Date { get; set; }
        public DateTime? Posting_Date { get; set; }
        public DateTime? Actual_Payment_Date { get; set; }
        public string Item_Text { get; set; }
        public string Branch { get; set; }
        public string Bank_Key { get; set; }
        public string Bank_Name { get; set; }
        public string Bene_ID { get; set; }
        public string PO_No { get; set; }
        public string Invoice_No { get; set; }
        public string Requester_Name { get; set; }
        public string Requester_Email { get; set; }
        public string SAP_Payment_No { get; set; }
        public string Pending_Approver_Name { get; set; }
        public string Pending_Approver_Role { get; set; }
        public string List_Name { get; set; }
        public string Procurement_Department_Name { get; set; }
    }

    public class FinanceMenuExcelModel
    {
        public string Nintex_No { get; set; }
        public string Red_Receipt { get; set; }
        public int Sequence_Number { get; set; }
        public string Document_Number { get; set; }
        public string Finance_Status { get; set; }
        public int Fiscal_Year { get; set; }
        public string Currency { get; set; }
        public decimal? Amount { get; set; }
        public string Vendor_Number { get; set; }
        public string Account_Number { get; set; }
        public string Vendor_Name { get; set; }
        public string Account_Name { get; set; }
        public DateTime? Run_Date { get; set; }
        public DateTime? Posting_Date { get; set; }
        public string Item_Text { get; set; }
        public string Branch { get; set; }
        public string Bank_Key { get; set; }
        public string Bank_Name { get; set; }
        public string Bene_ID { get; set; }
        public string PO_No { get; set; }
        public string Invoice_No { get; set; }
    }

    public class FinanceConfirmationExcelModel
    {
        public string Nintex_No { get; set; }
        public string Red_Receipt { get; set; }
        public DateTime Payment_Date { get; set; }
        public string Identification { get; set; }
        public int Sequence_Number { get; set; }
        public string Document_Number { get; set; }
        public string Finance_Status { get; set; }
        public int Fiscal_Year { get; set; }
        public string Currency { get; set; }
        public decimal? Amount { get; set; }
        public string Vendor_Number { get; set; }
        public string Account_Number { get; set; }
        public string Vendor_Name { get; set; }
        public string Account_Name { get; set; }
        public DateTime? Run_Date { get; set; }
        public DateTime? Posting_Date { get; set; }
        public string Item_Text { get; set; }
        public string Branch { get; set; }
        public string Bank_Key { get; set; }
        public string Bank_Name { get; set; }
        public string Bene_ID { get; set; }
        public string PO_No { get; set; }
        public string Invoice_No { get; set; }
    }

    public class ProcessPaymentModel
    {
        public string ID { get; set; }
        public string Bank_Name { get; set; }
        public string Red_Receipt { get; set; }
        public string Nintex_No { get; set; }
    }
    class GeneralModel
    {
    }
}