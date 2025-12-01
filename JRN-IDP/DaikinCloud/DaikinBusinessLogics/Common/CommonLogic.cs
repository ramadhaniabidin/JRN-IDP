using Daikin.BusinessLogics.Apps.ClaimReimbursement.Model;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Common
{
    public class CommonLogic
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();

        #region Daikin Used

        public void InsertLog(LogModel log)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_EventReceiverLog_Insert";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "ListName", log.ListName);
                db.AddInParameter(db.cmd, "ListItemId", log.ListItemId);
                db.AddInParameter(db.cmd, "SysMessage", log.SysMessage);
                db.AddInParameter(db.cmd, "EventName", log.EventName);
                db.AddInParameter(db.cmd, "CompiledQuery", log.CompiledQuery);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void POSubconInsertApprovalLog(string listName, int listItemID, int action, string CurrentLogin, string CurrentLoginName, string currentLayer, string comments, string approverRole)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_NWC_POSubcon_InsertHistoryLog]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "ListName", listName);
                db.AddInParameter(db.cmd, "ListItemID", listItemID);
                db.AddInParameter(db.cmd, "ApprovalCode", action);
                db.AddInParameter(db.cmd, "CurrLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrLoginName", CurrentLoginName);
                db.AddInParameter(db.cmd, "Position", approverRole);
                db.AddInParameter(db.cmd, "CurrLayer", currentLayer);
                db.AddInParameter(db.cmd, "Comment", comments);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }

            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public DataTable GetMappingAttributes(string ListName)
        {
            try
            {
                DataTable dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterMappingColumn_GetByListName";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return dt;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public DataTable GetMappingAttributeDetails(string ListName)
        {
            try
            {
                DataTable dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterMappingColumnDetail_GetByListName";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return dt;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }


        #endregion

        public string ThrowExceptionMessage(string message)
        {
            return new JavaScriptSerializer().Serialize(new
            {
                ProcessSuccess = false, InfoMessage = message
            });
        }

        public string GetUserProfile(string user_logon, string property_name)
        {
            try
            {
                string property_value = "";
                user_logon = user_logon.Substring(7).TrimStart().TrimEnd();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Temp_GetUserProfile";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "user_logon", user_logon);
                db.AddInParameter(db.cmd, "property_name", property_name);

                SqlDataReader reader = db.cmd.ExecuteReader();
                while (reader.Read())
                {
                    property_value = reader.GetString(reader.GetOrdinal("PropertyValue"));
                }

                db.CloseConnection(ref conn);
                db.CloseDataReader(reader);
                return property_value;
            }
            catch(Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }

        }

        public bool IsDataExists(string TableName, string FieldKey, string FieldValue)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_CheckExistingData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "TableName", TableName);
                db.AddInParameter(db.cmd, "FieldKey", FieldKey);
                db.AddInParameter(db.cmd, "FieldValue", FieldValue);

                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return dt.Rows.Count > 0 ? true : false;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void ExecQuery(string fullQuery)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = fullQuery;
                db.cmd.CommandType = CommandType.Text;
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn, true);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public int ExecQueryWithReturnID(string fullQuery)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = fullQuery;
                db.cmd.CommandType = CommandType.Text;

                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn, true);

                var id = dt.Rows[0].Field<int>(0);

                return id;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void InsertAutoCodeBatch(string TableName, string DetailName, int TransID, string LastName, int ItemID, string ColumnName, string Format, int LenghtOfString)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_AutoCodeBatch_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "TableName", TableName);
                db.AddInParameter(db.cmd, "DetailName", DetailName);
                db.AddInParameter(db.cmd, "TransID", TransID);
                db.AddInParameter(db.cmd, "LastName", LastName);
                db.AddInParameter(db.cmd, "ItemID", ItemID);
                db.AddInParameter(db.cmd, "ColumnName", ColumnName);
                db.AddInParameter(db.cmd, "Format", Format);
                db.AddInParameter(db.cmd, "LenghtOfString", LenghtOfString);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch(Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void InsertApprovalLog(string listName, int listItemID, int action, string CurrentLogin, string CurrentLoginName, string currentLayer, string comments)
        {
            try
            {
                db.OpenConnection(ref conn);
                if(listName.ToUpper().Contains("CONTRACT") || listName.ToUpper().Contains("PURCHASE REQUEST") || listName.ToUpper().Contains("QCF") || listName.ToUpper().Contains("PO") || listName.ToUpper().Contains("PO RELEASE"))
                {
                    db.cmd.CommandText = "[dbo].[usp_NonComm_InsertApprovalLog]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ListName", listName);
                    db.AddInParameter(db.cmd, "ListItemID", listItemID);
                    db.AddInParameter(db.cmd, "Action", action);
                    db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                    db.AddInParameter(db.cmd, "CurrLoginName", CurrentLoginName);
                    db.AddInParameter(db.cmd, "Comment", comments);

                    db.cmd.ExecuteNonQuery();
                    db.CloseConnection(ref conn);
                }

                else
                {
                    db.cmd.CommandText = "[dbo].[usp_NWC_insertHistoryLog]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "listName", listName);
                    db.AddInParameter(db.cmd, "listItemID", listItemID);
                    db.AddInParameter(db.cmd, "action", action);
                    db.AddInParameter(db.cmd, "currentLogin", CurrentLogin);
                    db.AddInParameter(db.cmd, "currentLoginName", CurrentLoginName);
                    db.AddInParameter(db.cmd, "currentLayer", currentLayer);
                    db.AddInParameter(db.cmd, "comments", comments);

                    db.cmd.ExecuteNonQuery();
                    db.CloseConnection(ref conn);
                }

            }

            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public DataTable GetMappingAdditionalSP(string ListName)
        {
            try
            {
                DataTable dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_AdditionalSP_GetByListName]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return dt;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void ExecAdditionalSP(int ListItemID, string SPName)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = SPName;
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListItemID", ListItemID);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void SaveUpdateAttachments(AttachmentModel attc, bool Insert)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Attachment_Insert";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "module_code", attc.module_code);
                db.AddInParameter(db.cmd, "transaction_code", attc.transaction_code);
                db.AddInParameter(db.cmd, "list_name", attc.list_name);
                db.AddInParameter(db.cmd, "list_item_id", attc.list_item_id);
                db.AddInParameter(db.cmd, "file_name", attc.file_name);
                db.AddInParameter(db.cmd, "file_url", attc.file_url);
                db.AddInParameter(db.cmd, "uploaded_by", attc.uploaded_by);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<OptionModel> DropDownLists(string Table, int NeedFirstOption, string FirstDescription, string FirstValue, string FieldCode, string FieldName)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_DropDowns";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Table", Table);
                db.AddInParameter(db.cmd, "NeedFirstOption", NeedFirstOption);
                db.AddInParameter(db.cmd, "FirstDescription", FirstDescription);
                db.AddInParameter(db.cmd, "FirstValue", FirstValue);
                db.AddInParameter(db.cmd, "FieldCode", FieldCode);
                db.AddInParameter(db.cmd, "FieldName", FieldName);

                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<OptionModel>(dt);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public DataTable GetMappingAttributeDetailsNonCom(string ListName, int isMarketing)
        {
            try
            {
                DataTable dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterMappingColumnDetailNonCom_GetByListName";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                db.AddInParameter(db.cmd, "isMarketing", isMarketing);
                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                return dt;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void AutoCodeBatch(string ListName, int ItemID, int TransID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_AutoCode_Insert";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                db.AddInParameter(db.cmd, "ItemID", ItemID);
                db.AddInParameter(db.cmd, "TransID", TransID);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }
        
        public string GetModuleCode(string ListName)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                db.cmd.CommandText = "usp_MasterModule_GetModuleCode";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(db.cmd, "List_Name", ListName);

                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn, true);

                var Module_Code = dt.Rows[0].Field<string>(0);

                return Module_Code;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<CheckApproval> CheckApproval(string ListName)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Approval_CheckApprovalbyListName";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                //db.AddInParameter(db.cmd, "TransactionId", TransactionID);

                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                for (var colIdx = 0; colIdx < dt.Columns.Count; colIdx++)
                {
                    if (dt.Rows[0][colIdx] == System.DBNull.Value)
                    {
                        dt.Rows[0][colIdx] = "";
                    }
                }

                var approvalCheck = Utility.ConvertDataTableToList<CheckApproval>(dt);
                return approvalCheck;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<GetApprovalData> GetApprovalData(string ModuleCode, int ListItemID, int HeaderID)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Approval_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ModuleCode", ModuleCode);
                db.AddInParameter(db.cmd, "ListItemID", ListItemID);
                db.AddInParameter(db.cmd, "HeaderID", HeaderID);

                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                if (dt.Rows.Count > 0)
                {
                    for (var rowIdx = 0; rowIdx < dt.Rows.Count; rowIdx++)
                    {
                        for (var colIdx = 0; colIdx < dt.Columns.Count; colIdx++)
                        {
                            if (dt.Rows[rowIdx][colIdx] == System.DBNull.Value)
                            {
                                dt.Rows[rowIdx][colIdx] = "";
                            }
                        }
                    }
                }

                var approvalData = Utility.ConvertDataTableToList<GetApprovalData>(dt);
                return approvalData;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public void ApprovalLog(string ModuleCode, int HeaderID, string FormNo, string PICName, string PICUsername, DateTime ActionDate, string PICRole, string ApproverGroup)
        {
            try
            {
                db.OpenConnection(ref conn);

                db.cmd.CommandText = "usp_Approval_Log";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ModuleCode", ModuleCode);
                db.AddInParameter(db.cmd, "TransactionHeaderID", HeaderID);
                db.AddInParameter(db.cmd, "FormNo", FormNo);
                db.AddInParameter(db.cmd, "ApprovalListItemID", 0);
                db.AddInParameter(db.cmd, "PICName", PICName);
                db.AddInParameter(db.cmd, "PICUsername", PICUsername);
                db.AddInParameter(db.cmd, "ActionID", 0);
                db.AddInParameter(db.cmd, "ActionName", "Submit Revise");
                db.AddInParameter(db.cmd, "ActionDate", ActionDate);
                db.AddInParameter(db.cmd, "Comments", "");
                db.AddInParameter(db.cmd, "CurrentLayer", 0);
                db.AddInParameter(db.cmd, "PICRole", PICRole);
                db.AddInParameter(db.cmd, "ApproverGroup", ApproverGroup);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<GeneralHistoryLogModel> GetHistoryLog(string Form_No, string ModuleCode, int Transaction_ID)
        {

            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_GetHistoryLogByTransId";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Form_No", Form_No);
                db.AddInParameter(db.cmd, "Module_Code", ModuleCode);
                db.AddInParameter(db.cmd, "Transaction_ID", Transaction_ID);

                SqlDataReader reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<GeneralHistoryLogModel>(dt);


            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<string> GetXMLAttributesValue(string listName, int itemID, string attributeName)
        {
            List<string> listValue = new List<string>();
            SPWeb web = new SPSite(Utility.SpSiteUrl).OpenWeb();
            SPList list = web.Lists[listName];
            SPListItem listItem = list.GetItemById(itemID);
            string xmlDetails = listItem["Details"].ToString();
            if (!string.IsNullOrEmpty(xmlDetails))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlDetails);
                XmlNodeList nodes = xmlDocument.SelectNodes($"//{attributeName}");
                foreach (XmlNode node in nodes)
                {
                    if (node != null && node.InnerText != null)
                    {
                        string decodedValue = WebUtility.HtmlDecode(node.InnerText);
                        decodedValue = Regex.Unescape(decodedValue);
                        decodedValue = decodedValue.Replace("\u0026", "&");
                        listValue.Add(decodedValue);
                    }
                }
            }

            return listValue;
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
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public string CreateEmailBody(string PO_Number, string Requester_Name)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"Dear {Requester_Name}, <br><br>" 
                + $"PO Subcon No. <strong>{PO_Number}</strong> is ready to MIGO <br><br>"
                + "Regards, <br>"
                + "Administrator");
            return sb.ToString();
        }

        public void SendEmail(string Receiver, string Base64pdf, string PO_Number, string Requester_Name)
        {
            try
            {
                byte[] pdfBytes = Convert.FromBase64String(Base64pdf);
                MemoryStream pdfStream = new MemoryStream(pdfBytes);
                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(pdfStream, $"{PO_Number}.pdf", "application/pdf");
                MailMessage message = new MailMessage
                {
                    From = new MailAddress("no-reply@daikin.co.id"),
                    Subject = $"{PO_Number} - Ready For MIGO",
                    Body = CreateEmailBody(PO_Number, Requester_Name),
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(Receiver));
                message.Attachments.Add(attachment);
                using (var smtpClient = new SmtpClient("mail3.daikin.co.id"))
                {
                    smtpClient.Port = 25;
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(message);
                    pdfStream.Dispose();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ShowButtonExcel(string CurrLogin)
        {
            bool show = false;
            using (var con = new SqlConnection(Utility.GetSQLConnDev()))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_GetRuleButtonExcel", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CurrLogin", CurrLogin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            show = reader.GetBoolean(0);
                        }
                        reader.Close();
                    }
                }
                con.Close();
                con.Dispose();
            }
            return show;
        }

        public ListItemModel GetListItemProperties(string ListName, int ItemID)
        {
            SPWeb web = SPContext.Current.Web;
            SPList list = web.Lists[ListName];
            SPListItem listItem = list.GetItemById(ItemID);
            return new ListItemModel
            {
                formStatus = listItem["Form_x0020_Status"] == null ? "" : listItem["Form_x0020_Status"].ToString(),
                approvalStatus = listItem["Approval_x0020_Status"] == null ? "" : listItem["Approval_x0020_Status"].ToString(),
                workflowStatus = listItem["Workflow_x0020_Status"] == null ? "" : listItem["Workflow_x0020_Status"].ToString(),
                formNo = listItem["Title"].ToString(),
                transID = (ListName.ToUpper().Contains("QCF") || ListName.Contains("Purchase Request")) ? listItem["Transaction_x0020_ID"].ToString() : "0",
                vendorName = list.Fields.ContainsField("Vendor_x0020_Name") ? listItem["Vendor_x0020_Name"].ToString() : "",
                //currentLogin = Utility.GetSPUser(listItem, "Created By").LoginName,
                //currentLoginName = Utility.GetSPUser(listItem, "Created By").Name,
                //taskResponderEmail = listItem["Task_x0020_Responder_x0020_Email"] == null ? "" : listItem["Task_x0020_Responder_x0020_Email"].ToString(),
                //taskResponderName = listItem["Task_x0020_Responder_x0020_Name"] == null ? "" : listItem["Task_x0020_Responder_x0020_Name"].ToString(),
                //taskResponderLogin = listItem["Task_x0020_Responder_x0020_Login"] == null ? "" : listItem["Task_x0020_Responder_x0020_Login"].ToString(),
                //submittedBy = listItem["Submitted_x0020_By"] == null ? Utility.GetSPUser(listItem, "Created By").LoginName : listItem["Submitted_x0020_By"].ToString()
            };
        }

        public ListItemModel GetTaskResponder(string ListName, int ItemID)
        {
            string responderEmailKey = "";
            string responderNameKey = "";
            string responderLoginKey = "";
            SPListItem listItem = SPContext.Current.Web.Lists[ListName].GetItemById(ItemID);
            if(ListName.ToUpperInvariant().Contains("AFFILIATE"))
            {
                responderEmailKey = "Task_x0020_Responder_x0020_Email";
                responderNameKey = "Task_x0020_Responder_x0020_Name";
                responderLoginKey = "Task_x0020_Responder_x0020_Login";
            }
            return new ListItemModel
            {
                taskResponderEmail = listItem[responderEmailKey].ToString(),
                taskResponderName = listItem[responderNameKey].ToString(),
                taskResponderLogin = listItem[responderLoginKey].ToString()
            };
        }

        public ListItemModel GetItemSubmitter(string ListName, int ItemID)
        {
            SPWeb web = SPContext.Current.Web;
            SPListItem listItem = web.Lists[ListName].GetItemById(ItemID);
            bool specialCondition = ListName.ToUpperInvariant().Contains("QCF") || ListName.ToUpperInvariant().Contains("PO RELEASE");
            return new ListItemModel
            {
                currentLogin = specialCondition ? web.EnsureUser(listItem["Submitted_x0020_By"].ToString()).LoginName : Utility.GetSPUser(listItem, "Created By").LoginName,
                currentLoginName = specialCondition ? web.EnsureUser(listItem["Submitted_x0020_By"].ToString()).Name : Utility.GetSPUser(listItem, "Created By").Name
            };
        }

        public string QueryInsertHeader(SPListItem Item, DataTable Attributes)
        {
            string query = "";
            for(int i = 0; i < Attributes.Rows.Count; i++)
            {
                var row = Attributes.Rows[i];
                if(i == 0)
                {
                    query = "INSERT INTO " + Utility.GetStringValue(row, "Table_Header") + "(" + Utility.GetStringValue(row, "Database_Column_Name");
                }
                else
                {
                    query += "," + Utility.GetStringValue(row, "Database_Column_Name");
                }
            }
            query += ", Approval_Status, PIC_Team)";
            return query;
        }

        public string QueryInsertValues(SPListItem Item, DataTable Attributes, string FormStatus)
        {
            List<string> values = new List<string>();
            foreach(DataRow row in Attributes.Rows)
            {
                values.Add(Utility.getColumnValue(Utility.GetIntValue(row, "Sharepoint_Column_Type"), row, Item));
            }
            if (FormStatus == "Start") values.Add("2");
            else if (FormStatus == "Draft") values.Add("8");
            values.Add("'" + Utility.GetSPUser(Item, "Created By").LoginName + "'");
            return "(" + string.Join(",", values) + ")";
        }

        public string QueryUpdateHeader(SPListItem Item, DataTable Attributes, string TransID, string ListName, int Item_ID)
        {
            string query = string.Empty;
            for(int i = 0; i < Attributes.Rows.Count; i++)
            {
                var row = Attributes.Rows[i];
                int SPColumnType = Utility.GetIntValue(row, "Sharepoint_Column_Type");
                if(i == 0)
                {
                    query = "UPDATE " + Utility.GetStringValue(row, "Table_Header") + " SET " + Utility.GetStringValue(row, "Database_Column_Name") + " = " + Utility.getColumnValue(SPColumnType, row, Item);
                }
                else
                {
                    query += ", " + Utility.GetStringValue(row, "Database_Column_Name") + "=" + Utility.getColumnValue(SPColumnType, row, Item);
                }
            }
            bool cond = (ListName.ToUpperInvariant().Contains("QCF") || ListName.ToUpperInvariant().Contains("PURCHASE REQUEST"));
            query += cond ? $" OUTPUT INSERTED.ID WHERE Item_ID = {Item_ID} AND ID = {TransID} AND Is_New = 1" : $" OUTPUT INSERTED.ID WHERE Item_ID = {Item_ID} AND Is_New = 1";
            return query;
        }

        public int SaveUpdateHeader(SPListItem Item, DataTable Attributes, string TransID, string ListName, int Item_ID, string FormStatus, string ApprovalStatus)
        {
            string query = "";
            if((FormStatus == "Start" || FormStatus == "Draft") && string.IsNullOrEmpty(ApprovalStatus))
            {
                query = QueryInsertHeader(Item, Attributes) + " OUTPUT INSERTED.ID VALUES " + QueryInsertValues(Item, Attributes, FormStatus);
            }
            else
            {
                query = QueryUpdateHeader(Item, Attributes, TransID, ListName, Item_ID);
            }
            return ExecQueryWithReturnID(query);
        }

        public string QuerySaveUpdateHeader(SPListItem Item, DataTable Attributes, string TransID, string ListName, int Item_ID, string FormStatus, string ApprovalStatus)
        {
            string query = "";
            if ((FormStatus == "Start" || FormStatus == "Draft") && string.IsNullOrEmpty(ApprovalStatus))
            {
                query = QueryInsertHeader(Item, Attributes) + " OUTPUT INSERTED.ID VALUES " + QueryInsertValues(Item, Attributes, FormStatus);
            }
            else
            {
                query = QueryUpdateHeader(Item, Attributes, TransID, ListName, Item_ID);
            }
            return query;
        }

        public DefaultAttrModel GenerateAttribute(int Header_ID, string Code, int Item_ID, string Table_Name, string SPColumnName, string XML_string)
        {
            XElement root = XElement.Parse(XML_string);
            foreach(var item in root.Descendants("Item"))
            {
                if (XMLManager.GetValue(item, "ID") == "NOT EXISTS") XMLManager.AddAttribute(item, "ID", "");
                if (XMLManager.GetValue(item, "Header_ID") == "NOT EXISTS") XMLManager.AddAttribute(item, "Header_ID", Header_ID.ToString());
                if (string.IsNullOrEmpty(XMLManager.GetValue(item, "ID"))) item.Element("ID").Value = Guid.NewGuid().ToString();
            }
            return new DefaultAttrModel
            {
                code = Code, item_id = Item_ID, TableName = Table_Name, SPColumnName = SPColumnName, XMLString = root.ToString()
            };
        }

        public DataTable GetDistinctMappings(DataTable dtAttrDetails)
        {
            DataView view = new DataView(dtAttrDetails);
            return view.ToTable(true, "Sharepoint_Column", "Database_Column", "Table_Detail");
        }

        public DefaultAttrModel BuildDefaultAttrModel(SPListItem listItem, DataTable distinctValues)
        {
            return new DefaultAttrModel
            {
                code = listItem["Title"].ToString(),
                item_id = listItem.ID,
                TableName = Utility.GetStringValue(distinctValues.Rows[0], "Table_Detail"),
                SPColumnName = "Details",
                XMLString = listItem["Details"].ToString()
            };
        }

        public DefaultAttrModel BuildDefaultAttrModel_V2(SPListItem listItem, DataTable distinctValues, string detailsColumn)
        {
            return new DefaultAttrModel
            {
                code = listItem["Title"].ToString(),
                item_id = listItem.ID,
                TableName = Utility.GetStringValue(distinctValues.Rows[0], "Table_Detail"),
                SPColumnName = detailsColumn,
                XMLString = listItem[detailsColumn].ToString()
            };
        }

        public DataTable BuildFilteredMappingTable(DataTable dtAttrDetails)
        {
            DataTable dtFilter = dtAttrDetails.Clone();
            if (!dtAttrDetails.AsEnumerable().Any(r => r["Database_Column"].ToString() == "ID"))
            {
                dtFilter.Rows.Add("", "ID", "ID");
            }
            if (!dtAttrDetails.AsEnumerable().Any(r => r["Database_Column"].ToString() == "Header_ID"))
            {
                dtFilter.Rows.Add("", "Header_ID", "Header_ID");
            }
            foreach (DataRow row in dtAttrDetails.Rows)
            {
                dtFilter.ImportRow(row);
            }               
            return dtFilter;
        }

        public string EnsureXmlHasIds(string xmlString, object headerId)
        {
            XElement root = XElement.Parse(xmlString);
            foreach (var item in root.Descendants("Item"))
            {
                if (XMLManager.GetValue(item, "ID") == "NOT EXISTS")
                {
                    XMLManager.AddAttribute(item, "ID", "");
                }
                if (XMLManager.GetValue(item, "Header_ID") == "NOT EXISTS")
                {
                    XMLManager.AddAttribute(item, "Header_ID", headerId.ToString());
                }
                if (string.IsNullOrEmpty(XMLManager.GetValue(item, "ID")))
                {
                    item.Element("ID").Value = Guid.NewGuid().ToString();
                }
            }
            return root.ToString();
        }

        public void DeleteExistingDetailRecords(string tableName, int headerId)
        {
            string deleteQuery = $"DELETE {tableName} WHERE Header_ID = {headerId}";
            ExecQuery(deleteQuery);
        }

        public void ExecuteDetailQueries(List<string> queries)
        {
            foreach (string query in queries)
            {
                ExecQuery(query);
            }
        }

        public void ExecuteQueryDetail(List<string> queries, string ListName, int ItemID)
        {
            string sysMessage = "";
            string compliedQuery = "";
            for (int i = 0; i < queries.Count; i++)
            {
                try
                {
                    sysMessage = $"Insert detail row number: {i + 1} - Success";
                    ExecQuery(queries[i]);
                }
                catch (Exception ex)
                {
                    sysMessage = $"Insert detail row number: {i + 1} - failed | {ex.GetType().Name} : {ex.Message}";
                    compliedQuery = queries[i];
                }
                InsertLog(new LogModel
                {
                    EventName = $"Insert Detail | Row number: {i + 1}",
                    CompiledQuery = compliedQuery,
                    ListName = ListName,
                    ListItemId = ItemID,
                    SysMessage = sysMessage
                });
            }
        }

        public List<string> GenerateQueryDetails(DataTable AttributeDetails, SPListItem ListItem, ListItemModel ItemProperties, string ListName, int ItemID, int HeaderID, bool DeleteExisting, string SPListDetailColumn)
        {
            try
            {
                DataTable distinctValues = GetDistinctMappings(AttributeDetails);
                DefaultAttrModel attr = BuildDefaultAttrModel_V2(ListItem, distinctValues, SPListDetailColumn);
                DataTable dtAttrDetailsFilter = BuildFilteredMappingTable(AttributeDetails);
                attr.XMLString = EnsureXmlHasIds(attr.XMLString, HeaderID);
                List<string> listQueryDetails = XMLManager.ConvertDataTableToSQL(dtAttrDetailsFilter, attr, ItemProperties.vendorName);
                if(DeleteExisting)
                {
                    DeleteExistingDetailRecords(attr.TableName, HeaderID);
                }
                return listQueryDetails;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public void FirstSubmitAction(string ListName, int ItemID, int HeaderID, SPListItem ListItem, string CurrentLogin, string CurrentLoginName)
        {
            AutoCodeBatch(ListName, ItemID, HeaderID);
            string comment = ListName.ToUpper().Contains("PURCHASE REQUEST") ? ListItem["Purpose"].ToString() : "";
            InsertApprovalLog(ListName, ItemID, 1, CurrentLogin, CurrentLoginName, "0", comment);
        }

        //public void SubmitReviseAction(string)
    }
}
