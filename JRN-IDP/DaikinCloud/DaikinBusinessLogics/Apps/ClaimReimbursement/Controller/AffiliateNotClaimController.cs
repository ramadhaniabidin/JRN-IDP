using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Controller
{
    public class AffiliateNotClaimController
    {
        private readonly CommonLogic func = new CommonLogic();
        private readonly NintexCloudManager ntx = new NintexCloudManager();
        private readonly SharePointManager sp = new SharePointManager();
        private readonly string serverPath = HttpContext.Current.Server.MapPath("~/Commercials/");
        private readonly string urlSite = SPContext.Current.Web.Url;
        private readonly Type typeString = typeof(string);
        private readonly Type typeDecimal = typeof(decimal);
        private readonly Type typeInt = typeof(int);
        private readonly Type typeBool = typeof(bool);
        private readonly Type typeDateTime = typeof(DateTime);
        private readonly string connectionString = Utility.GetSqlConnection();
        private readonly string formUrl = "/_layouts/15/Daikin.Application/Modules/ClaimReimbursement/affiliatenotclaim.aspx?ID=";
        private readonly string MODULE_CODE = "M028";
        private readonly string MODULE_NAME = "Affiliate Not Claim";
        private readonly string MANAGER_EMAIL_KEY = "Manager Email";
        private readonly string MANAGER_NAME_KEY = "Manager Name";
        private readonly string TABLE_HEADER = "AffiliateClaimHeader";
        private const bool configureAwait = false;


        private SqlParameter CreateSQLParam(string key, Type type, object value)
        {
            SqlParameter param = new SqlParameter { ParameterName = key, Value = value, Direction = ParameterDirection.Input };
            if (type == typeof(string))
            {
                param.SqlDbType = SqlDbType.VarChar;
            }
            else if (type == typeof(int))
            {
                param.SqlDbType = SqlDbType.Int;
            }
            else if (type == typeof(decimal))
            {
                param.SqlDbType = SqlDbType.Decimal;
            }
            else if (type == typeof(DateTime))
            {
                param.SqlDbType = SqlDbType.DateTime;
            }
            else if (type == typeof(bool))
            {
                param.SqlDbType = SqlDbType.Bit;
            }
            return param;
        }

        private void SetField(SPListItem item, string fieldName, object value)
        {
            try
            {
                item[fieldName] = value;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error updating SharePoint field '{fieldName}' with value '{value}'",
                    ex
                );
            }
        }

        private int InsertToSPList(AffiliateNotClaimHeader header)
        {
            SPWeb web = new SPSite(urlSite).OpenWeb();
            SPList list = web.Lists[MODULE_NAME];
            web.AllowUnsafeUpdates = true;
            SPListItem item = header.Item_ID == 0 ? list.Items.Add() : list.GetItemById(header.Item_ID);
            item["Title"] = header.Form_No;
            item["Request Date"] = Convert.ToDateTime(header.Request_Date);
            item["Requester Account"] = header.Requester_Account;
            item["Requester Branch"] = header.Branch;
            item["Requester Department"] = header.Requester_Department;
            item["Requester Email"] = header.Requester_Email;
            item["Requester Name"] = header.Requester_Name;
            item["BPN Paid By"] = header.BPN_Paid_By;
            //item["PPJK"] = header.PPJK;
            item["Requester Business Area"] = header.Requester_Business_Area;
            item["Cost Center"] = header.Cost_Center;
            item["Document Date"] = Convert.ToDateTime(header.PPJK_Invoice_Date);
            item["Vendor Invoice No"] = header.PPJK_Invoice_No;
            item["Business Area"] = header.Business_Area_Item_ID;
            item["Texting"] = header.Texting;
            item["Approval Status"] = header.Approval_Status_Name;
            item["Approval Status ID"] = header.Approval_Status;
            item["Vendor Name"] = header.Vendor_Name;
            item["Vendor Number"] = header.Vendor_Number;
            item["Bank Key"] = header.Bank_Key;
            item["Bank Account No"] = header.Bank_Account_No;
            item["Bank Account Name"] = header.Bank_Account_Name;
            item["Bank Name"] = header.Bank_Name;
            item["Partner bank"] = header.Partner_Bank;
            item["Expense Type"] = header.Expense_Type_Item_ID;
            item["Grand Total"] = header.Grand_Total;
            item["PIB"] = header.PIB;
            item["PIB Number"] = header.PIB_Number;
            item["Total Tax Base"] = header.Total_Tax_Base;
            item["Total VAT"] = header.Total_VAT_Amount;
            item["Category"] = header.Category_Item_ID;
            item["Transaction ID"] = header.ID;
            item.Update();
            web.AllowUnsafeUpdates = false;
            return item.ID;
        }

        public void Save(AffiliateNotClaimHeader Header, List<AffiliateNotClaimDetail> Details, List<AffiliateNotClaimAttachment> attachments)
        {
            Header.Item_ID = InsertToSPList(Header);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int Header_ID = InsertHeader(conn, trans, Header);
                    InsertDetails(conn, trans, Details, Header_ID);
                    InsertAttachments(conn, trans, attachments, Header_ID, Header.Item_ID);
                    trans.Commit();
                    if (Header.Approval_Status == 5)
                    {
                        TriggerNAC(MODULE_CODE, Header.Item_ID, Header.ID, MODULE_NAME);
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task SaveAsync(AffiliateNotClaimHeader Header, List<AffiliateNotClaimDetail> Details, List<AffiliateNotClaimAttachment> attachments)
        {
            Header.Item_ID = InsertToSPList(Header);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(configureAwait);
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    int Header_ID = await InsertHeaderAsync(conn, trans, Header).ConfigureAwait(configureAwait);
                    await InsertDetailsAsync(conn, trans, Details, Header_ID).ConfigureAwait(configureAwait);
                    await InsertAttachmentsAsync(conn, trans, attachments, Header_ID, Header.Item_ID).ConfigureAwait(configureAwait);
                    trans.Commit();
                    if (Header.Approval_Status == 5) await TriggerNACAsync(MODULE_CODE, Header.Item_ID, Header.ID, MODULE_NAME).ConfigureAwait(configureAwait);
                }
            }
        }

        private void TriggerNAC(string Module_Code, int Item_ID, int Transaction_ID, string List_Name)
        {
            Task.Run(async () =>
            {
                var nwc = ntx.GenerateNACPayload(Transaction_ID, Item_ID, Module_Code, List_Name);
                await ntx.StartNWC(nwc).ConfigureAwait(configureAwait);
            }).Wait();
        }

        private async Task TriggerNACAsync(string Module_Code, int Item_ID, int Transaction_ID, string List_Name)
        {
            var nwc = await ntx.GenerateNACPayloadAsync(Transaction_ID, Item_ID, Module_Code, List_Name).ConfigureAwait(configureAwait);
            await ntx.StartNWC(nwc).ConfigureAwait(configureAwait);
        }

        private async Task TriggerNACAsync(string Module_Code, int Item_ID, int Transaction_ID, string List_Name, SqlConnection conn, SqlTransaction trans)
        {
            var nwc = await ntx.GenerateNACPayloadAsync(Transaction_ID, Item_ID, Module_Code, List_Name).ConfigureAwait(configureAwait);
            await ntx.StartNWC(nwc, conn, trans);
        }

        private void AssignHeaderParameters(SqlCommand cmd, AffiliateNotClaimHeader Header)
        {
            cmd.Parameters.Add(CreateSQLParam("@Form_No", typeString, Header.Form_No));
            cmd.Parameters.Add(CreateSQLParam("@Item_ID", typeInt, Header.Item_ID));
            cmd.Parameters.Add(CreateSQLParam("@Request_Date", typeDateTime, Convert.ToDateTime(Header.Request_Date)));
            cmd.Parameters.Add(CreateSQLParam("@Requester_Name", typeString, Header.Requester_Name));
            cmd.Parameters.Add(CreateSQLParam("@Requester_Email", typeString, Header.Requester_Email));
            cmd.Parameters.Add(CreateSQLParam("@Requester_Account", typeString, Header.Requester_Account));
            cmd.Parameters.Add(CreateSQLParam("@Department", typeString, Header.Requester_Department));
            cmd.Parameters.Add(CreateSQLParam("@Branch", typeString, Header.Branch));
            cmd.Parameters.Add(CreateSQLParam("@Business_Area", typeString, Header.Business_Area));
            cmd.Parameters.Add(CreateSQLParam("@Requester_Business_Area", typeString, Header.Requester_Business_Area));
            cmd.Parameters.Add(CreateSQLParam("@Vendor_Name", typeString, Header.Vendor_Name));
            cmd.Parameters.Add(CreateSQLParam("@Vendor_Number", typeString, Header.Vendor_Number));
            cmd.Parameters.Add(CreateSQLParam("@Bank_Key", typeString, Header.Bank_Key));
            cmd.Parameters.Add(CreateSQLParam("@Bank_Account_No", typeString, Header.Bank_Account_No));
            cmd.Parameters.Add(CreateSQLParam("@Bank_Account_Name", typeString, Header.Bank_Account_Name));
            cmd.Parameters.Add(CreateSQLParam("@Bank_Name", typeString, Header.Bank_Name));
            cmd.Parameters.Add(CreateSQLParam("@Partner_Bank", typeString, Header.Partner_Bank));
            cmd.Parameters.Add(CreateSQLParam("@Cost_Center", typeString, Header.Cost_Center));
            cmd.Parameters.Add(CreateSQLParam("@Expense_Type", typeString, Header.Expense_Type));
            cmd.Parameters.Add(CreateSQLParam("@Vendor_Invoice_No", typeString, Header.PPJK_Invoice_No));
            cmd.Parameters.Add(CreateSQLParam("@Document_Date", typeDateTime, Convert.ToDateTime(Header.PPJK_Invoice_Date)));
            cmd.Parameters.Add(CreateSQLParam("@PIB", typeString, Header.PIB ?? ""));
            cmd.Parameters.Add(CreateSQLParam("@PIB_Number", typeString, Header.PIB_Number ?? ""));
            cmd.Parameters.Add(CreateSQLParam("@BPN_Paid_By", typeString, Header.BPN_Paid_By ?? ""));
            cmd.Parameters.Add(CreateSQLParam("@Total_Tax_Base", typeDecimal, Header.Total_Tax_Base));
            cmd.Parameters.Add(CreateSQLParam("@Total_VAT", typeDecimal, Header.Total_VAT_Amount));
            cmd.Parameters.Add(CreateSQLParam("@Grand_Total", typeDecimal, Header.Grand_Total));
            cmd.Parameters.Add(CreateSQLParam("@Texting", typeString, Header.Texting));
            cmd.Parameters.Add(CreateSQLParam("@PPJK", typeString, Header.PPJK));
            cmd.Parameters.Add(CreateSQLParam("@Category", typeString, Header.Category));
        }

        private int InsertHeader(SqlConnection conn, SqlTransaction trans, AffiliateNotClaimHeader Header)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimHeader_SaveUpdate", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AssignHeaderParameters(cmd, Header);
                    SqlParameter outId = new SqlParameter("@OutID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outId);
                    cmd.ExecuteNonQuery();
                    return (int)outId.Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in InsertHeader() | {ex.Message}", ex);
            }
        }

        private async Task<int> InsertHeaderAsync(SqlConnection conn, SqlTransaction trans, AffiliateNotClaimHeader Header)
        {
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimHeader_SaveUpdate", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                AssignHeaderParameters(cmd, Header);
                SqlParameter outId = new SqlParameter("@OutID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outId);
                await cmd.ExecuteNonQueryAsync();
                return (int)outId.Value;
            }
        }

        private void InsertDetails(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimDetail> Details, int Header_ID)
        {
            try
            {
                var dt = InsertDetailGetType(Details, Header_ID);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_Save", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    var param = cmd.Parameters.AddWithValue("@Details", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "AffiliateNotClaimDetailType";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in InsertDetails() | {ex.Message}");
            }
        }

        private async Task InsertDetailsAsync(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimDetail> Details, int Header_ID)
        {
            var dt = InsertDetailGetType(Details, Header_ID);
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_Save", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var p = cmd.Parameters.AddWithValue("@Details", dt);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateNotClaimDetailType";
                await cmd.ExecuteReaderAsync().ConfigureAwait(configureAwait);
            }
        }

        private void InsertAttachments(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimAttachment> attachments, int Header_ID, int Item_ID)
        {
            try
            {
                DataTable dTable = new DataTable();
                dTable.Columns.Add("Header_ID", typeof(int));
                dTable.Columns.Add("Doc_Type", typeof(string));
                dTable.Columns.Add("Is_Mandatory", typeof(int));
                dTable.Columns.Add("Attachment_Url", typeof(string));
                dTable.Columns.Add("Attachment_Name", typeof(string));
                foreach (var att in attachments)
                {
                    if (!string.IsNullOrEmpty(att.Attachment_Name))
                    {
                        string attachment_url = $"/Lists/{MODULE_NAME}/Attachments/{Item_ID}/{att.Attachment_Name}";
                        dTable.Rows.Add(Header_ID, att.Doc_Type, att.Is_Mandatory, attachment_url, att.Attachment_Name);
                        sp.UploadFileInCustomList(MODULE_NAME, Item_ID, Path.Combine(serverPath, att.Attachment_Name), urlSite);
                    }
                }
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimSaveAttachment", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter p = cmd.Parameters.AddWithValue("@Attachments", dTable);
                    p.SqlDbType = SqlDbType.Structured;
                    p.TypeName = "AffiliateClaimAttachmentType";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in InsertAttachments() while saving Affiliate Claim Attachment records | {ex.Message}");
            }
        }

        private async Task InsertAttachmentsAsync(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimAttachment> attachments, int Header_ID, int Item_ID)
        {
            DataTable dTable = new DataTable();
            dTable.Columns.Add("Header_ID", typeInt);
            dTable.Columns.Add("Doc_Type", typeString);
            dTable.Columns.Add("Is_Mandatory", typeInt);
            dTable.Columns.Add("Attachment_Url", typeString);
            dTable.Columns.Add("Attachment_Name", typeString);
            foreach (var att in attachments)
            {
                if (!string.IsNullOrEmpty(att.Attachment_Name))
                {
                    string attachment_url = $"/Lists/{MODULE_NAME}/Attachments/{Item_ID}/{att.Attachment_Name}";
                    dTable.Rows.Add(Header_ID, att.Doc_Type, att.Is_Mandatory, attachment_url, att.Attachment_Name);
                    sp.UploadFileInCustomList(MODULE_NAME, Item_ID, Path.Combine(serverPath, att.Attachment_Name), urlSite);
                }
            }
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimSaveAttachment", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Attachments", dTable);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimAttachmentType";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(configureAwait);
            }
        }

        private DataTable InsertDetailGetType(List<AffiliateNotClaimDetail> Details, int Header_ID)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Header_ID", typeInt);
            dt.Columns.Add("No", typeInt);
            dt.Columns.Add("Jurnal_No", typeInt);
            dt.Columns.Add("GL", typeString);
            dt.Columns.Add("Material_Code", typeString);
            dt.Columns.Add("Material_Description", typeString);
            dt.Columns.Add("Vendor_Name", typeString);
            dt.Columns.Add("Vendor_No", typeString);
            dt.Columns.Add("Partner_Bank", typeString);
            dt.Columns.Add("Tax_Base", typeDecimal);
            dt.Columns.Add("VAT", typeString);
            dt.Columns.Add("VAT_Amount", typeDecimal);
            dt.Columns.Add("Tax_Invoice_No", typeString);
            dt.Columns.Add("Description", typeString);
            dt.Columns.Add("WHT", typeString);
            dt.Columns.Add("WHT_Amount", typeDecimal);
            dt.Columns.Add("Amount", typeDecimal);
            dt.Columns.Add("Recon_Account", typeString);
            dt.Columns.Add("Document_Date", typeDateTime);
            dt.Columns.Add("Is_Enable", typeBool);
            foreach (var d in Details)
            {
                dt.Rows.Add(Header_ID, d.No, d.Jurnal_No, d.GL, d.Material,
                    d.Material_Description, d.Vendor_Name, d.Vendor_Number, d.Partner_Bank,
                    d.Tax_Base, d.Tax_Code, d.VAT_Amount, d.Tax_Invoice_Number,
                    d.Texting, d.WHT_Type, d.WHT_Amount, d.Total_Amount, d.Recon_Account,
                    Convert.ToDateTime(d.Document_Date), d.Enabled);
            }
            return dt;
        }

        private DataTable UpdateRemarksGetType(List<ServiceCostRemarks> Remarks, string Form_No)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Form_No", typeString);
            dt.Columns.Add("Remarks", typeString);
            dt.Columns.Add("Outcome", typeString);
            dt.Columns.Add("Reason_Rejection", typeString);
            foreach (var remark in Remarks)
            {
                dt.Rows.
                    Add(Form_No, remark.Remarks, remark.Outcome, remark.Reason_Rejection);
            }
            return dt;
        }

        public AffiliateNotClaimHeader GetHeaderData(string Form_No)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_AffiliateNotClaimHeader_GetData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(CreateSQLParam("@Form_No", typeString, Form_No));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            return Utility.ConvertDataTableToList<AffiliateNotClaimHeader>(dt)[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in GetHeaderData() | {ex.Message}");
            }
        }

        public async Task<AffiliateNotClaimHeader> GetHeaderDataAsync(string Form_No)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync().ConfigureAwait(configureAwait);
                using (var cmd = new SqlCommand("usp_AffiliateNotClaimHeader_GetData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Form_No", typeString, Form_No));
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(configureAwait))
                    {
                        var list = await Utility.MapReaderToList<AffiliateNotClaimHeader>(reader).ConfigureAwait(configureAwait);
                        if (list != null && list.Count > 0) return list[0];
                        return null;
                    }
                }
            }
        }

        public List<AffiliateNotClaimDetail> GetDetailData(int Header_ID)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_AffiliateNotClaimDetail_GetData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            return Utility.ConvertDataTableToList<AffiliateNotClaimDetail>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in GetDetailData() | {ex.Message}");
            }
        }

        public async Task<List<AffiliateNotClaimDetail>> GetDetailDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(configureAwait);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configureAwait))
                    {
                        return await Utility.MapReaderToList<AffiliateNotClaimDetail>(r).ConfigureAwait(configureAwait);
                    }
                }
            }
        }

        public List<AffiliateNotClaimAttachment> GetAttachmentsData(int Header_ID)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_AffiliateNotClaimAttachment_GetData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            return Utility.ConvertDataTableToList<AffiliateNotClaimAttachment>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in GetAttachmentsData() | {ex.Message}");
            }
        }

        public async Task<List<AffiliateNotClaimAttachment>> GetAttachmentsDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(configureAwait);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimAttachment_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configureAwait))
                    {
                        return await Utility.MapReaderToList<AffiliateNotClaimAttachment>(r);
                    }
                }
            }
        }

        public List<ServiceCostRemarks> GetRemarksData(int Header_ID)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("[usp_AffiliateNotClaimRemarks_ListByID]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                            return Utility.ConvertDataTableToList<ServiceCostRemarks>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in GetAttachmentsData() | {ex.Message}");
            }
        }

        public async Task<List<ServiceCostRemarks>> GetRemarksDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimRemarks_ListByID", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return await Utility.MapReaderToList<ServiceCostRemarks>(r).ConfigureAwait(false);
                    }
                }
            }
        }

        public CommonResponseModel ExecuteApprovalAction(AffiliateNotClaimHeader Header, TaskActionModel Action,
            List<AffiliateNotClaimDetail> Details, List<ServiceCostRemarks> Remarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    UpdateTaskResponder(Header.ID, Action, conn, trans);
                    if (Header.Pending_Approver_Role_ID == 1)        // Receiver Document
                    {
                        UpdateDocumentReceived(Header.ID, Action.IsDocumentReceived, conn, trans);
                    }
                    if (Header.Pending_Approver_Role_ID == 48)       // Verifier Document
                    {
                        //UpdatePartnerBankID(Header.ID, Vendor_Bank, conn, trans);
                        UpdateRemarks(conn, trans, Remarks, Header.ID, Header.Form_No);
                    }
                    if (Header.Pending_Approver_Role_ID == 15)       // Tax verifier
                    {
                        UpdateWHT(conn, trans, Details, Header.ID);
                    }
                    var taskAssignmentResponse = ntx.GetTaskAssignment(Header.Task_ID, Header.Form_No);
                    var response = NintexCloudManager.ProcessNACTask(taskAssignmentResponse.TaskAssignments, Action.Approver_Email, Action.Action_Name);
                    trans.Commit();
                    return response;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception($"Error occurred ExecuteApprovalAction() | {ex.Message}");
                }
            }
        }

        public async Task<CommonResponseModel> ExecuteApprovalActionAsync(AffiliateNotClaimHeader Header, TaskActionModel Action,
            List<AffiliateNotClaimDetail> Details, List<ServiceCostRemarks> Remarks)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                using (SqlTransaction _trans = _conn.BeginTransaction())
                {
                    var taskAssignment = await ntx.GetTaskAssignmentAsync(Header.Task_ID, Header.Form_No).ConfigureAwait(false);
                    await UpdateTaskResponderAsync(Header.ID, Action, _conn, _trans).ConfigureAwait(false);
                    if (Header.Pending_Approver_Role_ID == 1) await UpdateDocumentReceivedAsync(Header.ID, Action.IsDocumentReceived, _conn, _trans).ConfigureAwait(false);
                    if (Header.Pending_Approver_Role_ID == 48) await UpdateRemarksAsync(_conn, _trans, Remarks, Header.ID, Header.Form_No).ConfigureAwait(false);
                    if (Header.Pending_Approver_Role_ID == 15) await UpdateWHTAsync(_conn, _trans, Details, Header.ID).ConfigureAwait(false);
                    var response = await NintexCloudManager.ProcessNACTaskAsync(taskAssignment.TaskAssignments, Action.Approver_Email, Action.Action_Name).ConfigureAwait(false);
                    if (response.Success) _trans.Commit();
                    return response;
                }
            }
        }

        public void UpdateTaskResponder(int Header_ID, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            try
            {
                using (var cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@List_Name", MODULE_NAME);
                    cmd.Parameters.AddWithValue("@Header_ID", Header_ID);
                    cmd.Parameters.AddWithValue("@Comments", Action.Comment);
                    cmd.Parameters.AddWithValue("@Approver_Name", Action.Approver_Name);
                    cmd.Parameters.AddWithValue("@Approver_Account", Action.Approver_Account);
                    cmd.Parameters.AddWithValue("@Approver_Email", Action.Approver_Email);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in UpdateTaskResponder() | {ex.Message}");
            }
        }

        public async Task UpdateTaskResponderAsync(int Header_ID, TaskActionModel Action, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_NonComm_CustomFormUpdateApprover", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@List_Name", MODULE_NAME);
                cmd.Parameters.AddWithValue("@Header_ID", Header_ID);
                cmd.Parameters.AddWithValue("@Comments", Action.Comment);
                cmd.Parameters.AddWithValue("@Approver_Name", Action.Approver_Name);
                cmd.Parameters.AddWithValue("@Approver_Account", Action.Approver_Account);
                cmd.Parameters.AddWithValue("@Approver_Email", Action.Approver_Email);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void UpdateDocumentReceived(int Header_ID, bool IsDocumentReceived, SqlConnection conn, SqlTransaction trans)
        {
            try
            {
                using (var cmd = new SqlCommand("usp_AffiliateNotClaim_UpdateDocumentReceived", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header_ID));
                    cmd.Parameters.Add(CreateSQLParam("@IsReceived", typeBool, IsDocumentReceived));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in UpdateDocumentReceived() | {ex.Message}");
            }
        }

        private async Task UpdateDocumentReceivedAsync(int Header_ID, bool IsDocumentReceived, SqlConnection conn, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand("usp_AffiliateNotClaim_UpdateDocumentReceived", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@ID", typeInt, Header_ID));
                cmd.Parameters.Add(CreateSQLParam("@IsReceived", typeBool, IsDocumentReceived));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void UpdateWHT(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimDetail> Details, int Header_ID)
        {
            try
            {
                var dt = InsertDetailGetType(Details, Header_ID);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_SaveWHT", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    var param = cmd.Parameters.AddWithValue("@Details", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "AffiliateNotClaimDetailType";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in UpdateWHT() | {ex.Message}");
            }
        }

        private async Task UpdateWHTAsync(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimDetail> Details, int Header_ID)
        {
            DataTable t = InsertDetailGetType(Details, Header_ID);
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_SaveWHT", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                var param = cmd.Parameters.AddWithValue("@Details", t);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "AffiliateNotClaimDetailType";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void UpdateRemarks(SqlConnection conn, SqlTransaction trans, List<ServiceCostRemarks> Remarks, int Header_ID, string Form_No)
        {
            try
            {
                var dt = UpdateRemarksGetType(Remarks, Form_No);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaim_SaveRemarks", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    var param = cmd.Parameters.AddWithValue("@Remarks", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "AffiliateNotClaimRemarksType";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred in UpdateRemarks() | {ex.Message}");
            }
        }

        private async Task UpdateRemarksAsync(SqlConnection conn, SqlTransaction trans, List<ServiceCostRemarks> Remarks, int Header_ID, string Form_No)
        {
            var dt = UpdateRemarksGetType(Remarks, Form_No);
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaim_SaveRemarks", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                var param = cmd.Parameters.AddWithValue("@Remarks", dt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "AffiliateNotClaimRemarksType";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }


    }
}
