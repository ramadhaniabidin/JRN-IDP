using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.FinanceMenu.Model;
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

namespace Daikin.BusinessLogics.Apps.FinanceMenu.Controller
{
    public class GeneralController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();

        public DataTable GetDetailMenuByCode(string menu_code)
        {
            DataTable dtMenu = new DataTable();
            try
            {
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
            catch (Exception ex)
            {
                throw ex;
            }
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
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public List<OptionModel> GetMappingBranchByUser(string CurrentLogin)
        {
            List<OptionModel> listOption = new List<OptionModel>();
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterFinanceTeam_GetListBranchByCurrentLogin";
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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<OptionModel> GetMenuList(SPWeb web, int UserId)
        {
            try
            {
                List<OptionModel> list = sp.GetMultipleValues(web, UserId, "Module");
                return list;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //usp_FinancePayment_PaymentVoucher
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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<OptionModel> BindingMasterDatabase(string TableName, string codeColumn, string displayColumn, string firstOptionText)
        {
            try
            {
                List<OptionModel> listOption = new List<OptionModel>();
                using(var conn = new SqlConnection(Utility.GetSqlConnection()))
                {
                    conn.Open();
                    //var query = $"SELECT * FROM {TableName}";
                    //var query = $"SELECT * FROM {TableName}";
                    using (var cmd = new SqlCommand("dbo.usp_GetMasterData", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "TableName", Value = TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                        using(var reader = cmd.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            foreach(DataRow row in dt.Rows)
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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<OptionModel> BindingMasterSPList(string ListName, string codeColumn, string displayColumn, string firstOptionText)
        {
            List<OptionModel> listOption = new List<OptionModel>();
            try
            {
                SPWeb web = SPContext.Current.Web;
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {

                    SPList list = web.Lists[ListName];
                    SPListItemCollection items = list.Items;
                    dt = new DataTable();
                    dt = items.GetDataTable();
                });

                if (!string.IsNullOrEmpty(firstOptionText))
                {
                    listOption.Add(new OptionModel
                    {
                        Code = "", Name = firstOptionText, Selected = true
                    });
                }

                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new OptionModel
                    {
                        Code = Utility.GetStringValue(row, codeColumn), Name = Utility.GetStringValue(row, displayColumn)
                    });
                }
                return listOption.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PPJKOptionModel> BindingMasterSPListPPJK()
        {
            List<PPJKOptionModel> listOption = new List<PPJKOptionModel>();
            try
            {
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ServiceCostConditionModel> BindingMasterSPListCondition(bool isSelected)
        {
            List<ServiceCostConditionModel> listOption = new List<ServiceCostConditionModel>();
            try
            {
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
                    Code = "", Name = "Please Select", Selected = isSelected, Title = "", ID = "0"
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FinanceMenuModel> ListData(string SearchBy, string Keywords, string BranchName, int pageIndex, int pageSize,
                                               int DocNoFrom, int DocNoTo, string StartDate, string EndDate,
                                               string SPSiteUrl, string Status, string ModuleId, string DueDate, string ProcurementDepartment,
                                               out int recordCount, out decimal GrandTotal)
        {
            dt = new DataTable();

            try
            {
                //UserProfile profile = GetUserProfile(SPSiteUrl);
                //branch = Convert.ToString(profile["Office"].Value);

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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                recordCount = 0;
                GrandTotal = 0;
                throw ex;
            }
            return Utility.ConvertDataTableToList<FinanceMenuModel>(dt);
        }

        public List<FinanceMenuModel> ScheduledPaymentList(int PageIndex, int PageSize, string TransNo, string NintexNo, out int recordCount, out decimal grandTotal)
        {
            List<FinanceMenuModel> list = new List<FinanceMenuModel>();
            dt = new DataTable();

            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ScheduledPayment_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_No", TransNo);
                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "PageSize", PageSize);
                db.AddInParameter(db.cmd, "Nintex_No", NintexNo);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);

                recordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                grandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<FinanceMenuModel>(dt);

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<FinanceMenuModel> GetFinanceConfirmation(int pageIndex, string identification,
            string bankName, DateTime? paymentDateFrom, DateTime? paymentDateTo, string BranchName, int pExcel, out int recordCount, out decimal grandTotal)
        {
            dt = new DataTable();
            //string branch = "";

            try
            {
                //UserProfile profile = GetUserProfile(SPSiteUrl);
                //branch = Convert.ToString(profile["Office"].Value);


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
            catch (Exception ex)
            {
                recordCount = 0;
                grandTotal = 0;
                throw ex;
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
            catch (Exception ex)
            {
                throw ex;
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
            catch (Exception ex)
            {
                throw ex;
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

                #region Run Workflow for Paid Notification (Sementara disable karena error)
                //foreach (FinanceMenuModel fm in idList)
                //{
                //    if (FinanceStatus == "7")
                //    {
                //        SPSecurity.RunWithElevatedPrivileges(delegate ()
                //        {
                //            SPListItem cnritem = list.GetItemById(fm.Transaction_List_Item_ID);
                //            cnritem["Finance Status"] = 7;
                //            cnritem["Form Status"] = "Start";
                //            cnritem.Update();
                //        });
                //    }
                //}
                #endregion

                web.AllowUnsafeUpdates = false;

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }

        }

        public string GenerateXMLPaymentDetail(List<FinanceMenuModel> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (FinanceMenuModel data in idList)
            {
                //\"Hello World\"

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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public void SubmitScheduleApproval(string SPSiteUrl, DateTime paymentDate, string branch, string bankName, decimal Grand_Total, string tipe)
        {
            #region Trigger Workflow
            string TransNo = bankName + paymentDate.ToString("ddMMyyyy");

            if (tipe.ToUpper() != "REGULAR")
            {
                TransNo = "AD-" + TransNo;
            }

            SPSite spSite = new SPSite(SPSiteUrl);
            SPWeb web = spSite.OpenWeb();
            string CurrentLogin = sp.GetCurrentLoginFullName(SPSiteUrl);
            string CurrentLoginEmail = sp.GetCurrentLoginEmail(SPSiteUrl);

            //perlu tambah validasi jika status scheduled jgn jalanin lagi

            //end of
            int ListItemId = 0;

            #region Check If SP List already inserted
            SPList listCheck = web.Lists["Scheduled Payment"];
            var q = new SPQuery()
            {
                Query = @"<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>" + TransNo + "</Value></Eq></Where>"
            };

            var r = listCheck.GetItems(q);
            string FinanceStatus = "";
            if (r.Count > 0)
            {
                foreach (SPListItem item in r)
                {
                    FinanceStatus = item["Status"].ToString();
                    ListItemId = item.ID;
                }
                if (FinanceStatus == "99") throw new Exception("This scheduled payment is already on process");
            }

            /*
            db.OpenConnection(ref conn);
            db.cmd.CommandText = "dbo.usp_SchedulePaymentHeader_GetItemId";
            db.cmd.CommandType = CommandType.StoredProcedure;
            db.cmd.Parameters.Clear();

            db.AddInParameter(db.cmd, "bank", bankName);
            db.AddInParameter(db.cmd, "payment_date", paymentDate);

            reader = db.cmd.ExecuteReader();
            dt.Load(reader);
            db.CloseDataReader(reader);

            foreach (DataRow row in dt.Rows)
            {
                ListItemId = Utility.GetIntValue(row, "Item_ID");
            }

            db.CloseConnection(ref conn);
            */
            #endregion


            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                SPList list = web.Lists["Scheduled Payment"];
                web.AllowUnsafeUpdates = true;

                SPListItem item;

                if (ListItemId == 0)
                {
                    item = list.Items.Add();
                    item["Title"] = TransNo;
                    item["Payment Date"] = paymentDate;
                    item["Payment Date Text"] = paymentDate.ToString("yyyy-MM-dd");
                    item["Url Trans List"] = SPSiteUrl + "/_layouts/15/Daikin.WebApps/FinanceMenu/List.aspx?m_id=9&bank=" + bankName + "&date=" + paymentDate.ToString("yyyy-MM-dd");
                    item["Branch"] = branch;
                    item["Bank"] = bankName;
                    item["Status"] = "9";
                    item["Form Type"] = tipe;
                    item["Current Layer"] = "";
                    item["Requester Name"] = CurrentLogin;
                    item["Requester Email"] = CurrentLoginEmail;
                    item["Grand Total"] = Grand_Total;
                    item["Url Approval Log"] = SPSiteUrl + "/_layouts/15/Daikin.WebApps/ApprovalLog.aspx?Form_No=" +
                                               TransNo + "&Module_Code=M008&Desc=Scheduled%20Payment%20" + paymentDate.ToString("dd%20MMM%20yyyy");
                    //item["Payment Detail"] = GenerateXMLPaymentDetail(idList);
                }
                else
                {
                    item = list.GetItemById(ListItemId);
                    item["Grand Total"] = Grand_Total;
                    item["Status"] = "9";
                    //item["Payment Detail"] = GenerateXMLPaymentDetail(idList);
                }
                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;


            });

            #endregion

            #region Update Item ID
            db.OpenConnection(ref conn, true);

            db.cmd.CommandText = "usp_SchedulePaymentHeader_SaveUpdate";
            db.cmd.CommandType = CommandType.StoredProcedure;
            db.cmd.Parameters.Clear();

            db.AddInParameter(db.cmd, "Bank_Name", bankName);
            db.AddInParameter(db.cmd, "Branch", branch);
            db.AddInParameter(db.cmd, "Payment_Date", paymentDate);
            db.AddInParameter(db.cmd, "Status", "9");
            db.AddInParameter(db.cmd, "Item_ID", ListItemId);
            db.AddInParameter(db.cmd, "Is_AutoDebet", tipe.ToUpper() == "REGULAR" ? 0 : 1);
            db.AddInParameter(db.cmd, "Created_By", CurrentLogin);

            int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());

            db.CloseConnection(ref conn, true);
            #endregion

        }



        public WebModel<String> ProcessPayments2(List<FinanceMenuModel> idList, string identification, string bankName,
            DateTime paymentDate, string SPSiteUrl, string CurrentLogin, string branch)
        {
            WebModel<String> result = new WebModel<String>();
            int itemChanged = 0;
            int itemInserted = 0;

            string TransNo = bankName + paymentDate.ToString("ddMMyyyy");
            decimal Grand_Total = 0;

            try
            {
                #region Check if already processed
                List<string> listNintexNo = new List<string>();
                listNintexNo = idList.Select(s => s.Nintex_No).ToList();
                string strListNintexNo = String.Join(";", listNintexNo.ToArray());

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_FinancePayment_CheckIfProcessed";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Nintex_No", strListNintexNo);
                reader = db.cmd.ExecuteReader();
                DataTable dtNtx = new DataTable();
                dtNtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                string msg = "This below transaction no has been already processed: ";
                foreach (DataRow row in dtNtx.Rows)
                {
                    msg += "\n " + Utility.GetStringValue(row, "Nintex_No") + " - " + Utility.GetDateValue(row, "Payment_Date").ToString("dd MMM yyyy");
                }

                if (dtNtx.Rows.Count > 0)
                {
                    result.Message = msg;
                    result.Code = 500;
                    result.CountItem = 0;
                    return result;
                }

                #endregion



                #region Check If already running


                //DataTable dt = db.GetValueFromSP("dbo.usp_SchedulePaymentHeader_GetItemId", "Transaction_No", TransNo);

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SchedulePaymentHeader_GetItemId";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "bank", bankName);
                db.AddInParameter(db.cmd, "payment_date", paymentDate);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);


                string Status = string.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    Status = Utility.GetStringValue(row, "Status");
                }
                db.CloseConnection(ref conn);

                if (Status == "9")
                {
                    result.Message = "Cannot process this payment due to on this " + paymentDate.ToString("dd-MMM-yyyy") + " is already on approval process";
                    result.Code = 500;
                    result.CountItem = 0;
                    return result;
                }

                #endregion

                #region Save DB
                db.OpenConnection(ref conn, true);

                db.cmd.CommandText = "usp_SchedulePaymentHeader_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Bank_Name", bankName);
                db.AddInParameter(db.cmd, "Branch", branch);
                db.AddInParameter(db.cmd, "Payment_Date", paymentDate);
                db.AddInParameter(db.cmd, "Identification", identification);
                db.AddInParameter(db.cmd, "Status", "4");
                db.AddInParameter(db.cmd, "Item_ID", 0);
                db.AddInParameter(db.cmd, "Created_By", CurrentLogin);

                int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());

                foreach (FinanceMenuModel data in idList)
                {

                    db.cmd.CommandText = "usp_SchedulePaymentDetail_SaveUpdate";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "Header_No", TransNo);
                    db.AddInParameter(db.cmd, "Nintex_No", data.Nintex_No);
                    db.AddInParameter(db.cmd, "Identification", identification);
                    db.AddInParameter(db.cmd, "Red_Receipt", data.Red_Receipt);
                    db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                    db.AddInParameter(db.cmd, "Payment_Date", paymentDate);
                    db.AddInParameter(db.cmd, "Created_By", CurrentLogin);
                    db.AddInParameter(db.cmd, "Status", "4");

                    db.cmd.ExecuteNonQuery();
                    Grand_Total += data.Amount;
                }

                db.CloseConnection(ref conn, true);

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


        //#region Nanti dilanjutkan
        //public void GenerateRDLCBytesReport(DateTime PaymentDate, string Branch, string BankName, string SiteUrl, string ServerPath, int list_item_id, string RDLC_Name, string ReportDS, string Tipe)
        //{
        //    ReportDataSource datasource;
        //    string reference_code = PaymentDate.ToString("yyyyMMdd") + "_" + BankName + "_" + Tipe;
        //    db.OpenConnection(ref conn);
        //    dt = new DataTable();

        //    if (Tipe.ToUpper() == "DETAIL") db.cmd.CommandText = "dbo.usp_ScheduledPaymentSubDetail_GetList";
        //    else if (Tipe.ToUpper() == "PIVOT") db.cmd.CommandText = "dbo.usp_ScheduledPayment_PivotTransaction";
        //    else db.cmd.CommandText = "dbo.usp_ScheduledPaymentDetails_GetList";

        //    db.cmd.CommandType = CommandType.StoredProcedure;
        //    db.cmd.Parameters.Clear();
        //    db.AddInParameter(db.cmd, "Header_No", BankName + PaymentDate.ToString("ddMMyyyy"));
        //    reader = db.cmd.ExecuteReader();
        //    dt.Load(reader);
        //    db.CloseDataReader(reader);
        //    db.CloseConnection(ref conn);


        //    ReportViewer ReportViewer1 = new ReportViewer();
        //    ReportViewer1.SizeToReportContent = true;
        //    ReportViewer1.ProcessingMode = ProcessingMode.Local;
        //    ReportViewer1.LocalReport.ReportPath = ServerPath + "RDLC\\" + RDLC_Name + ".RDLC"; ;
        //    ReportViewer1.LocalReport.DataSources.Clear();

        //    datasource = new ReportDataSource(ReportDS, dt);
        //    ReportViewer1.LocalReport.DataSources.Add(datasource);

        //    //ReportParameter param;
        //    //param = new ReportParameter("paramRptName", data.ReportName);
        //    //try
        //    //{
        //    //    ReportViewer1.LocalReport.SetParameters(param);
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    //theres no param, handle error param
        //    //}

        //    Warning[] warnings;
        //    string[] streamIds;
        //    string contentType;
        //    string encoding;
        //    string extension;
        //    //Export the RDLC Report to Byte Array.

        //    byte[] bytes = ReportViewer1.LocalReport.Render("PDF", null, out contentType, out encoding, out extension, out streamIds, out warnings);
        //    string docName = reference_code + ".pdf";
        //    string localPath = ServerPath + "/Exported/" + docName;
        //    System.IO.File.WriteAllBytes(localPath, bytes);

        //    if (list_item_id > 0)
        //    {
        //        sp.UploadFileInCustomList("Scheduled Payment", list_item_id, localPath, SiteUrl);
        //    }


        //}
        //#endregion


        #region Nyusahin
        //    public WebModel<String> ProcessPayments(List<ProcessPaymentModel> idList, string identification, string bankName, DateTime paymentDate, string SPSiteUrl)
        //    {
        //        // saat ini masih pake itemid
        //        WebModel<String> result = new WebModel<String>();
        //        int itemChanged = 0;
        //        int itemInserted = 0;
        //        bool isTrans = true;
        //        int headerId;
        //        int cnrId;


        //        UserProfile profile = GetUserProfile(SPSiteUrl);
        //        string branch = Convert.ToString(profile["Office"].Value);

        //        SPSite spSite = new SPSite(SPSiteUrl);
        //        SPWeb web = spSite.OpenWeb();
        //        try
        //        {
        //            db.OpenConnection(ref conn, isTrans);

        //            web.AllowUnsafeUpdates = true;
        //            SPList list = web.Lists["Scheduled Payment"];
        //            SPList sploglist = web.Lists["Scheduled Payment Log"];
        //            SPList cnrlist = web.Lists["Claim Reimbursement"];

        //            db.cmd.CommandText = "usp_FinancePayment_ProcessPayment";
        //            db.cmd.CommandType = CommandType.StoredProcedure;
        //            // populate DataTable from your List here
        //            db.AddInParameter(db.cmd, "ID", 0);
        //            db.AddInParameter(db.cmd, "Bank_Name", "");
        //            db.AddInParameter(db.cmd, "Red_Receipt", "");
        //            db.AddInParameter(db.cmd, "Identification", identification);
        //            db.AddInParameter(db.cmd, "Payment_Date", null);
        //            db.AddOutParameter(db.cmd, "@outHeaderId", SqlDbType.Int);
        //            db.AddOutParameter(db.cmd, "@outClaimReimbursementId", SqlDbType.Int);

        //            SPSecurity.RunWithElevatedPrivileges(delegate ()
        //            {
        //                headerId = Convert.ToInt32(db.cmd.Parameters["@outHeaderId"].Value);
        //                //query ke scheduled payment
        //                SPQuery query = new SPQuery();
        //                string paydate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(paymentDate);
        //                query.Query = string.Concat(
        //                    "<Where>",
        //                        "<And>",
        //                            "<Eq>",
        //                                "<FieldRef Name='Branch'/>",
        //                                "<Value Type='Text'>",
        //                                    branch,
        //                                "</Value>",
        //                            "</Eq>",
        //                            "<And>",
        //                                "<Eq>",
        //                                    "<FieldRef Name='Bank'/>",
        //                                    "<Value Type='Text'>",
        //                                        bankName,
        //                                    "</Value>",
        //                                "</Eq>",
        //                                "<Eq>",
        //                                    "<FieldRef Name='Payment_x0020_Date'/>",
        //                                    "<Value Type='DateTime' IncludeTimeValue='False'>",
        //                                        paydate,
        //                                    "</Value>",
        //                                "</Eq>",
        //                            "</And>",
        //                        "</And>",
        //                    "</Where>");

        //                // Select our fields
        //                query.ViewFields = string.Concat(
        //                    //"<FieldRef Name='ID' />",
        //                    "<FieldRef Name='Title' />",
        //                    "<FieldRef Name='Payment_x0020_Date'/>",
        //                    "<FieldRef Name='Bank'/>");
        //                // Get All Data
        //                query.ViewFieldsOnly = false;
        //                // SP Query
        //                SPListItemCollection qItems = list.GetItems(query);
        //                result.CountItem = qItems.Count;

        //                // Get first item
        //                //SPItem item = qItems[0];

        //                // Assign our values
        //                //textboxfname = Convert.ToString(item["firstname"]);

        //                SPListItem item;
        //                if (qItems.Count == 0)
        //                {
        //                    item = list.Items.Add();
        //                    item["Title"] = bankName + paymentDate.Day + paymentDate.Month + paymentDate.Year;
        //                    item["Payment Date"] = paymentDate;
        //                    item["Branch"] = branch;
        //                    item["Bank"] = bankName;
        //                    item["Status"] = 4;
        //                    itemChanged += 1;
        //                    item.Update();
        //                }
        //                else
        //                {
        //                    //belum bisa rollback
        //                    result.Message += qItems[0].ID;
        //                    item = qItems[0];
        //                    //item = list.GetItemById(qItems[0].ID);//mabok laut
        //                    item["Status"] = 4;
        //                    itemInserted += 1;
        //                    item.Update();
        //                }

        //                db.AddInParameter(db.cmd, "Item_ID", item.ID);
        //                //Query ke DB
        //                foreach (ProcessPaymentModel ppm in idList)
        //                {
        //                    db.ChangeParameter(db.cmd, "ID", ppm.ID);
        //                    db.ChangeParameter(db.cmd, "Bank_Name", bankName);
        //                    db.ChangeParameter(db.cmd, "Payment_Date", paymentDate);
        //                    db.ChangeParameter(db.cmd, "Identification", identification);
        //                    db.ChangeParameter(db.cmd, "Red_Receipt", ppm.Red_Receipt);
        //                    //query ke splist scheduled payment log
        //                    reader = db.cmd.ExecuteReader();
        //                    dt.Load(reader);

        //                    cnrId = Convert.ToInt32(db.cmd.Parameters["@outClaimReimbursementId"].Value);
        //                    //query ke claim n reimbursement
        //                    SPListItem cnritem = cnrlist.GetItemById(cnrId);
        //                    cnritem["Form_x0020_Status"] = "Start";
        //                    cnritem["Finance_x0020_Payment_x0020_Proc"] = true;
        //                    cnritem.Update();

        //                    SPListItem splogitem = sploglist.Items.Add();
        //                    splogitem["Nintex No"] = dt.Rows[0]["Nintex_No"];
        //                    splogitem["Red Receipt"] = dt.Rows[0]["Red_Receipt"];
        //                    splogitem["Sequence Number"] = dt.Rows[0]["Sequence_Number"];
        //                    splogitem["Document Number"] = dt.Rows[0]["Document_Number"];
        //                    splogitem["Fiscal Year"] = Convert.ToInt32(dt.Rows[0]["Fiscal_Year"]);
        //                    splogitem["Currency"] = dt.Rows[0]["Currency"];
        //                    splogitem["Amount"] = Convert.ToInt32(dt.Rows[0]["Amount"]);
        //                    splogitem["Vendor Number"] = dt.Rows[0]["Vendor_Number"];
        //                    splogitem["Account Number"] = dt.Rows[0]["Account_Number"];
        //                    splogitem["Vendor Name"] = dt.Rows[0]["Vendor_Name"];
        //                    splogitem["Account Name"] = dt.Rows[0]["Account_Name"];
        //                    if (dt.Rows[0]["Run_Date"] != null)
        //                        splogitem["Run Date"] = Convert.ToDateTime(dt.Rows[0]["Run_Date"]);
        //                    if (dt.Rows[0]["Posting_Date"] != null)
        //                        splogitem["Posting Date"] = Convert.ToDateTime(dt.Rows[0]["Posting_Date"]);
        //                    splogitem["Item Text"] = dt.Rows[0]["Item_Text"];
        //                    splogitem["Branch"] = dt.Rows[0]["Branch"];
        //                    splogitem["Bank Key"] = dt.Rows[0]["Bank_Key"];
        //                    splogitem["Bank Name"] = dt.Rows[0]["Bank_Name"];
        //                    splogitem["Bene ID"] = dt.Rows[0]["Bene_ID"];
        //                    splogitem["PO No"] = dt.Rows[0]["PO_No"];
        //                    splogitem["Invoice No"] = dt.Rows[0]["Invoice_No"];
        //                    splogitem["Identification"] = dt.Rows[0]["Identification"];
        //                    splogitem.Update();
        //                    dt = new DataTable();
        //                }


        //                /*db.ClearParameter(db.cmd);

        //                db.cmd.CommandText = "usp_ScheduledPaymentHeader_Update";
        //                db.cmd.CommandType = CommandType.StoredProcedure;
        //                // populate DataTable from your List here
        //                db.AddInParameter(db.cmd, "pId", headerId);
        //                db.AddInParameter(db.cmd, "pItemId", item.ID);

        //                db.cmd.ExecuteNonQuery();*/


        //});
        //            //db.cmd.ExecuteScalar();
        //        }
        //        catch (Exception ex)
        //        {
        //            isTrans = false;
        //            result.Message = ex.Message;
        //            result.Code = 500;
        //            result.CountItem = 0;
        //            return result;
        //        }
        //        finally
        //        {
        //            web.Close();
        //            spSite.Close();
        //            db.CloseDataReader(reader);
        //            db.CloseConnection(ref conn, isTrans);
        //        }

        //        result.Code = 200;
        //        result.Message = "OK";
        //        result.Data = itemInserted + " Item inserted " + itemChanged + " Item changed";
        //        return result;
        //    }
        #endregion

        //private UserProfile GetUserProfile(string SPSiteUrl)
        //{
        //    SPSite spSite = new SPSite(Utility.SpSiteUrl);
        //    SPWeb web = spSite.OpenWeb();
        //    SPUser user = web.CurrentUser;

        //    SPServiceContext context = SPServiceContext.GetContext(spSite);
        //    Microsoft.SharePoint.SPServiceContext serviceContext = Microsoft.SharePoint.SPServiceContext.Current;
        //    UserProfileManager upm = new UserProfileManager(serviceContext);
        //    ProfileSubtypePropertyManager pspm = upm.DefaultProfileSubtypeProperties;

        //    UserProfile profile = upm.GetUserProfile(user.LoginName);

        //    return profile;
        //}
    }
}
