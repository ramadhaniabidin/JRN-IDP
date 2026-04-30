using Daikin.BusinessLogics.Apps.NonCommercials.Model;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.NonCommercials
{
    public class ContractRepository
    {
        //private readonly DatabaseManager db = new DatabaseManager();
        private readonly DatabaseManager db;
        private readonly string connString = Utility.GetSqlConnection();
        private readonly string dateFormat = "yyMM";
        private readonly string listName = "Contract";
        private readonly string tableName = "ContractHeader";
        private readonly string moduleCode = "M014";

        public ContractRepository(DatabaseManager _db)
        {
            db = _db;
        }

        public async Task<List<MasterUserProcDept>> GetBranchesAsync()
        {
            string currentUser = SPContext.Current?.Web?.CurrentUser?.LoginName;
            var list = new List<MasterUserProcDept>();
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("dbo.usp_MasterUserProcDept_GetBranch", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(cmd, "Title", currentUser);
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                        {
                            list.Add(new MasterUserProcDept
                            {
                                Code = reader.GetString(reader.GetOrdinal("Branch_Title")),
                                Name = reader.GetString(reader.GetOrdinal("Branch_Title")),
                                Branch_ID = reader.GetInt32(reader.GetOrdinal("Branch_ID")),
                                Branch_Title = reader.GetString(reader.GetOrdinal("Branch_Title")),
                                Branch_Code = reader.GetString(reader.GetOrdinal("Branch_Code")),
                                Branch_BusinessArea = reader.GetString(reader.GetOrdinal("Branch_BusinessArea"))
                            });
                        }
                    return list;
                }
            }
        }

        public async Task<List<ContractHeader>> GetDataContractHeaderAsync(string formNo)
        {
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("dbo.usp_ContractHeader_GetData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(cmd, "Form_No", formNo);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<ContractHeader>(reader);
                    }
                }
            }
        }

        public async Task<List<ContractDetail>> GetDataContractDetailAsync(string formNo)
        {
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("dbo.usp_ContractDetail_GetData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(cmd, "Form_No", formNo);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<ContractDetail>(reader);
                    }
                }
            }
        }

        public async Task<List<ContractAttachment>> GetDataContractAttachmentAsync(string formNo)
        {
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("usp_ContractAttachment_GetData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(cmd, "Form_No", formNo);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<ContractAttachment>(reader);
                    }
                }
            }
        }

        public async Task<string> GetDataHeaderFormNo(string code)
        {
            string d = DateTime.Now.ToString(dateFormat);
            int c = 1;
            string n = c.ToString().PadLeft(4, '0');

            string formNo = await db.AutoCounterAsync("Form_No", tableName, "Form_No", code + d, 4);
            return string.IsNullOrEmpty(formNo) ? (code + d + n) : formNo;
        }

        public async Task<int> SaveHeader(SqlConnection conn, SqlTransaction trans, ContractHeader header, string currentUser)
        {
            using (var cmd = new SqlCommand("usp_ContractHeader_SaveUpdate", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                AddSaveHeaderSQLParams(cmd, header, currentUser);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var list = await Utility.MapReaderToList<ContractHeader>(reader);
                    return list[0].ID;
                }
            }
        }

        public async Task SaveDetail(SqlConnection conn, SqlTransaction trans, ContractHeader header, ContractDetail detail)
        {
            using (var cmd = new SqlCommand("usp_ContractDetail_SaveUpdate", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                AddSaveDetailSQLParams(cmd, detail, header.ID, header.Form_No, header.Contract_No);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveAttachment(SqlConnection conn, SqlTransaction trans, ContractHeader header, ContractAttachment attachment, int itemid)
        {
            using (var cmd = new SqlCommand("usp_ContractAttachment_SaveUpdate", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                AddSaveAttachmentSQLParams(cmd, attachment, header.ID, header.Form_No, itemid);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task CollectPICTeam(SqlConnection conn, SqlTransaction trans, int headerId)
        {
            using (var cmd = new SqlCommand("usp_Utility_CollectPICTeam", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(cmd, "Module_ID", moduleCode);
                db.AddInParameter(cmd, "Header_ID", headerId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteDetail(SqlConnection conn, SqlTransaction trans, string deleteDetailId)
        {
            using (var cmd = new SqlCommand("usp_ContractDetail_DeleteById", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(cmd, "ID", deleteDetailId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAttachment(SqlConnection conn, SqlTransaction trans, string deleteAttachmentId)
        {
            using (var cmd = new SqlCommand("usp_ContractAttachment_DeleteById", conn, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(cmd, "ID", deleteAttachmentId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private void AddSaveDetailSQLParams(SqlCommand cmd, ContractDetail detail, int headerid, string form_no, string contract_no)
        {
            db.AddInParameter(cmd, "ID", detail.ID);
            db.AddInParameter(cmd, "No", detail.No);
            db.AddInParameter(cmd, "Header_ID", headerid);
            db.AddInParameter(cmd, "Contract_Amount", detail.Contract_Amount);
            db.AddInParameter(cmd, "Material_Description", detail.Material_Description);
            db.AddInParameter(cmd, "Material_Number", detail.Material_Number);

            string displayName = detail.Material_Name;
            if (detail.Material_Name.Contains("-"))
            {
                string[] nameParts = detail.Material_Name.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 1) displayName = nameParts[1].Trim();
            }

            db.AddInParameter(cmd, "Material_Name", displayName);
            db.AddInParameter(cmd, "Form_No", form_no);
            db.AddInParameter(cmd, "Contract_No", contract_no);
            db.AddInParameter(cmd, "Variable_Amount", detail.Variable_Amount);
        }

        private void AddSaveHeaderSQLParams(SqlCommand cmd, ContractHeader header, string currentUser)
        {
            db.AddInParameter(cmd, "Approval_Status", header.Approval_Status);
            db.AddInParameter(cmd, "Branch", header.Branch);
            db.AddInParameter(cmd, "Contract_No", header.Contract_No);
            db.AddInParameter(cmd, "Contract_Status_ID", header.Contract_Status_ID);
            db.AddInParameter(cmd, "Contract_Status_Name", header.Contract_Status_Name);
            db.AddInParameter(cmd, "Contract_Type_ID", header.Contract_Type_ID);
            db.AddInParameter(cmd, "Contract_Type_Name", header.Contract_Type_Name);
            db.AddInParameter(cmd, "Cost_Center", header.Cost_Center);
            db.AddInParameter(cmd, "Created_By", currentUser);
            db.AddInParameter(cmd, "PIC_Team", currentUser);
            db.AddInParameter(cmd, "Form_No", header.Form_No);
            db.AddInParameter(cmd, "Internal_Order_Code", header.Internal_Order_Code);
            db.AddInParameter(cmd, "Internal_Order_Name", header.Internal_Order_Name);
            db.AddInParameter(cmd, "Document_Received", header.Document_Received);
            db.AddInParameter(cmd, "Grand_Total", header.Grand_Total);
            db.AddInParameter(cmd, "ID", header.ID);
            db.AddInParameter(cmd, "Item_ID", header.Item_ID);
            db.AddInParameter(cmd, "Modified_By", header.Modified_By);
            db.AddInParameter(cmd, "PO_Number", header.PO_Number);
            db.AddInParameter(cmd, "Period_End", header.Period_End);
            db.AddInParameter(cmd, "Period_Start", header.Period_Start);
            db.AddInParameter(cmd, "Procurement_Department", header.Procurement_Department);
            db.AddInParameter(cmd, "Remarks", header.Remarks);
            db.AddInParameter(cmd, "Request_Date", header.Request_Date);
            db.AddInParameter(cmd, "Requester_Email", header.Requester_Email);
            db.AddInParameter(cmd, "Requester_Name", header.Requester_Name);
            db.AddInParameter(cmd, "Vendor_Code", header.Vendor_Code);
            db.AddInParameter(cmd, "Vendor_Name", header.Vendor_Name);
            db.AddInParameter(cmd, "Is_New", true);
            db.AddInParameter(cmd, "Reference_No", header.Reference_No);
        }

        private void AddSaveAttachmentSQLParams(SqlCommand cmd, ContractAttachment attachment, int headerid, string form_no, int itemid)
        {
            int attachmentId = attachment.ID > 0 ? attachment.ID : 0;
            db.AddInParameter(cmd, "ID", attachmentId);
            db.AddInParameter(cmd, "Header_ID", headerid);
            db.AddInParameter(cmd, "Form_No", form_no);
            db.AddInParameter(cmd, "Attachment_FileName", attachment.Attachment_FileName);
            db.AddInParameter(cmd, "Attachment_Url", "/Lists/" + listName + "/Attachments/" + itemid.ToString() + "/" + attachment.Attachment_FileName);
        }

        public async Task InsertLogFirstSubmit(int itemId, string currentUser, string currentFullName)
        {
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("usp_NonComm_InsertApprovalLog", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(cmd, "ListName", listName);
                    db.AddInParameter(cmd, "ListItemID", itemId);
                    db.AddInParameter(cmd, "Action", 1);
                    db.AddInParameter(cmd, "CurrentLogin", currentUser);
                    db.AddInParameter(cmd, "CurrLoginName", currentFullName);
                    db.AddInParameter(cmd, "Comment", "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
