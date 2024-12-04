using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class IDModel
    {
        public int ID { get; set; }
    }


    public class SupplierModel
    {
        public string SupplierSite { get; set; }
        public string SupplierBusinessUnit { get; set; }
    }

    public class PayloadModel
    {
        public string InvoiceNumber { get; set; } = "";
        public string InvoiceCurrency { get; set; }
        public string InvoiceAmount { get; set; }
        public string InvoiceDate { get; set; }
        public string BusinessUnit { get; set; }
        public string Supplier { get; set; }
        public string SupplierSite { get; set; }
        public string Requester { get; set; }
        public string InvoiceGroup { get; set; }
        public string Description { get; set; }
        public string PaymentMethodCode { get; set; }
        public string TermsDate { get; set; }
        public List<AttachmentModel> attachments { get; set; }
        public List<InvoiceInstallmentModel> invoiceInstallments { get; set; }
        public List<InvoiceLineModel> invoiceLines { get; set; }
        public List<InvoiceDffModel> invoiceDff { get; set; }
    }

    public class InvoiceHeaderModel
    {
        //public int HeaderID { get; set; }
        //public string Invoice_ID { get; set; }
        public string BusinessUnit { get; set; }
        //public string PurchaseOrderNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceAmount { get; set; }
        public string InvoiceDate { get; set; }
        public string Supplier { get; set; }
        public string SupplierSite { get; set; }
        public string InvoiceCurrency { get; set; }
        public string PaymentCurrency { get; set; }
        public string Description { get; set; }
        public string InvoiceType { get; set; }
        public string LegalEntity { get; set; }
        public string PaymentTerms { get; set; }
        public string TermsDate { get; set; }
        public string InvoiceReceivedDate { get; set; }
        public string AccountingDate { get; set; }
        public string ConversionRateType { get; set; }
        public string ConversionDate { get; set; }
        public string ConversionRate { get; set; }
        public List<InvoiceLineModel> invoiceLines { get; set; }
        public List<AttachmentModel> attachments { get; set; }
        //public string Tax_Invoice_No { get; set; }
        //public string Tax_Invoice_Date { get; set; }
        //public string Subtotal { get; set; }
        //public string Created_By { get; set; }
        //public DateTime Created_Date { get; set; }
        //public string Modified_By { get; set; }
        //public DateTime Modified_Date { get; set; }
    }

    public class AttachmentModel
    {
        public string Type { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; }
        public string FileUrl { get; set; }
        public string FileContents { get; set; }
    }

    public class InvoiceDffModel
    {

    }

    public class InvoiceInstallmentModel
    {
        public int InstallmentNumber { get; set; }
        public string DueDate { get; set; }
        public decimal GrossAmount { get; set; }
        public List<InvoiceInstallmentDffModel> invoiceInstallmentDff { get; set; } = new List<InvoiceInstallmentDffModel>();
    }

    public class InvoiceInstallmentDffModel
    {

    }

    public class InvoiceLineModel
    {
        public string LineNumber { get; set; }
        public string Item { get; set; }
        public string LineAmount { get; set; }
        public string Quantity { get; set; }
        //public List<InvoiceDistributionModel> invoiceDistributions { get; set; } = new List<InvoiceDistributionModel>();
        public string LineType { get; set; }
        public string ReceiptNumber { get; set; }
        public int ReceiptLineNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int PurchaseOrderLineNumber { get; set; }
        public int PurchaseOrderScheduleLineNumber { get; set; }
        public string UnitPrice { get; set; }
        //public string ItemDescription { get; set; }
        public string Description { get; set; }
    }
    public class InvoiceDistributionModel
    {
        public string DistributionLineNumber { get; set; }
        public string DistributionLineType { get; set; }
        public string DistributionCombination { get; set; }
        public string DistributionAmount { get; set; }
        //public string UOMCode { get; set; }
        //public string UOM {  get; set; }
    }

}
