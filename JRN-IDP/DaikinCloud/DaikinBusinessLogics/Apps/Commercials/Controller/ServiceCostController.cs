using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.FinanceMenu.Controller;
using Daikin.BusinessLogics.Apps.NonCommercials.Controller;
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

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class ServiceCostController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        private readonly NintexCloudManager nintexCloudManager = new NintexCloudManager();
        public string SPList = "Commercials";

        public void SaveInbound(int Header_ID)
        {
            //usp_ServiceCostInbounds_SaveUpdate
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ServiceCostInbounds_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public List<OptionModel> BindingMasterSPListVendorLC(string ExpenseType)
        {
            try
            {
                List<OptionModel> listOption = new List<OptionModel>();
                SPWeb web = SPContext.Current.Web;
                SPList list = web.Lists["Master Vendor Service Cost"];
                var q = new SPQuery()
                {
                    Query = @"<Where><Eq><FieldRef Name='Criteria_x003a_Title' /><Value Type='Lookup'>" +
                                ExpenseType.ToUpper() + "</Value></Eq></Where>"
                };
                var r = list.GetItems(q);
                dt = new DataTable();
                dt = r.GetDataTable();
                listOption.Add(new OptionModel
                {
                    Code = "", Name = "Please Select", Selected = true
                });
                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new OptionModel
                    {
                        Code = Utility.GetStringValue(row, "Title"),
                        Name = Utility.GetStringValue(row, "Combine")
                    });
                }
                return listOption;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public List<ServiceCostRemarks> GetRemarks(int Header_ID)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ServiceCostRemarks_ListByID";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return Utility.ConvertDataTableToList<ServiceCostRemarks>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<ServiceCostDetail> GetDetail(int Header_ID)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ServiceCostDetail_ListByID";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return Utility.ConvertDataTableToList<ServiceCostDetail>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public ServiceCostOptionModel GetSCDropDown()
        {
            try
            {
                GeneralController fcGen = new GeneralController();
                var listExpenseType = fcGen.BindingMasterDatabase("MasterExpenseType", "Module", "Name", "Please Select");
                listExpenseType = listExpenseType.Where(x => x.Code == "SC").ToList();
                listExpenseType.Insert(0, new OptionModel() { Code = "", Name = "Please Select", Selected = true });
                return new ServiceCostOptionModel
                {
                    Success = true,
                    Messsage = "OK",
                    ListTradingPartner = fcGen.BindingMasterDatabase("MasterTradingPartner", "Title", "Combine", "Please Select"),
                    ListPlant = fcGen.BindingMasterDatabase("MasterPlant_", "Title", "Name", "Please Select"),
                    ListBussPlace = fcGen.BindingMasterDatabase("MasterBussPlace", "Title", "Name", "Please Select"),
                    ListExpenseType = listExpenseType,
                    ListPPJK = BindingMasterSPListPPJK(),
                    ListConditionSC = BindingMasterSPListCondition(false),
                    ListWHT = BindingMasterSPListWHT(),
                    ListVAT = BindingMasterSPListVAT(),
                    ListMasterVendor = fcGen.BindingMasterSPList("Master Vendor Service Cost", "Title", "Name", "Please Select")
                };
            }
            catch(Exception ex)
            {
                return new ServiceCostOptionModel
                {
                    Success = false,
                    Messsage = $"Error fetching dropdown: {ex.Message}"
                };
            }
        }

        public ServiceCostHeader GetDataHeader(string Form_No)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ServiceCostHeader_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", Form_No);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<ServiceCostHeader>(dt)[0];
                }
                else
                {
                    return new ServiceCostHeader();
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public CommonSaveResponseModel SaveSPList(string SiteUrl, ServiceCostHeader h, int TotalItems, string Status)
        {
            try
            {
                int ListItemId = h.Item_ID;
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists["Service Cost"];
                web.AllowUnsafeUpdates = true;
                SPListItem item;
                if (ListItemId == 0)
                {
                    item = list.Items.Add();
                    item["Title"] = h.Form_No;
                    item["Date Process"] = DateTime.Now;
                    item["Requester Name"] = h.Requester_Name;
                    item["Requester Email"] = h.Requester_Email;
                    item["Requester Account"] = h.Requester_Account;
                    item["Requester Branch"] = h.Branch;
                    item["Form Type"] = "SC";
                    item["Status"] = Status;
                    item["Current Layer"] = 0;
                    item["Total Items"] = TotalItems;
                    item["Grand Total"] = h.Grand_Total;
                }
                else
                {
                    item = list.GetItemById(ListItemId);
                    if (h.ID > 0) item["Transaction ID"] = h.ID;
                    item["Status"] = Status;
                    item["Grand Total"] = h.Grand_Total;
                }
                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;
                return new CommonSaveResponseModel
                {
                    ID = ListItemId,
                    Success = true,
                    Message = "Save SP List Service Cost OK"
                };
            }
            catch(Exception ex)
            {
                SaveDataLog("M011", h.ID, h.Form_No, $"Error save to SharePoint List: {ex.Message}");
                return new CommonSaveResponseModel
                {
                    ID = 0,
                    Success = false,
                    Message = $"Error save to SharePoint List: {ex.Message}"
                };
            }
        }

        public void NotifAccountingMIRO(int ListItemId, string MiroNo, string NintexNo)
        {
            string SiteUrl = new Utility().GetConfigValue("SiteUrl");
            SPWeb web = new SPSite(SiteUrl).OpenWeb();
            SPList list = web.Lists[SPList];
            web.AllowUnsafeUpdates = true;

            SPListItem item;
            item = list.GetItemById(ListItemId);
            item["Status"] = "1";
            item["Notify"] = "2";
            item["MIRO No"] = MiroNo;
            item["Nintex No"] = NintexNo;

            item.Update();
            ListItemId = item.ID;
            web.AllowUnsafeUpdates = false;

        }

        public List<WHTOptionModel> BindingMasterSPListWHT()
        {
            try
            {
                SPWeb web = SPContext.Current.Web;
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {

                    SPList list = web.Lists["Master WHT"];
                    SPListItemCollection items = list.Items;
                    dt = new DataTable();
                    dt = items.GetDataTable();
                });
                List<WHTOptionModel> listOption = new List<WHTOptionModel>();
                int ct = 0;
                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new WHTOptionModel
                    {
                        Code = Utility.GetStringValue(row, "Title"),
                        Name = Utility.GetStringValue(row, "Name"),
                        Percentage = Utility.GetDecimalValue(row, "Percentage"),
                        Selected = ct == 0
                    });
                    ct++;
                }
                return listOption;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<VATOptionModel> BindingMasterSPListVAT()
        {
            List<VATOptionModel> listOption = new List<VATOptionModel>();
            try
            {
                SPWeb web = SPContext.Current.Web;
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {

                    SPList list = web.Lists["Master VAT"];
                    SPListItemCollection items = list.Items;
                    dt = new DataTable();
                    dt = items.GetDataTable();
                });
                foreach (DataRow row in dt.Rows)
                {
                    listOption.Add(new VATOptionModel
                    {
                        Code = Utility.GetStringValue(row, "Title"),
                        Name = Utility.GetStringValue(row, "Name"),
                        VAT_Percent = Utility.GetStringValue(row, "Percentage"),
                        Order_x0020_Id = Utility.GetIntValue(row, "Order_x0020_Id"),
                        Selected = Utility.GetIntValue(row, "Order_x0020_Id") == 1
                    });
                }
                return listOption.OrderBy(o => o.Order_x0020_Id).ToList();

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


                ServiceCostConditionModel data = new ServiceCostConditionModel();
                data.Code = "";
                data.Name = "Please Select";
                data.Selected = isSelected;
                data.Title = "";
                data.ID = "0";
                listOption.Add(data);


                foreach (DataRow row in dt.Rows)
                {
                    data = new ServiceCostConditionModel();
                    data.Code = Utility.GetStringValue(row, "ID");
                    data.Name = Utility.GetStringValue(row, "Combine");
                    data.ID = Utility.GetStringValue(row, "ID");
                    data.Title = Utility.GetStringValue(row, "Title");
                    data.Selected = false;
                    listOption.Add(data);
                }
                return listOption;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public CommonResponseModel UpdateRemarks(List<ServiceCostRemarks> listRemarks, string CurrentLoginName)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                foreach (ServiceCostRemarks r in listRemarks)
                {
                    if (string.IsNullOrEmpty(r.Outcome) && string.IsNullOrEmpty(r.Reason_Rejection))
                    {
                        db.CloseConnection(ref conn);
                        throw new Exception("Please tick if OK for " + r.Remarks + "\n if not, then specify the reason of rejection");
                    }
                    db.cmd.CommandText = "dbo.usp_ServiceCostRemarks_Update";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Outcome", r.Outcome);
                    db.AddInParameter(db.cmd, "Reason", r.Reason_Rejection);
                    db.AddInParameter(db.cmd, "ID", r.ID);
                    db.AddInParameter(db.cmd, "Modified_By", CurrentLoginName);
                    db.cmd.ExecuteNonQuery();
                }
                db.CloseConnection(ref conn, true);
                return new CommonResponseModel
                {
                    Success = true, Message = "OK"
                };
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonResponseModel
                {
                    Success = false,
                    Message = $"Error occurredd at UpdateRemarks method on in ServiceCostController | {ex.Message}"
                };
            }
        }

        public CommonResponseModel ApprovalAction_ByPositionID(NintexApprovalModel ntx, List<ServiceCostRemarks> listRemarks, List<ServiceCostDetail> listDetail, bool IsDocumentReceived, string CurrentLoginUserName)
        {
            var response = new CommonResponseModel();
            try
            {
                switch (ntx.Position_ID)
                {
                    case "22":
                        if (!IsDocumentReceived && ntx.OutcomeName == "Approve") throw new Exception("Please tick if the physical document has been received. Then you can continue to approval.");
                        response = new Utility().UpdateDocumentReceived(ntx.Transaction_ID.ToString(), "M011");
                        break;
                    case "15":
                        response = SaveDetail(listDetail, ntx, CurrentLoginUserName);
                        break;
                    case "16":
                        response = UpdateRemarks(listRemarks, CurrentLoginUserName);
                        break;
                    default:
                        response.Success = true;
                        response.Message = "OK | No action required for this position";
                        break;
                }
                return response;
            }
            catch(Exception ex)
            {
                return new CommonResponseModel { Success = false, Message = $"Error occurredd at SC_ApprovalActionByPosition method on in ServiceCostController | {ex.Message}"};
            }
        }

        public List<OptionModel> GetListDueOn()
        {
            var listDueOn = new List<OptionModel>();
            var currentDT = DateTime.Now;
            for (int k = 3; k > 0; k--)
            {
                var dt = currentDT.AddMonths(-k);
                listDueOn.Add(new OptionModel
                {
                    Code = dt.ToString("MMyyyy"),
                    Name = dt.ToString("MMM yyyy")
                });
            }
            for (int i = 0; i < 12; i++)
            {
                var dt = currentDT.AddMonths(i);
                listDueOn.Add(new OptionModel
                {
                    Code = dt.ToString("MMyyyy"),
                    Name = dt.ToString("MMM yyyy")
                });
            }
            return listDueOn;
        }

        public string KoreksiAttachment()
        {
            string query = "";
            string ListId = "193,194,195,196,197,198,199,200,201,202,203,206,207";
            foreach (string s in ListId.Split(','))
            {
                string AttachmentList = sp.GetAllAttachmentByListName("Commercials", Convert.ToInt32(s), "https://sp3.daikin.co.id:8443");
                int i = 1;
                foreach (string attach in AttachmentList.Split(';'))
                {
                    if (!string.IsNullOrEmpty(attach))
                    {
                        string fileName = System.IO.Path.GetFileName(attach);
                        db.OpenConnection(ref conn);
                        db.cmd.CommandText = "UPDATE ServiceCostDetail SET [File_Name] = '" + fileName + "'";
                        db.cmd.CommandText += ", Attachment_Url = '" + attach.Replace("https://sp.daikin.co.id:8443", "") + "'";
                        db.cmd.CommandText += " FROM ServiceCostHeader a inner join ServiceCostDetail b";
                        db.cmd.CommandText += " ON b.Header_ID = a.ID";
                        db.cmd.CommandText += " WHERE b.Item_ID = " + s + " AND b.[No] = " + i.ToString();
                        query += Environment.NewLine + db.cmd.CommandText;

                        Console.WriteLine(query);

                        db.CloseConnection(ref conn);
                        i++;
                    }
                }
            }
            return query;
        }

        public void SaveDataLog(string Module_Code, int Transaction_ID, string Form_No, string Message)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "SaveDataLog_Insert";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", Module_Code);
                db.AddInParameter(db.cmd, "Transaction_ID", Transaction_ID);
                db.AddInParameter(db.cmd, "Form_No", Form_No);
                db.AddInParameter(db.cmd, "Message", Message);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                Console.WriteLine($"Error saving log: {ex.Message}");
            }
        }

        public void InsertHistoryLog(int Header_ID, string Form_No, int Action, string CurrentLogin, string CurrentLoginName, string Comment)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = "usp_Commercial_InsertHistoryLog";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                db.AddInParameter(db.cmd, "Module_Code", "M011");
                db.AddInParameter(db.cmd, "Action", Action);
                db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrLoginName", CurrentLoginName);
                db.AddInParameter(db.cmd, "Comment", Comment);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn, true);
            }
            catch(Exception ex)
            {
                SaveDataLog("M011", Header_ID, Form_No, $"Error insert history log for Service Cost: {ex.Message}");
                db.CloseConnection(ref conn, true);
            }
        }

        public SaveHeaderSCModel SaveHeader(ServiceCostHeader h, List<ServiceCostDetail> listDetail, string SiteUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(h.Form_No))
                {
                    db.OpenConnection(ref conn);
                    h.Form_No = db.Autocounter("Form_No", "ServiceCostHeader", "Form_No", "LC" + DateTime.Now.ToString("yyMM"), 4);
                    db.CloseConnection(ref conn);
                    h.Item_ID = 0;
                }
                h.Grand_Total = listDetail.Sum(s => s.Total_Amount);
                var SaveSPListResponse = SaveSPList(SiteUrl, h, listDetail.Count, "-");
                if (!SaveSPListResponse.Success)
                {
                    return new SaveHeaderSCModel
                    {
                        ID = 0,
                        Success = false,
                        Message = SaveSPListResponse.Message,
                        Header = h
                    };
                }
                h.Item_ID = SaveSPListResponse.ID;
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = "dbo.usp_ServiceCostHeader_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                db.AddInParameter(db.cmd, "Requester_Name", h.Requester_Name);
                db.AddInParameter(db.cmd, "Requester_Email", h.Requester_Email);
                db.AddInParameter(db.cmd, "Trading_Partner_Code", h.Trading_Partner_Code);
                db.AddInParameter(db.cmd, "Plant_Code", h.Plant_Code);
                db.AddInParameter(db.cmd, "PPJK_Code", h.PPJK_Code);
                db.AddInParameter(db.cmd, "Buss_Place_Code", h.Buss_Place_Code);
                db.AddInParameter(db.cmd, "Expense_Type_Code", h.Expense_Type_Code);
                db.AddInParameter(db.cmd, "Grand_Total", h.Grand_Total);
                db.AddInParameter(db.cmd, "Item_ID", h.Item_ID);
                db.AddInParameter(db.cmd, "Approval_Status", h.Approval_Status);
                db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                db.AddInParameter(db.cmd, "Trading_Partner_Name", h.Trading_Partner_Name);
                db.AddInParameter(db.cmd, "Plant_Name", h.Plant_Name);
                db.AddInParameter(db.cmd, "PPJK_Name", h.PPJK_Name);
                db.AddInParameter(db.cmd, "Buss_Place_Name", h.Buss_Place_Name);
                db.AddInParameter(db.cmd, "Expense_Type_Name", h.Expense_Type_Name);
                db.AddInParameter(db.cmd, "PPJK_Curr", h.PPJK_Curr);
                db.AddInParameter(db.cmd, "PPJK_Category", h.PPJK_Category);
                db.AddInParameter(db.cmd, "Bank_Account_No", h.Bank_Account_No);
                db.AddInParameter(db.cmd, "Bank_Key_ID", h.Bank_Key_ID);
                db.AddInParameter(db.cmd, "Is_New", SiteUrl.Contains("3473"));
                db.AddInParameter(db.cmd, "DIID_Invoice", h.DIID_Invoice);
                int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());
                db.CloseConnection(ref conn, true);
                h.ID = Header_ID;
                return new SaveHeaderSCModel
                {
                    ID = Header_ID, Success = true, Message = "Save ServiceCostHeader OK", Header = h
                };
            }
            catch(Exception ex)
            {
                SaveDataLog("M011", h.ID, h.Form_No, $"Error save ServiceCostHeader: {ex.Message}");
                db.CloseConnection(ref conn, true);
                return new SaveHeaderSCModel
                {
                    ID = 0,
                    Success = false,
                    Message = $"Error save ServiceCostHeader: {ex.Message}",
                    Header = h
                };
            }
        }

        public CommonResponseModel SaveDetail(List<ServiceCostDetail> ListDetail, NintexApprovalModel ntx, string Modified_By)
        {
            int index = 0;
            try
            {
                db.OpenConnection(ref conn, true);
                foreach (var d in ListDetail)
                {
                    index++;
                    if (string.IsNullOrEmpty(d.WHT_Type_Name))
                    {
                        db.CloseConnection(ref conn);
                        throw new Exception("Please select the WHT Type for BL No. " + d.BL_No);
                    }
                    db.cmd.CommandText = "dbo.[usp_ServiceCostDetail_SaveUpdate]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "ID", d.ID);
                    db.AddInParameter(db.cmd, "No", d.No);
                    db.AddInParameter(db.cmd, "Header_ID", ntx.Transaction_ID);
                    db.AddInParameter(db.cmd, "Form_No", ntx.FormNo);
                    db.AddInParameter(db.cmd, "Document_Date", d.Document_Date);
                    db.AddInParameter(db.cmd, "Ref_Type", d.Ref_Type);
                    db.AddInParameter(db.cmd, "Ref_No", d.Ref_No);
                    db.AddInParameter(db.cmd, "BL_No", d.BL_No);
                    db.AddInParameter(db.cmd, "FOB_No", d.FOB_No);
                    db.AddInParameter(db.cmd, "Vendor_No", d.Vendor_No);
                    db.AddInParameter(db.cmd, "Vendor_Name", d.Vendor_Name);
                    db.AddInParameter(db.cmd, "Vendor_Invoice_No", d.Vendor_Invoice_No);
                    db.AddInParameter(db.cmd, "Condition_Code", d.Condition_Code);
                    db.AddInParameter(db.cmd, "Condition_Name", d.Condition_Name);
                    db.AddInParameter(db.cmd, "Text", d.Text);
                    db.AddInParameter(db.cmd, "VAT_Type", d.VAT_Type);
                    db.AddInParameter(db.cmd, "VAT_No", d.VAT_No);
                    db.AddInParameter(db.cmd, "VAT_Percent", d.VAT_Percent);
                    db.AddInParameter(db.cmd, "VAT_Amount", d.VAT_Amount);
                    db.AddInParameter(db.cmd, "Freight_Cost", d.Freight_Cost);
                    db.AddInParameter(db.cmd, "Tax_Base_Amount", d.Tax_Base_Amount);
                    db.AddInParameter(db.cmd, "WHT_Type_Code", d.WHT_Type_Code);
                    db.AddInParameter(db.cmd, "WHT_Type_Name", d.WHT_Type_Name);
                    db.AddInParameter(db.cmd, "WHT_Amount", d.WHT_Amount);
                    db.AddInParameter(db.cmd, "Total_Amount", d.Total_Amount);
                    db.AddInParameter(db.cmd, "Modified_By", Modified_By);
                    db.AddInParameter(db.cmd, "Assignment", d.Assignment);
                    db.cmd.ExecuteNonQuery();
                }
                db.CloseConnection(ref conn, true);
                return new CommonResponseModel
                {
                    Success = true, Message ="OK"
                };
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                SaveDataLog("M011", ntx.Transaction_ID, ntx.FormNo, $"Error save ServiceCostDetail: {ex.Message} | Error at row: {index}");
                return new CommonResponseModel
                {
                    Success = false,
                    Message = $"Error save ServiceCostDetail: {ex.Message} | Error at row: {index}"
                };
            }
        }

        public CommonSaveResponseModel SaveDetail(List<ServiceCostDetail> listDetail, ServiceCostHeader h, string ServerPath, string SiteUrl)
        {
            int index = 0;
            try
            {
                db.OpenConnection(ref conn, true);
                foreach(var d in listDetail)
                {
                    index++;
                    db.cmd.CommandText = "dbo.[usp_ServiceCostDetail_SaveUpdate]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ID", d.ID);
                    db.AddInParameter(db.cmd, "No", d.No);
                    db.AddInParameter(db.cmd, "Header_ID", h.ID);
                    db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                    db.AddInParameter(db.cmd, "Document_Date", d.Document_Date);
                    db.AddInParameter(db.cmd, "Ref_Type", d.Ref_Type);
                    db.AddInParameter(db.cmd, "Ref_No", d.Ref_No);
                    db.AddInParameter(db.cmd, "BL_No", d.BL_No);
                    db.AddInParameter(db.cmd, "FOB_No", d.FOB_No);
                    db.AddInParameter(db.cmd, "Vendor_No", d.Vendor_No);
                    db.AddInParameter(db.cmd, "Vendor_Name", d.Vendor_Name);
                    db.AddInParameter(db.cmd, "Vendor_Invoice_No", d.Vendor_Invoice_No);
                    db.AddInParameter(db.cmd, "Condition_ID", d.Condition_ID);
                    db.AddInParameter(db.cmd, "Condition_Code", d.Condition_Code);
                    db.AddInParameter(db.cmd, "Condition_Name", d.Condition_Name);
                    db.AddInParameter(db.cmd, "Text", d.Text);
                    db.AddInParameter(db.cmd, "VAT_Type", d.VAT_Type);
                    db.AddInParameter(db.cmd, "VAT_No", d.VAT_No);
                    db.AddInParameter(db.cmd, "VAT_Percent", d.VAT_Percent);
                    db.AddInParameter(db.cmd, "VAT_Amount", d.VAT_Amount);
                    db.AddInParameter(db.cmd, "Freight_Cost", d.Freight_Cost);
                    db.AddInParameter(db.cmd, "Tax_Base_Amount", d.Tax_Base_Amount);
                    db.AddInParameter(db.cmd, "WHT_Type_Code", d.WHT_Type_Code);
                    db.AddInParameter(db.cmd, "WHT_Type_Name", d.WHT_Type_Name);
                    db.AddInParameter(db.cmd, "WHT_Amount", d.WHT_Amount);
                    db.AddInParameter(db.cmd, "Total_Amount", d.Total_Amount);
                    db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                    db.AddInParameter(db.cmd, "File_Name", d.File_Name);
                    db.AddInParameter(db.cmd, "Attachment_Url", "/Lists/Service Cost/Attachments/" + h.Item_ID.ToString() + "/" + d.File_Name);
                    db.AddInParameter(db.cmd, "Trading_Partner_Code", h.Trading_Partner_Code);
                    db.AddInParameter(db.cmd, "Business_Place_Code", h.Buss_Place_Code);
                    db.AddInParameter(db.cmd, "PPJK_Code", h.PPJK_Code);
                    db.AddInParameter(db.cmd, "Partner_Bank_Type", h.PPJK_Code);
                    db.AddInParameter(db.cmd, "Currency", h.PPJK_Curr);
                    db.AddInParameter(db.cmd, "Assignment", d.FOB_No);

                    db.cmd.ExecuteNonQuery();
                    var dPathFile = ServerPath + d.File_Name;
                    sp.UploadFileInCustomList("Service Cost", h.Item_ID, dPathFile, SiteUrl);
                }
                db.CloseConnection(ref conn, true);
                return new CommonSaveResponseModel
                {
                    ID = h.ID,
                    Success = true,
                    Message = "Save ServiceCostDetail OK"
                };
            }
            catch(Exception ex)
            {
                SaveDataLog("M011", h.ID, h.Form_No, $"Error save ServiceCostDetail: {ex.Message} | Error at row: {index}");
                db.CloseConnection(ref conn, true);
                return new CommonSaveResponseModel
                {
                    Success = false,
                    Message = $"Error save ServiceCostDetail: {ex.Message} | Error at row: {index}"
                };
            }
        }

        public CommonSaveResponseModel Save(ServiceCostHeader h, List<ServiceCostDetail> listDetail, string SiteUrl, string ServerPath)
        {
            try
            {
                #region Commented out code
                //if (string.IsNullOrEmpty(h.Form_No))
                //{
                //    db.OpenConnection(ref conn);
                //    h.Form_No = db.Autocounter("Form_No", "ServiceCostHeader", "Form_No", "LC" + DateTime.Now.ToString("yyMM"), 4);
                //    db.CloseConnection(ref conn);
                //    h.Item_ID = 0;
                //}
                //h.Grand_Total = listDetail.Sum(s => s.Total_Amount);
                //h.Item_ID = SaveSPList(SiteUrl, h, listDetail.Count, "-"); //1 Trigger WF

                //db.OpenConnection(ref conn, true);
                //#region Header
                ////db.cmd.CommandText = "dbo.usp_ServiceCostHeader_SaveUpdate";
                ////db.cmd.CommandType = CommandType.StoredProcedure;
                ////db.cmd.Parameters.Clear();

                ////db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                ////db.AddInParameter(db.cmd, "Requester_Name", h.Requester_Name);
                ////db.AddInParameter(db.cmd, "Requester_Email", h.Requester_Email);
                ////db.AddInParameter(db.cmd, "Trading_Partner_Code", h.Trading_Partner_Code);
                ////db.AddInParameter(db.cmd, "Plant_Code", h.Plant_Code);
                ////db.AddInParameter(db.cmd, "PPJK_Code", h.PPJK_Code);
                ////db.AddInParameter(db.cmd, "Buss_Place_Code", h.Buss_Place_Code);
                ////db.AddInParameter(db.cmd, "Expense_Type_Code", h.Expense_Type_Code);
                ////db.AddInParameter(db.cmd, "Grand_Total", h.Grand_Total);
                ////db.AddInParameter(db.cmd, "Item_ID", h.Item_ID);
                ////db.AddInParameter(db.cmd, "Approval_Status", h.Approval_Status);
                ////db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                ////db.AddInParameter(db.cmd, "Trading_Partner_Name", h.Trading_Partner_Name);
                ////db.AddInParameter(db.cmd, "Plant_Name", h.Plant_Name);
                ////db.AddInParameter(db.cmd, "PPJK_Name", h.PPJK_Name);
                ////db.AddInParameter(db.cmd, "Buss_Place_Name", h.Buss_Place_Name);
                ////db.AddInParameter(db.cmd, "Expense_Type_Name", h.Expense_Type_Name);
                ////db.AddInParameter(db.cmd, "PPJK_Curr", h.PPJK_Curr);
                ////db.AddInParameter(db.cmd, "PPJK_Category", h.PPJK_Category);
                ////db.AddInParameter(db.cmd, "Bank_Account_No", h.Bank_Account_No);
                ////db.AddInParameter(db.cmd, "Bank_Key_ID", h.Bank_Key_ID);

                ////int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());
                ////h.ID = Header_ID;

                //#endregion

                //#region Detail
                //foreach (ServiceCostDetail d in listDetail)
                //{
                //    db.cmd.CommandText = "dbo.[usp_ServiceCostDetail_SaveUpdate]";
                //    db.cmd.CommandType = CommandType.StoredProcedure;
                //    db.cmd.Parameters.Clear();

                //    db.AddInParameter(db.cmd, "ID", d.ID);
                //    db.AddInParameter(db.cmd, "No", d.No);
                //    db.AddInParameter(db.cmd, "Header_ID", h.ID);
                //    db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                //    db.AddInParameter(db.cmd, "Document_Date", d.Document_Date);
                //    db.AddInParameter(db.cmd, "Ref_Type", d.Ref_Type);
                //    db.AddInParameter(db.cmd, "Ref_No", d.Ref_No);
                //    db.AddInParameter(db.cmd, "BL_No", d.BL_No);
                //    db.AddInParameter(db.cmd, "FOB_No", d.FOB_No);
                //    db.AddInParameter(db.cmd, "Vendor_No", d.Vendor_No);
                //    db.AddInParameter(db.cmd, "Vendor_Name", d.Vendor_Name);
                //    db.AddInParameter(db.cmd, "Vendor_Invoice_No", d.Vendor_Invoice_No);

                //    db.AddInParameter(db.cmd, "Condition_ID", d.Condition_ID);
                //    db.AddInParameter(db.cmd, "Condition_Code", d.Condition_Code);
                //    db.AddInParameter(db.cmd, "Condition_Name", d.Condition_Name);
                //    db.AddInParameter(db.cmd, "Text", d.Text);
                //    db.AddInParameter(db.cmd, "VAT_Type", d.VAT_Type);
                //    db.AddInParameter(db.cmd, "VAT_No", d.VAT_No);
                //    db.AddInParameter(db.cmd, "VAT_Percent", d.VAT_Percent);
                //    db.AddInParameter(db.cmd, "VAT_Amount", d.VAT_Amount);
                //    db.AddInParameter(db.cmd, "Freight_Cost", d.Freight_Cost);
                //    db.AddInParameter(db.cmd, "Tax_Base_Amount", d.Tax_Base_Amount);
                //    db.AddInParameter(db.cmd, "WHT_Type_Code", d.WHT_Type_Code);
                //    db.AddInParameter(db.cmd, "WHT_Type_Name", d.WHT_Type_Name);
                //    db.AddInParameter(db.cmd, "WHT_Amount", d.WHT_Amount);
                //    db.AddInParameter(db.cmd, "Total_Amount", d.Total_Amount);
                //    db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);

                //    db.AddInParameter(db.cmd, "File_Name", d.File_Name);
                //    db.AddInParameter(db.cmd, "Attachment_Url", "/Lists/" + SPList + "/Attachments/" + h.Item_ID.ToString() + "/" + d.File_Name);

                //    db.AddInParameter(db.cmd, "Trading_Partner_Code", h.Trading_Partner_Code);
                //    db.AddInParameter(db.cmd, "Business_Place_Code", h.Buss_Place_Code);
                //    db.AddInParameter(db.cmd, "PPJK_Code", h.PPJK_Code);
                //    db.AddInParameter(db.cmd, "Partner_Bank_Type", h.PPJK_Code);
                //    db.AddInParameter(db.cmd, "Currency", h.PPJK_Curr);
                //    db.AddInParameter(db.cmd, "Assignment", d.FOB_No);

                //    db.cmd.ExecuteNonQuery();

                //    var dPathFile = ServerPath + d.File_Name;
                //    sp.UploadFileInCustomList(SPList, h.Item_ID, dPathFile, SiteUrl);

                //}
                //#endregion  

                //db.CloseConnection(ref conn, true);

                //#region Trigger WF
                //h.Item_ID = SaveSPList(SiteUrl, h, listDetail.Count, "1"); //1 Trigger WF
                //#endregion  

                //return h.ID;
                #endregion
                var saveHeaderResponse = SaveHeader(h, listDetail, SiteUrl);
                if (!saveHeaderResponse.Success)
                {
                    return new CommonSaveResponseModel
                    {
                        ID = 0,
                        Success = false,
                        Message = saveHeaderResponse.Message
                    };
                }
                var saveDetailResponse = SaveDetail(listDetail, saveHeaderResponse.Header, ServerPath, SiteUrl);
                if (!saveDetailResponse.Success)
                {
                    return new CommonSaveResponseModel
                    {
                        ID = 0,
                        Success = false,
                        Message = saveDetailResponse.Message
                    };
                }
                InsertHistoryLog(saveHeaderResponse.Header.ID, saveHeaderResponse.Header.Form_No, 1, sp.GetCurrentUserLogin(SiteUrl), sp.GetCurrentLoginFullName(SiteUrl), "");

                string NAC_WorkflowId = "";
                DataTable dtMenu = new FinanceMenu.Controller.GeneralController().GetDetailMenuByCode("M010");
                foreach (DataRow row in dtMenu.Rows)
                {
                    NAC_WorkflowId = Utility.GetStringValue(row, "NAC_Workflow_ID");
                }

                Task.Run(async () => { await nintexCloudManager.Commercial_StartWorkflow(
                    saveHeaderResponse.Header.Item_ID, saveHeaderResponse.Header.ID, "M011", NAC_WorkflowId); }).Wait();
                return new CommonSaveResponseModel
                {
                    ID = saveHeaderResponse.Header.ID,
                    Message = "Save Data OK",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonSaveResponseModel
                {
                    Success = false,
                    Message = $"Error Save Service Cost: {ex.Message}"
                };
            }
        }

        public bool IsRecordExists(ServiceCostDetail sd)
        {
            try
            {
                DataTable dtRecords = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPInboundBL_Check";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Trading_Partner_Code", sd.Trading_Partner_Code);
                db.AddInParameter(db.cmd, "PPJK_Code", sd.PPJK_Code);
                db.AddInParameter(db.cmd, "BL_No", sd.BL_No);

                reader = db.cmd.ExecuteReader();
                dtRecords.Load(reader);

                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                if (dtRecords.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public SAPInboundModel GetDataByType(string ReferenceNo, string ReferenceType, string TradingPartnerCode, string PPJKCode)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPInboundBL_GetDataByType";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ReferenceNo", ReferenceNo);
                db.AddInParameter(db.cmd, "ReferenceType", ReferenceType);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                if(dt.Rows.Count <= 0)
                {
                    throw new Exception(ReferenceType + " No. [" + ReferenceNo + "] not found");
                }
                return Utility.ConvertDataTableToList<SAPInboundModel>(dt)[0];
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<PopReferenceModel> ListMasterReference(string Keywords, int PageIndex, string ReferenceType, out int RecordCount)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPInboundBL_ListDataByType";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "Keywords", Keywords);
                db.AddInParameter(db.cmd, "Reference_Type", ReferenceType);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<PopReferenceModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public CommonResponseModel ApprovalAction(string ApprovalOutcome, string ListName, string Module_Code, string Form_No, int ItemID, int HeaderID, string Comment)
        {
            try
            {
                CurrentApproverModel taskResponder = nintexCloudManager.Commercial_GetTaskResponder(Module_Code, HeaderID, Form_No);
                if (taskResponder == null)
                {
                    return new CommonResponseModel { Success = false, Message = "Error occurred when retrieving Task Responder on Commercial_GetTaskResponder method" };
                }
                new ListController().CustomFormUpdateApprover(HeaderID, ListName, taskResponder.UserName, taskResponder.FullName, Comment);
                var transactionData = Aprroval.GetListDataIDByHeaderID_New(ListName, HeaderID);
                var taskAssignmentResponse = nintexCloudManager.GetTaskAssignment(transactionData[0].NAC_Guid, Form_No);
                if (!taskAssignmentResponse.Success || taskAssignmentResponse.TaskAssignments == null)
                {
                    return new CommonResponseModel { Success = false, Message = taskAssignmentResponse.Message };
                }
                return NintexCloudManager.ProcessNACTask(taskAssignmentResponse.TaskAssignments, taskResponder.Email, ApprovalOutcome);
            }
            catch(Exception ex)
            {
                return new CommonResponseModel
                {
                    Success = false, Message = $"Error in ApprovalAction method in ServiceCostController | {ex.Message}"
                };
            }
        }
    }
}
