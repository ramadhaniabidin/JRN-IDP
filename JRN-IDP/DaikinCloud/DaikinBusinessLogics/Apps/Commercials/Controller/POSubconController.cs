using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class POSubconController
    {
        DataTable dt = new DataTable();
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;

        public List<VendorSubconModel> ListVendor(int PageIndex, string Keywords, out int RecordCount)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_SAPCommercialVendorData_PopListData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "Keywords", Keywords);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<VendorSubconModel>(dt);

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POSubconModel> ListPendingData()
        {
            try
            {
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "usp_SAPCommercialPOSubconHeader_ListPending";
                db.cmd.CommandText = "SAP.usp_SAPCommercialPOSubconHeader_ListPending";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<POSubconModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public bool IsThereWorkflowRunning(int ListItemId)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_POSubconHeader_CheckRunningWorkflow";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "ListItemId", ListItemId);

                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dt.Rows)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void UpdateNeedSyncToZero(string Vendor_Code, string Partner_Bank_ID, int Item_ID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_SAPCommercialVendorBankData_UpdateSync]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", Vendor_Code);
                db.AddInParameter(db.cmd, "Partner_Bank_ID", Partner_Bank_ID);
                db.AddInParameter(db.cmd, "Item_ID", Item_ID);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public void SyncUpdateVendorBankSubcon()
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_SAPCommercialVendorBankData_PendingSync]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dt.Rows)
                {
                    SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
                    SPList list = web.Lists["Master Vendor Subcon"];
                    web.AllowUnsafeUpdates = true;
                    int ListItemId = Utility.GetIntValue(row, "Item_ID");
                    try
                    {
                        SPListItem item;

                        if (ListItemId == 0)
                        {
                            item = list.Items.Add();
                        }
                        else
                        {
                            item = list.GetItemById(ListItemId);
                        }
                        item["Title"] = Utility.GetStringValue(row, "Vendor_Name");
                        item["Code"] = Utility.GetStringValue(row, "Vendor_Code");
                        item["Vendor_x0020_PIC"] = Utility.GetStringValue(row, "Vendor_PIC");
                        item["Bank_x0020_Key_x0020_ID"] = Utility.GetStringValue(row, "Bank_Key");
                        item["Bank_x0020_Key_x0020_Name"] = Utility.GetStringValue(row, "Bank_Key_Name");
                        item["Bank_x0020_Account"] = Utility.GetStringValue(row, "Bank_Account");
                        item["Partner_x0020_Bank_x0020_ID"] = Utility.GetStringValue(row, "Partner_Bank_ID");
                        item["Account_x0020_Holder"] = Utility.GetStringValue(row, "Account_Holder");
                        item.Update();

                        UpdateNeedSyncToZero(Utility.GetStringValue(row, "Vendor_Code"), Utility.GetStringValue(row, "Partner_Bank_ID"), item.ID);
                    }
                    catch (Exception ex)
                    {
                        db.CloseConnection(ref conn);
                        throw ex;
                    }
                    finally
                    {
                        web.AllowUnsafeUpdates = false;
                    }
                }

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //TriggerWorkflow and notif to Admin Service to submit documents
        public int UpdateXML_List(string xml, int ListItemId)
        {
            SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
            //SPList list = web.Lists["Commercials"];
            SPList list = web.Lists["PO Subcon Sales Force"];
            web.AllowUnsafeUpdates = true;
            try
            {
                SPListItem item;

                item = list.GetItemById(ListItemId);
                item["Notify"] = "X";
                item["Status"] = IsThereWorkflowRunning(ListItemId) ? "Already Running" : "1";
                item["Lap"] = 1;
                item["Allow Edit"] = "0";
                item["Approval Type"] = "2"; //Admin Service (Show Mandatory& Optional Attachment)
                item["Details"] = xml;
                item.Update();

                return item.ID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public int UpdateXML_List_Fixing(string xml, int ListItemId)
        {
            SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
            SPList list = web.Lists["Commercials"];
            web.AllowUnsafeUpdates = true;
            try
            {
                SPListItem item;

                item = list.GetItemById(ListItemId);
                item["Details"] = xml;
                item.Update();

                return item.ID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }


        public void ResumeApproval(int Item_ID, string Allow_Edit, string Notify)
        {
            try
            {
                SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
                SPList list = web.Lists["Commercials"];
                web.AllowUnsafeUpdates = true;

                SPListItem item;

                if (Item_ID > 0)
                {
                    item = list.GetItemById(Item_ID);
                    item["Status"] = "1";
                    item["Allow Edit"] = Allow_Edit;


                    item["Notify"] = Notify;

                    if (Notify == "2") //Notif PO Release
                    {
                        item["Released"] = true;
                    }

                    item["Approval Type"] = "x"; //Admin Service Team submit document. Jika 1 tidak muncul
                    item.Update();
                    web.AllowUnsafeUpdates = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int SaveSPList(string SiteUrl, POSubconModel h, string xml, string xml_rs_atc_mandatory, string xml_rs_atc_optional)
        {
            int ListItemId = 0;
            SPWeb web = new SPSite(SiteUrl).OpenWeb();
            SPList list = web.Lists["Commercials"];
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
                item["Requester Branch"] = h.Requester_Branch;
                item["Vendor Number"] = h.Vendor_Number;
                item["Vendor Name"] = h.Vendor_Name;
                item["Vendor Account Number"] = h.Account_Number;
                item["Vendor Account Name"] = h.Bank_Account_Name;
                item["Vendor Bank"] = h.Bank_Key_Name;
                item["Document Date"] = h.Document_Date;
                item["Form Type"] = "SUBCON";
                item["Nintex No"] = h.Form_No;

                item["Details"] = xml;
                item["Mandatory Attachment"] = xml_rs_atc_mandatory;
                item["Detail Attachments"] = xml_rs_atc_optional;

                item["Status"] = "Draft";
                item["Allow Edit"] = "1";
                item["Current Layer"] = 0;
                item["Grand Total"] = h.Grand_Total;
                item["Approval Type"] = "1"; //for PO Subcon Requestor
                item["Notify"] = "3"; //Info to Admin Service Team that PO Subcon has been generated
                item["SVO No"] = h.SVO_No;

                item["Purch Group"] = h.Purchasing_Group;
                item["Subcon Category"] = h.Subcon_Category_Name;

                if (h.ID > 0) item["Transaction ID"] = h.ID;
            }
            else
            {
                item = list.GetItemById(ListItemId);
                item["Transaction ID"] = h.ID;
                item["Details"] = xml;
                item["Status"] = "";
                item["Notify"] = "0"; //Sending EMail to Requestor after submit
                item["Grand Total"] = h.Grand_Total;
            }
            item.Update();
            ListItemId = item.ID;
            web.AllowUnsafeUpdates = false;

            return ListItemId;
        }

        public List<POSubconAttachmentModel> listAttachmentType(string mandatory, int subcon_category_id)
        {
            try
            {
                DataTable dtx = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_MasterDocumentSubcon_List]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Mandatory", mandatory);
                db.AddInParameter(db.cmd, "Subcon_Category_ID", subcon_category_id);
                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<POSubconAttachmentModel>(dtx);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public string GenerateXML_RS_AttachmentMandatory(List<POSubconAttachmentModel> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (POSubconAttachmentModel data in idList)
            {
                xml += "<Item><m_description type=\"System.String\">" + data.Title + "</m_description>";
                xml += "<m_atc type=\"System.String\" ></m_atc>";
                xml += "</Item>";
                seq++;
            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public string GenerateXML_RS_AttachmentOptional(List<POSubconAttachmentModel> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (POSubconAttachmentModel data in idList)
            {
                xml += "<Item><d_description type=\"System.String\">" + data.Title + "</d_description>";
                xml += "<d_atc type=\"System.String\" ></d_atc>";
                xml += "</Item>";
                seq++;
            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public string GenerateXML_RS(List<POSubconDetailModel> idList, bool IsNew)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            if (IsNew)
            {
                foreach (POSubconDetailModel data in idList)
                {

                    xml += "<Item><no type=\"System.String\">" + seq.ToString() + "</no>";
                    xml += "<item_code type=\"System.String\" >" + data.Item_Code + "</item_code>";
                    xml += "<item_text type=\"System.String\" >" + SecurityElement.Escape(data.Item_Text) + "</item_text>";
                    xml += "<delivery_date type=\"System.DateTime\">" + data.Delivery_Date + "</delivery_date>";
                    xml += "<uom type=\"System.String\">" + data.UoM + "</uom>";
                    xml += "<material_code type=\"System.String\">" + data.Material_Code + "</material_code>";
                    xml += "<po_qty type=\"System.Int32\" >" + data.PO_Qty.ToString() + "</po_qty>";
                    xml += "<wht_amount type=\"System.Double\" >0</wht_amount>";
                    xml += "<base_amount type=\"System.Double\">" + data.Base_Amount.ToString() + "</base_amount>";
                    xml += "<net_price type=\"System.Double\">" + data.Net_Price.ToString() + "</net_price>";
                    xml += "</Item>";
                    seq++;
                }
            }
            else
            {

                foreach (POSubconDetailModel data in idList)
                {

                    xml += "<Item><no type=\"System.String\">" + seq.ToString() + "</no>";
                    xml += "<item_code type=\"System.String\" >" + data.Item_Code + "</item_code>";
                    xml += "<item_text type=\"System.String\" >" + SecurityElement.Escape(data.Item_Text) + "</item_text>";
                    xml += "<delivery_date type=\"System.DateTime\">" + data.Delivery_Date + "</delivery_date>";
                    xml += "<uom type=\"System.String\">" + data.UoM + "</uom>";
                    xml += "<material_code type=\"System.String\">" + data.Material_Code + "</material_code>";
                    xml += "<po_qty type=\"System.Int32\" >" + data.PO_Qty.ToString() + "</po_qty>";

                    xml += "<ddl_WHT type=\"System.String\">" + data.DDL_WHT + "</ddl_WHT>";
                    xml += "<WHT_Amount type=\"System.Double\">" + data.WHT_Amount.ToString() + "</WHT_Amount>";
                    xml += "<WHT_Percent type=\"System.String\">" + data.WHT_Percent + "</WHT_Percent>";
                    xml += "<WHT_Description type=\"System.String\">" + SecurityElement.Escape(data.WHT_Description) + "</WHT_Description>";


                    xml += "<base_amount type=\"System.Double\">" + data.Base_Amount.ToString() + "</base_amount>";
                    xml += "<net_price type=\"System.Double\">" + data.Net_Price.ToString() + "</net_price>";
                    xml += "<ddl_VAT type=\"System.String\">" + data.DDL_VAT + "</ddl_VAT>";
                    xml += "<vat_percentage type=\"System.String\">" + data.VAT_Percentage + "</vat_percentage>";
                    xml += "<vat_code type=\"System.String\">" + data.VAT_Code + "</vat_code>";
                    xml += "<vat_id type=\"System.Int32\">" + data.VAT_ID.ToString() + "</vat_id>";
                    xml += "<vat_amount type=\"System.String\">" + data.VAT_Amount.ToString() + "</vat_amount>";
                    xml += "<cv_SuperNetAmount type=\"System.Double\">" + data.Net_Price.ToString() + "</cv_SuperNetAmount>";

                    xml += "</Item>";
                    seq++;
                }

            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public void GetAllNintexNeedsUpdateWHT()
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_POSubconDetail_ListNoWHTAmount";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                foreach (DataRow row in dt.Rows)
                {
                    string pd = Utility.GetStringValue(row, "Form_No");
                    int Item_ID = Utility.GetIntValue(row, "Item_ID");
                    List<POSubconDetailModel> listDetail = new POSubconController().listDetailByNintexNo(pd);
                    Console.WriteLine(pd);

                    string xmlDetails = FixingXMLRS(listDetail);
                    UpdateXML_List_Fixing(xmlDetails, Item_ID);

                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public string FixingXMLRS(List<POSubconDetailModel> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (POSubconDetailModel data in idList)
            {

                xml += "<Item><no type=\"System.String\">" + seq.ToString() + "</no>";
                xml += "<item_code type=\"System.String\" >" + data.Item_Code + "</item_code>";
                xml += "<item_text type=\"System.String\" >" + SecurityElement.Escape(data.Item_Text).Replace("&nbsp;", "") + "</item_text>";
                xml += "<delivery_date type=\"System.DateTime\">" + data.Delivery_Date + "</delivery_date>";
                xml += "<uom type=\"System.String\">" + data.UoM + "</uom>";
                xml += "<material_code type=\"System.String\">" + data.Material_Code + "</material_code>";
                xml += "<po_qty type=\"System.Int32\" >" + data.PO_Qty.ToString() + "</po_qty>";
                xml += "<wht_amount type=\"System.Double\" >0</wht_amount>";
                xml += "<base_amount type=\"System.Double\">" + data.Base_Amount.ToString() + "</base_amount>";
                xml += "<net_price type=\"System.Double\">" + data.Net_Price.ToString() + "</net_price>";
                xml += "<ddl_VAT type=\"System.String\">" + data.DDL_VAT + "</ddl_VAT>";
                xml += "<vat_percentage type=\"System.String\">" + data.VAT_Percentage + "</vat_percentage>";
                xml += "<vat_code type=\"System.String\">" + data.VAT_Code + "</vat_code>";
                xml += "<vat_id type=\"System.Int32\">" + data.VAT_ID.ToString() + "</vat_id>";
                xml += "<vat_amount type=\"System.String\">" + data.VAT_Amount.ToString() + "</vat_amount>";


                xml += "<ddl_WHT type=\"System.String\">" + data.DDL_WHT + "</ddl_WHT>";
                xml += "<WHT_Amount type=\"System.Double\">" + data.WHT_Amount.ToString() + "</WHT_Amount>";
                xml += "<WHT_Percent type=\"System.String\">" + data.WHT_Percent + "</WHT_Percent>";
                xml += "<WHT_Description type=\"System.String\">" + data.WHT_Description + "</WHT_Description>";

                xml += "<cv_SuperNetAmount type=\"System.Double\">" + data.Net_Price.ToString() + "</cv_SuperNetAmount>";


                xml += "</Item>";
                seq++;
            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public List<POSubconDetailModel> listDetailByNintexNo(string Nintex_No)
        {
            //usp_POSubconDetail_ListByNintexNo
            db.OpenConnection(ref conn);
            //db.cmd.CommandText = "usp_POSubconDetail_ListByNintexNo";
            db.cmd.CommandText = "SAP.usp_POSubconDetail_ListByNintexNo";
            db.cmd.CommandType = CommandType.StoredProcedure;
            db.cmd.Parameters.Clear();
            db.AddInParameter(db.cmd, "Nintex_No", Nintex_No);
            reader = db.cmd.ExecuteReader();
            dt = new DataTable();
            dt.Load(reader);
            db.CloseDataReader(reader);
            db.CloseConnection(ref conn);
            return Utility.ConvertDataTableToList<POSubconDetailModel>(dt);
        }

        public List<string> GetBranchCurrentLogin(string CurrentLogin)
        {
            List<string> listBranch = new List<string>();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterPOSubconCreator_GetBranchByCurrentLogin";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);

                db.CloseConnection(ref conn);
                foreach (DataRow row in dt.Rows)
                {
                    listBranch.Add(Utility.GetStringValue(row, "Branch_Name"));
                }
                return listBranch;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public int GetItemId(string pd)
        {
            try
            {
                DataTable dtx = new DataTable();
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "usp_SAPCommercialPOSubconHeader_GetItemId";
                db.cmd.CommandText = "SAP.usp_SAPCommercialPOSubconHeader_GetItemId";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Purchasing_Document", pd);
                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                int ListItemId = 0;
                foreach (DataRow r in dtx.Rows)
                {
                    ListItemId = Utility.GetIntValue(r, "Item_ID");
                }
                return ListItemId;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }

        }

        public void SaveBulkSPList()
        {
            try
            {
                List<POSubconModel> list = ListPendingData();
                foreach (POSubconModel data in list)
                {
                    Console.WriteLine(data.Form_No);
                    List<POSubconDetailModel> listDetail = listDetailByNintexNo(data.Purchasing_Document);
                    string xml_RS = GenerateXML_RS(listDetail, true);
                    string xml_RS_Atc_Mandatory = GenerateXML_RS_AttachmentMandatory(listAttachmentType("1", data.Subcon_Category_ID));
                    string xml_RS_Atc_Optional = GenerateXML_RS_AttachmentOptional(listAttachmentType("0", data.Subcon_Category_ID));



                    int Item_ID = SaveSPList(Utility.SpSiteUrl, data, xml_RS, xml_RS_Atc_Mandatory, xml_RS_Atc_Optional);
                    Console.WriteLine(Item_ID);


                    #region Update Item ID
                    db.OpenConnection(ref conn);
                    //db.cmd.CommandText = "dbo.usp_POSubconHeader_UpdateItemId";
                    db.cmd.CommandText = "SAP.usp_POSubconHeader_UpdateItemId";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Purchasing_Document", data.Purchasing_Document);
                    db.AddInParameter(db.cmd, "Item_ID", Item_ID);
                    db.cmd.ExecuteNonQuery();
                    db.CloseConnection(ref conn);
                    #endregion

                    #region Upload Print out PO Subcon
                    new SAPSubconController().SaveAttachmentPOSubcon(data.Form_No, Item_ID);
                    #endregion

                    #region Call Workflow Get Attachment from SF
                    GetAttachmentFromSF model = new GetAttachmentFromSF();
                    model.param = new GetAttachmentFromSFParam();
                    model.url = "https://daikin.workflowcloud.com/api/v1/workflow/published/7b256802-b3cf-4a65-a184-7bb069b79259/swagger.json?token=BNjvPZeRcGjBwL4OI3H4iy3VuO5JEJkfc0J35XdH3Rg3XuifQUmjliYtidNAJ9L0sqlUwb";
                    model.param.startData = new ParamStartData();
                    model.param.startData.se_ponumber = data.Form_No;
                    Task.Run(async () => { await GetAttachmentFromSalesForce(model); }).Wait();
                    #endregion
                }

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public static async Task GetAttachmentFromSalesForce(GetAttachmentFromSF model)
        {
            try
            {
                string requestBody = new JavaScriptSerializer().Serialize(model.param);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(model.url);
                client.DefaultRequestHeaders.Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, model.url);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<BusinessLogics.Apps.Commercials.Model.POSubconModel> ListData(ClaimReimbursement.Model.FilterHeaderSearchModel model, out int RecordCount, out decimal GrandTotal)
        {
            //usp_POSubconHeader_ListData
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_POSubconHeader_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", 10);
                db.AddInParameter(db.cmd, "FilterBy", model.FilterBy);
                db.AddInParameter(db.cmd, "StartDate", model.StartDate);
                db.AddInParameter(db.cmd, "EndDate", model.EndDate);
                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PaymentStatus", model.PaymentStatus);
                db.AddInParameter(db.cmd, "PostingStatus", model.PostingStatus);
                db.AddInParameter(db.cmd, "BranchName", model.BranchName);
                db.AddInParameter(db.cmd, "PendingApproverRoleID", model.PendingApproverRoleID);

                db.AddInParameter(db.cmd, "CurrentLogin", model.CurrentLogin);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                return dt.Rows.Count > 0 ?
                    Utility.ConvertDataTableToList<BusinessLogics.Apps.Commercials.Model.POSubconModel>(dt) :
                    new List<BusinessLogics.Apps.Commercials.Model.POSubconModel>();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }

        }

    }
}