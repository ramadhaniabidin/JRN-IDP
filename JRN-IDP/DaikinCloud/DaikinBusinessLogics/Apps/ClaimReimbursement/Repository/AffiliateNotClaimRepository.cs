using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
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

namespace Daikin.BusinessLogics.Apps.ClaimReimbursement.Repository
{
    public class AffiliateNotClaimRepository
    {
        private readonly Type typeString = typeof(string);
        private readonly Type typeDecimal = typeof(decimal);
        private readonly Type typeInt = typeof(int);
        private readonly Type typeBool = typeof(bool);
        private readonly string MODULE_NAME = "Affiliate Not Claim";
        private readonly string serverPath = HttpContext.Current.Server.MapPath("~/Commercials/");
        private readonly string urlSite = SPContext.Current.Web.Url;
        private readonly Type typeDateTime = typeof(DateTime);
        private readonly string connectionString = Utility.GetSqlConnection();
        private readonly bool configAwait = false;
        private readonly SharePointManager sp = new SharePointManager();
        private readonly DataTableHelper dtHelper = new DataTableHelper();

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

        private async Task<int> InsertHeaderAsync(SqlConnection conn, SqlTransaction trans, AffiliateNotClaimHeader header)
        {
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimHeader_SaveUpdate", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                AssignHeaderParameters(cmd, header);
                SqlParameter outId = new SqlParameter("@OutID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outId);
                await cmd.ExecuteNonQueryAsync();
                return (int)outId.Value;
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
                await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait);
            }
        }

        public async Task SaveAttachmentMetaDataAsync(SqlConnection conn, SqlTransaction trans, List<AffiliateNotClaimAttachment> attachments, int header_id, int item_id)
        {
            var dt = dtHelper.AffiliateNotClaimAttachmentTable(attachments, header_id, item_id);
            using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimSaveAttachment", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter p = cmd.Parameters.AddWithValue("@Attachments", dt);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "AffiliateClaimAttachmentType";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(configAwait);
            }
        }

        public async Task<int> SaveAsync(AffiliateNotClaimHeader header, List<AffiliateNotClaimDetail> details, List<AffiliateNotClaimAttachment> attachments)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(configAwait);
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int headerId = await InsertHeaderAsync(conn, trans, header).ConfigureAwait(configAwait);
                        await InsertDetailsAsync(conn, trans, details, headerId).ConfigureAwait(configAwait);
                        await SaveAttachmentMetaDataAsync(conn, trans, attachments, headerId, header.Item_ID).ConfigureAwait(configAwait);
                        trans.Commit();
                        return headerId;
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
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

        public async Task<AffiliateNotClaimHeader> GetHeaderDataAsync(string Form_No)
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync().ConfigureAwait(configAwait);
                using (var cmd = new SqlCommand("usp_AffiliateNotClaimHeader_GetData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Form_No", typeString, Form_No));
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait))
                    {
                        var list = await Utility.MapReaderToList<AffiliateNotClaimHeader>(reader).ConfigureAwait(configAwait);
                        if (list != null && list.Count > 0) return list[0];
                        return null;
                    }
                }
            }
        }

        public async Task<List<AffiliateNotClaimDetail>> GetDetailDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(configAwait);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimDetail_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait))
                    {
                        return await Utility.MapReaderToList<AffiliateNotClaimDetail>(r).ConfigureAwait(configAwait);
                    }
                }
            }
        }

        public async Task<List<AffiliateNotClaimAttachment>> GetAttachmentsDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(configAwait);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimAttachment_GetData", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait))
                    {
                        return await Utility.MapReaderToList<AffiliateNotClaimAttachment>(r).ConfigureAwait(configAwait);
                    }
                }
            }
        }

        public async Task<List<ServiceCostRemarks>> GetRemarksDataAsync(int Header_ID)
        {
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                await _conn.OpenAsync().ConfigureAwait(configAwait);
                using (SqlCommand cmd = new SqlCommand("usp_AffiliateNotClaimRemarks_ListByID", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateSQLParam("@Header_ID", typeInt, Header_ID));
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync().ConfigureAwait(configAwait))
                    {
                        return await Utility.MapReaderToList<ServiceCostRemarks>(r).ConfigureAwait(configAwait);
                    }
                }
            }
        }
    }
}
