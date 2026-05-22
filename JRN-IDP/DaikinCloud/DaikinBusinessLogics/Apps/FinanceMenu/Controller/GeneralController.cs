using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.FinanceMenu.Model;
using Daikin.BusinessLogics.Apps.FinanceMenu.Repository;
using Daikin.BusinessLogics.Apps.FinanceMenu.SharePointService;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reporting.WebForms;

namespace Daikin.BusinessLogics.Apps.FinanceMenu.Controller
{
    public class GeneralController
    {
        private readonly DatabaseManager db;
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        private readonly SharePointManager sp = new SharePointManager();
        private readonly FinanceMenuSharePointService service;
        private readonly string siteUrl = Utility.SpSiteUrl;
        private readonly FinanceMenuRepository repo;

        public GeneralController()
        {
            db = new DatabaseManager();
            repo = new FinanceMenuRepository(db);
            service = new FinanceMenuSharePointService(sp);
        }

        public DataTable GetDetailMenuByCode(string menu_code)
        {
            DataTable dtMenu = new DataTable();
            using (SqlConnection conn = new SqlConnection(db.GetSQLConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "[usp_MasterModule_GetByModuleCode]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Module_Code", menu_code);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dtMenu.Load(dr);
                    }

                }
                conn.Close();
            }
            return dtMenu;
        }

        public void InsertToFinancePayment(string Module_ID, int Header_ID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[usp_Utility_InsertToFinancePayment]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_ID", Module_ID);
                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<string> GetMappingBranchCurrentLogin(string CurrentLogin)
        {
            var listBranch = repo.GetMappingBranchByCurrentLogin(CurrentLogin).GetAwaiter().GetResult();
            return listBranch;
        }

        public List<OptionModel> GetMappingBranchByUser(string CurrentLogin)
        {
            return repo.GetMappingBranchByUser(CurrentLogin).GetAwaiter().GetResult();
        }

        public List<OptionModel> GetMappingBranchByCurrentUser(string CurrentLogin)
        {
            List<OptionModel> listOption = new List<OptionModel>();
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterMappingBreanch_GetListBranchByCurrentLogin";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PIC_Account", CurrentLogin);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dt.Rows)
                {
                    OptionModel data = new OptionModel();
                    data.Code = Utility.GetStringValue(row, "Branch");
                    data.Name = Utility.GetStringValue(row, "Branch");
                    listOption.Add(data);
                }
                return listOption;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<OptionModel> GetMenuList(SPWeb web, int UserId)
        {
            List<OptionModel> list = sp.GetMultipleValues(web, UserId, "Module");
            return list;
        }

        public List<string> GetBranchFinance(int SPUserId, string AttributeName)
        {
            return service.GetBranchFinance(SPUserId, AttributeName);
        }

        public List<OptionModel> GetMenuList(int userId)
        {
            return service.GetMultipleValues(siteUrl, userId, "Module");
        }

        public string GeneratePrintNumber(List<FinanceMenuModel> list, string CurrentLoginName)
        {
            try
            {
                string Print_No = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + CurrentLoginName;
                db.OpenConnection(ref conn, true);
                foreach (FinanceMenuModel data in list)
                {
                    db.cmd.CommandText = "usp_FinancePayment_PaymentVoucher";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "SAP_Payment_No", data.SAP_Payment_No);
                    db.AddInParameter(db.cmd, "Processed_By", CurrentLoginName);
                    db.AddInParameter(db.cmd, "Print_No", Print_No);
                    db.cmd.ExecuteNonQuery();

                }
                db.CloseConnection(ref conn, true);
                return Print_No;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        private ReportPDFModel GetReportData(string Type)
        {
            var reportData = new ReportPDFModel();
            if (Type.ToUpper() == "DETAIL")
            {
                reportData.Dataset = "SchedulePaymentReportDS_TestCopy";
                reportData.RDLC = "SchedulePaymentReportDetail_New";
                reportData.StoredProcedure = "usp_ScheduledPaymentSubDetail_GetList";
            }
            else if (Type.ToUpper() == "SUMMARY")
            {
                reportData.RDLC = "SchedulePaymentReportSummary_New";
                reportData.Dataset = "SchedulePaymentSumDS_TestCopy";
                reportData.StoredProcedure = "usp_ScheduledPaymentDetails_GetList";
            }
            else
            {
                reportData.RDLC = "SchedulePaymentReportPivot_New";
                reportData.Dataset = "dsPivot";
                reportData.StoredProcedure = "usp_ScheduledPayment_PivotTransaction";
            }
            return reportData;
        }

        public void GenerateRDLCBytesReport(DateTime PaymentDate, string Branch, string BankName, string ServerPath, int ItemId, string Tipe, string Title = "")
        {
            if (ItemId <= 0) throw new ArgumentNullException("Item id cannot null");

            string HeaderNo = BankName.ToUpper().Contains("MUFG") ? Title : BankName + PaymentDate.ToString("ddMMyyyy");
            string ReferenceCode = PaymentDate.ToString("yyyyMMdd") + "_" + BankName + "_" + Tipe;
            var reportData = GetReportData(Tipe);
            var dt = repo.GetReportData(reportData.StoredProcedure, HeaderNo).GetAwaiter().GetResult();

            string docName = ReferenceCode + ".pdf";
            string localPath = ServerPath + "/Exported/" + docName;

            GenerateReport(reportData.RDLC, reportData.Dataset, ServerPath, ReferenceCode, dt);
            sp.UploadFileInCustomList("Scheduled Payment", ItemId, localPath, siteUrl);
        }

        private void GenerateReport(string RDLC, string Dataset, string ServerPath, string ReferenceCode, DataTable ReportData)
        {
            ReportViewer report = new ReportViewer
            {
                SizeToReportContent = true,
                ProcessingMode = ProcessingMode.Local
            };
            report.LocalReport.ReportPath = ServerPath + "RDLC\\" + RDLC + ".RDLC";
            report.LocalReport.DataSources.Clear();
            report.LocalReport.DataSources.Add(new ReportDataSource(Dataset, ReportData));

            Warning[] warnings;
            string[] streamIds;
            string contentType;
            string encoding;
            string extension;

            byte[] bytes = report.LocalReport.Render("PDF", null, out contentType, out encoding, out extension, out streamIds, out warnings);
            string docName = ReferenceCode + ".pdf";
            string localPath = ServerPath + "/Exported/" + docName;
            System.IO.File.WriteAllBytes(localPath, bytes);
        }

        public List<OptionModel> BindingMasterDatabase(string TableName, string codeColumn, string displayColumn, string firstOptionText)
        {
            List<OptionModel> listOption = new List<OptionModel>();
            using (var conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.usp_GetMasterData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "TableName", Value = TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt = new DataTable();
                        dt.Load(reader);
                        foreach (DataRow row in dt.Rows)
                        {
                            listOption.Add(new OptionModel
                            {
                                Code = Utility.GetStringValue(row, codeColumn),
                                Name = Utility.GetStringValue(row, displayColumn)
                            });
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(firstOptionText))
            {
                listOption.Add(new OptionModel
                {
                    Code = "",
                    Name = firstOptionText,
                    Selected = true
                });
            }
            return listOption.OrderBy(x => x.Name).ToList();
        }

        public List<OptionModel> BindingMasterSPList(string ListName, string codeColumn, string displayColumn, string firstOptionText)
        {
            return service.BindingMasterSPList(ListName, codeColumn, displayColumn, firstOptionText);
        }

        public List<PPJKOptionModel> BindingMasterSPListPPJK()
        {
            List<PPJKOptionModel> listOption = new List<PPJKOptionModel>();
            SPWeb web = SPContext.Current.Web;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                SPList list = web.Lists["Master PPJK"];
                SPListItemCollection items = list.Items;
                dt = new DataTable();
                dt = items.GetDataTable();
            });
            PPJKOptionModel data = new PPJKOptionModel();
            data.Code = "";
            data.Name = "Please Select";
            data.Curr = "";
            data.Category = "";
            data.Bank_Account_No = "";
            data.Bank_Key = "";
            data.Bank_Name = "";
            listOption.Add(data);
            foreach (DataRow row in dt.Rows)
            {
                data = new PPJKOptionModel();
                data.Code = Utility.GetStringValue(row, "Code");
                data.Name = data.Code + " - " + Utility.GetStringValue(row, "Bank_x0020_Account_x0020_Name");
                data.Curr = Utility.GetStringValue(row, "Curr");
                data.Category = Utility.GetStringValue(row, "Category");
                data.Bank_Account_No = Utility.GetStringValue(row, "Title");
                data.Bank_Key = Utility.GetStringValue(row, "Bank_x0020_Key");
                listOption.Add(data);
            }
            return listOption;
        }

        public List<ServiceCostConditionModel> BindingMasterSPListCondition(bool isSelected)
        {
            List<ServiceCostConditionModel> listOption = new List<ServiceCostConditionModel>();
            SPWeb web = SPContext.Current.Web;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                SPList list = web.Lists["Master Service Cost Condition"];
                SPListItemCollection items = list.Items;
                dt = new DataTable();
                dt = items.GetDataTable();
            });

            listOption.Add(new ServiceCostConditionModel
            {
                Code = "",
                Name = "Please Select",
                Selected = isSelected,
                Title = "",
                ID = "0"
            });

            foreach (DataRow row in dt.Rows)
            {
                listOption.Add(new ServiceCostConditionModel
                {
                    Code = Utility.GetStringValue(row, "ID"),
                    Name = Utility.GetStringValue(row, "Combine"),
                    ID = Utility.GetStringValue(row, "ID"),
                    Title = Utility.GetStringValue(row, "Title"),
                    Selected = false
                });
            }
            return listOption;
        }

        public List<FinanceMenuModel> ListData(string SearchBy, string Keywords, string BranchName, int pageIndex, int pageSize,
                                               int DocNoFrom, int DocNoTo, string StartDate, string EndDate,
                                               string SPSiteUrl, string Status, string ModuleId, string DueDate, string ProcurementDepartment,
                                               out int recordCount, out decimal GrandTotal)
        {
            dt = new DataTable();

            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_FinancePayment_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "pKeywords", Keywords);
                db.AddInParameter(db.cmd, "pSearchBy", SearchBy);
                db.AddInParameter(db.cmd, "pBranchName", BranchName);
                db.AddInParameter(db.cmd, "pPageIndex", pageIndex);
                db.AddInParameter(db.cmd, "pPageSize", pageSize);
                db.AddInParameter(db.cmd, "pStatus", Status);
                db.AddInParameter(db.cmd, "pDocNoFrom", DocNoFrom);
                db.AddInParameter(db.cmd, "pDocNoTo", DocNoTo);
                db.AddInParameter(db.cmd, "pStartDate", StartDate);
                db.AddInParameter(db.cmd, "pEndDate", EndDate);
                db.AddInParameter(db.cmd, "pDueDate", DueDate);
                db.AddInParameter(db.cmd, "pModuleId", ModuleId);
                db.AddInParameter(db.cmd, "pProcurementDepartment", ProcurementDepartment);
                db.AddOutParameter(db.cmd, "@outRecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@outGrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);

                recordCount = Convert.ToInt32(db.cmd.Parameters["@outRecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@outGrandTotal"].Value);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
            }
            catch
            {
                recordCount = 0;
                GrandTotal = 0;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
            return Utility.ConvertDataTableToList<FinanceMenuModel>(dt);
        }

        public List<FinanceMenuModel> FinancePaymentListData(FinanceMenuSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            var items = repo.FinancePaymentListData(model).GetAwaiter().GetResult();
            RecordCount = 0;
            GrandTotal = 0;
            if (items != null && items.Count > 0)
            {
                RecordCount = items[0].Record_Count;
                GrandTotal = items[0].Grand_Total;
            }
            return items;
        }

        public List<FinanceMenuModel> ScheduledPaymentList(int PageIndex, int PageSize, string TransNo, string NintexNo, out int recordCount, out decimal grandTotal)
        {
            var items = repo.ScheduledPaymentList(PageIndex, PageSize, TransNo, NintexNo).GetAwaiter().GetResult();
            recordCount = items[0].Record_Count;
            grandTotal = items[0].Grand_Total;
            return items;
        }

        public List<FinanceMenuModel> GetFinanceConfirmation(int pageIndex, string identification,
            string bankName, DateTime? paymentDateFrom, DateTime? paymentDateTo, string BranchName, int pExcel, out int recordCount, out decimal grandTotal)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_FinancePayment_GetFinanceConfirmationList";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.AddInParameter(db.cmd, "pBankName", bankName);
                db.AddInParameter(db.cmd, "pPaymentDateFrom", paymentDateFrom);
                db.AddInParameter(db.cmd, "pPaymentDateTo", paymentDateTo);
                db.AddInParameter(db.cmd, "pIdentification", identification);
                db.AddInParameter(db.cmd, "pBranch", BranchName);
                db.AddInParameter(db.cmd, "pPageIndex", pageIndex);
                db.AddInParameter(db.cmd, "pExcel", pExcel);
                db.AddOutParameter(db.cmd, "@outRecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@outGrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);

                recordCount = Convert.ToInt32(db.cmd.Parameters["@outRecordCount"].Value);
                grandTotal = Convert.ToDecimal(db.cmd.Parameters["@outGrandTotal"].Value);

                db.CloseDataReader(reader);
            }
            catch
            {
                recordCount = 0;
                grandTotal = 0;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
            return Utility.ConvertDataTableToList<FinanceMenuModel>(dt);
        }

        public List<FinanceMenuExcelModel> GetFinanceMenuExcel()
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_FinancePayment_GetFinancePaymentMenuExcel";
                db.cmd.CommandType = CommandType.StoredProcedure;

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
            return Utility.ConvertDataTableToList<FinanceMenuExcelModel>(dt);
        }

        public List<FinanceConfirmationExcelModel> GetFinanceConfirmationExcel(string identification, string bankName, DateTime? paymentDateFrom, DateTime? paymentDateTo)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_FinancePayment_GetFinanceConfirmationExcel";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.AddInParameter(db.cmd, "pBankName", bankName);
                db.AddInParameter(db.cmd, "pPaymentDateFrom", paymentDateFrom);
                db.AddInParameter(db.cmd, "pPaymentDateTo", paymentDateTo);
                db.AddInParameter(db.cmd, "pIdentification", identification);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
            return Utility.ConvertDataTableToList<FinanceConfirmationExcelModel>(dt);
        }

        public void ConfirmPayments(List<FinanceMenuModel> idList, string FinanceStatus)
        {
            SPSite spSite = new SPSite(Utility.SpSiteUrl);
            SPWeb web = spSite.OpenWeb();
            web.AllowUnsafeUpdates = true;
            SPList list = web.Lists["Claim Reimbursement"];
            try
            {
                db.OpenConnection(ref conn, true);
                foreach (FinanceMenuModel fm in idList)
                {
                    db.cmd.CommandText = "dbo.usp_FinancePayment_ConfirmPayment";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "Nintex_No", fm.Nintex_No);
                    db.AddInParameter(db.cmd, "Module_Id", fm.Module_ID);
                    db.AddInParameter(db.cmd, "Status", FinanceStatus);

                    db.cmd.ExecuteNonQuery();

                }
                db.CloseConnection(ref conn, true);
                web.AllowUnsafeUpdates = false;

            }
            finally
            {
                db.CloseConnection(ref conn);
            }

        }

        public string GenerateXMLPaymentDetail(List<FinanceMenuModel> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (FinanceMenuModel data in idList)
            {
                xml += "<Item><nintex_no type=\"System.String\">" + data.Nintex_No + "</nintex_no><red_receipt type=\"System.String\" >" + data.Red_Receipt + "</red_receipt>";
                xml += "<sequence_number type=\"System.String\">" + seq.ToString() + "</sequence_number><document_number type=\"System.String\">" + data.Document_Number + "</document_number>";
                xml += "<fiscal_year type=\"System.String\" >" + data.Fiscal_Year.ToString() + "</fiscal_year>";
                xml += "<currency type=\"System.String\">" + data.Currency + "</currency><amount type=\"System.String\">" + data.Amount.ToString() + "</amount>";
                xml += "<vendor_number type=\"System.String\">" + data.Vendor_Number + "</vendor_number>";
                xml += "<vendor_name type=\"System.String\">" + data.Vendor_Name + "</vendor_name>";
                xml += "<account_name type=\"System.String\">" + data.Account_Name + "</account_name><account_number type=\"System.String\">" + data.Account_Number + "</account_number>";
                xml += "<run_date type=\"System.DateTime\">" + data.Run_Date.ToString() + "</run_date><posting_date type=\"System.DateTime\">" + data.Posting_Date + "</posting_date>";
                xml += "<item_text type=\"System.String\"></item_text ><branch type=\"System.String\">" + data.Branch + "</branch>";
                xml += "<bank_name type=\"System.String\">" + data.Bank_Name + "</bank_name><bank_key type=\"System.String\">" + data.Bank_Key + "</bank_key>";
                xml += "<bene_id type=\"System.String\">" + data.Bene_ID + "</bene_id><po_no type=\"System.String\"></po_no><invoice_no type=\"System.String\"></ invoice_no></Item>";
                seq++;
            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public void RescheduleApproval(List<FinanceMenuModel> list)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                foreach (FinanceMenuModel data in list)
                {
                    db.cmd.CommandText = "dbo.usp_SchedulePaymentDetail_Reschedule";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ID", data.ID);
                    db.AddInParameter(db.cmd, "Nintex_No", data.Nintex_No);
                    db.AddInParameter(db.cmd, "Module_ID", data.Module_ID);

                    db.cmd.ExecuteNonQuery();

                }
                db.CloseConnection(ref conn, true);
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }
        public void SubmitScheduleApproval(string SPSiteUrl, DateTime paymentDate, string branch, string bankName, decimal Grand_Total, string tipe, string headerNo)
        {
            string currentLogin = sp.GetCurrentLoginFullName(SPSiteUrl);
            string currentLoginEmail = sp.GetCurrentLoginEmail(SPSiteUrl);
            string transNo = bankName + paymentDate.ToString("ddMMyyyy");
            if (bankName.ToUpper().Contains("MUFG")) transNo = headerNo.Trim();
            if (tipe.ToUpper() != "REGULAR") transNo = "ID-" + transNo;
            int itemId = 0;
            string financeStatus = "";

            var existingItem = service.GetExistingScheduledPaymentItem(transNo);
            if (existingItem.Count > 0)
            {
                foreach (SPListItem item in existingItem)
                {
                    financeStatus = item["Status"].ToString();
                    if (financeStatus == "99") throw new Exception("This scheduled payment is already on process");
                    itemId = item.ID;
                }
            }

            itemId = service.SaveSPListScheduledPayment(itemId, Grand_Total, transNo, paymentDate, bankName, tipe, branch, SPSiteUrl);
            repo.ScheduledPaymentUpdateItemId(bankName, branch, paymentDate, itemId, tipe, currentLogin, headerNo).GetAwaiter().GetResult();
        }



        public WebModel<String> ProcessPayments2(List<FinanceMenuModel> idList, string identification, string bankName, DateTime paymentDate, string SPSiteUrl, string CurrentLogin, string branch)
        {
            WebModel<String> result = new WebModel<String>();
            int itemChanged = 0;
            int itemInserted = 0;

            string TransNo = bankName + paymentDate.ToString("ddMMyyyy");

            try
            {
                #region Check if already processed
                var listNintexNo = idList.Select(s => s.Nintex_No).ToList();
                var strListNintexNo = string.Join(";", listNintexNo);
                var processedFinancePayment = repo.GetProcessedFinancePayment(strListNintexNo).GetAwaiter().GetResult();
                if (processedFinancePayment.Count > 0)
                {
                    var sb = new StringBuilder("This below transaction no has been already processed:");
                    foreach (var payment in processedFinancePayment)
                    {
                        sb.AppendLine(
                            $"{payment.Nintex_No} - {payment.Payment_Date.ToString("dd MMM yyyy")}"
                        );
                    }

                    result.Message = sb.ToString();
                    result.Code = 500;
                    result.CountItem = 0;
                    return result;
                }
                #endregion



                #region Check If already running

                var runningScheduledPayment = repo.GetRunningScheduledPayment(bankName, paymentDate).GetAwaiter().GetResult();
                if (runningScheduledPayment.Count > 0)
                {
                    var financeStatus = runningScheduledPayment[0].Status;
                    if (financeStatus == "9")
                    {
                        result.Message = "Cannot process this payment due to on this " + paymentDate.ToString("dd-MMM-yyyy") + " is already on approval process";
                        result.Code = 500;
                        result.CountItem = 0;
                        return result;
                    }
                }
                #endregion

                #region Save DB
                using (var con = new SqlConnection(Utility.GetSqlConnection()))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        var saveHeaderResponse = repo.SaveScheduledPaymentHeader(con, trans, bankName, branch, paymentDate, identification, CurrentLogin).GetAwaiter().GetResult();
                        foreach (var data in idList)
                        {
                            repo.SaveScheduledPaymentDetail(con, trans, data, saveHeaderResponse.Header_ID, identification, CurrentLogin,
                                saveHeaderResponse.Transaction_No, paymentDate).GetAwaiter().GetResult();
                        }
                        trans.Commit();
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                result.Message = ex.Message;
                result.Code = 500;
                result.CountItem = 0;
                return result;
            }

            result.Code = 200;
            result.Message = "OK";
            result.Data = itemInserted + " Item inserted " + itemChanged + " Item changed";
            return result;
        }

        Utility util = new Utility();
    }
}
