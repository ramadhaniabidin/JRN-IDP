using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Apps.NonCommercials.Controller;
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

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class FOBController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        string SPList = "Commercials";
        SharePointManager sp = new SharePointManager();
        private readonly NintexCloudManager nintexCloudManager = new NintexCloudManager();
        public void FOB_PostToSAP()
        {
            try
            {
                //Buat Logic create txt file untuk put di shared folder
                //Update flag Post_To_SAP = 1, Post_Date = GETDATE()
                //di-proses secara berkala oleh Task Scheduler
                //Berdasarkan Approval_Status = '7' dan Post_To_SAP = 0
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void UpdateRemarks(List<FOBRemarksModel> listRemarks, string CurrentLoginName)
        {
            try
            {
                db.OpenConnection(ref conn, true);
                foreach (FOBRemarksModel r in listRemarks)
                {
                    //if (string.IsNullOrEmpty(r.Outcome) && string.IsNullOrEmpty(r.Reason_Rejection))
                    //{
                    //    db.CloseConnection(ref conn);
                    //    throw new Exception("Please tick if OK for " + r.Remarks + "\n if not, then specify the reason of rejection");
                    //}

                    db.cmd.CommandText = "dbo.[usp_FOBRemarks_SaveUpdate]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Outcome", r.Outcome);
                    db.AddInParameter(db.cmd, "Reason", r.Reason);
                    db.AddInParameter(db.cmd, "ID", r.ID);
                    db.AddInParameter(db.cmd, "Modified_By", CurrentLoginName);

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
            catch (Exception ex)
            {
                return new CommonResponseModel
                {
                    Success = false,
                    Message = $"Error in ApprovalAction method in ServiceCostController | {ex.Message}"
                };
            }
        }

        public List<Common.Model.OptionModel> ListCurrency()
        {
            try
            {
                List<Common.Model.OptionModel> list = new List<Common.Model.OptionModel>();
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_DDLCurrency";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseConnection(ref conn);

                Common.Model.OptionModel data = new Common.Model.OptionModel();
                data.Code = "";
                data.Name = "Please Select";
                list.Add(data);
                foreach (DataRow row in dt.Rows)
                {
                    data = new Common.Model.OptionModel();
                    data.Code = Utility.GetStringValue(row, "Currency");
                    data.Name = Utility.GetStringValue(row, "Currency");
                    list.Add(data);
                }
                return list;

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<FOBDetailModel> ListDetail(int ID, int PageIndex, int PageSize, out int RecordCount)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_FOBDetail_ListData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Header_ID", ID);
                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "PageSize", PageSize);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                db.CloseConnection(ref conn);
                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<FOBDetailModel>(dt);
                }
                else
                {
                    return new List<FOBDetailModel>();
                }

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public FOBHeaderModel GetData(string Form_No)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);

                db.cmd.CommandText = "dbo.usp_FOBHeader_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Form_No", Form_No);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<FOBHeaderModel>(dt)[0];
                }
                else
                {
                    return new FOBHeaderModel();
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public CommonSaveResponseModel SaveSPList(string SiteUrl, FOBHeaderModel h, string Status)
        {
            int ListItemId = Convert.ToInt32(h.Item_ID);
            SPWeb web = new SPSite(SiteUrl).OpenWeb();
            SPList list = web.Lists["FOB"];
            try
            {
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
                    item["Form Type"] = "FOB";
                    item["Status"] = Status;
                    item["Current Layer"] = 0;
                    item["Grand Total"] = h.Grand_Total;
                }
                else
                {
                    item = list.GetItemById(ListItemId);
                    if (h.ID > 0) item["Transaction ID"] = h.ID;
                    item["Status"] = Status;
                    item["Notify"] = "0"; //Sending EMail to Requestor after submit
                    item["Grand Total"] = h.Grand_Total;
                }
                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;
                return new CommonSaveResponseModel { ID = ListItemId, Success = true, Message = "Save SP List FOB OK" };
            }
            catch(Exception ex)
            {
                web.AllowUnsafeUpdates = false;
                return new CommonSaveResponseModel { ID = 0, Success = false, Message = $"Error save SP List FOB method : {ex.Message}" };
            }


        }

        public List<FOBRemarksModel> ListRemarks(int Header_ID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_FOBRemarks_ListById";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);

                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<FOBRemarksModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }

        }

        public string GenerateFormNo(string Form_No, string Table_Header, string Format_Code, int Length)
        {
            try
            {
                if (!string.IsNullOrEmpty(Form_No)) return Form_No;
                db.OpenConnection(ref conn);
                string Generated_Code = db.Autocounter("Form_No", Table_Header, "Form_No", Format_Code, 4);
                db.CloseConnection(ref conn);
                return Generated_Code;
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public SaveHeaderFOBModel SaveHeader(FOBHeaderModel h, List<FOBDetailModel> listDetail, string SiteUrl)
        {
            try
            {
                h.Form_No = GenerateFormNo(h.Form_No, "FOBHeader", $"FB{DateTime.Now.ToString("yyMM")}", 4);
                h.Grand_Total = listDetail.Sum(s => s.Amount_In_Local_Curr);
                var SaveSPListResponse = SaveSPList(SiteUrl, h, "-");
                if (!SaveSPListResponse.Success)
                {
                    return new SaveHeaderFOBModel { ID = 0, Success = false, Header = h, Message = SaveSPListResponse.Message };
                }
                h.Item_ID = SaveSPListResponse.ID; //1 Trigger WF
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_FOBHeader_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                db.AddInParameter(db.cmd, "Requester_Name", h.Requester_Name);
                db.AddInParameter(db.cmd, "Requester_Email", h.Requester_Email);
                db.AddInParameter(db.cmd, "Factory", h.Factory);
                db.AddInParameter(db.cmd, "Due_On", h.Due_On);
                db.AddInParameter(db.cmd, "Grand_Total", h.Grand_Total);
                db.AddInParameter(db.cmd, "Currency", h.Currency);
                db.AddInParameter(db.cmd, "Plant_Code", h.Plant_Code);
                db.AddInParameter(db.cmd, "Plant_Name", h.Plant_Name);
                db.AddInParameter(db.cmd, "Item_ID", h.Item_ID);
                db.AddInParameter(db.cmd, "Approval_Status", h.Approval_Status);
                db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                db.AddInParameter(db.cmd, "OA_Summary_Attachment", "/Lists/FOB/Attachments/" + h.Item_ID.ToString() + "/" + h.OA_Summary_FileName);
                db.AddInParameter(db.cmd, "OA_Summary_FileName", h.OA_Summary_FileName);
                db.AddInParameter(db.cmd, "Is_New", SiteUrl.ToUpperInvariant().Contains("3473"));

                int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());
                h.ID = Header_ID;
                db.CloseConnection(ref conn);
                return new SaveHeaderFOBModel { ID = Header_ID, Header = h, Message = "Save FOB header OK", Success = true };
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                return new SaveHeaderFOBModel { ID = 0, Header = h, Message = $"Error at Save Header method | {ex.Message}", Success = false };
            }
        }

        public CommonResponseModel SaveRemarks(bool IsNew, FOBHeaderModel h, List<FOBRemarksModel> listRemarks, SPWeb web)
        {
            try
            {
                var listMasterRemarks = new Utility().ListRemarks(web, "FOB");
                db.OpenConnection(ref conn);
                if (IsNew)
                {
                    foreach (string s in listMasterRemarks)
                    {
                        db.cmd.CommandText = "usp_FOBRemarks_SaveUpdate";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();
                        db.AddInParameter(db.cmd, "Header_ID", h.ID);
                        db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                        db.AddInParameter(db.cmd, "Remarks", s);
                        db.AddInParameter(db.cmd, "Reason", "");
                        db.AddInParameter(db.cmd, "Outcome", "");
                        db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                        db.cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    foreach (FOBRemarksModel s in listRemarks)
                    {
                        db.cmd.CommandText = "usp_FOBRemarks_SaveUpdate";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();
                        db.AddInParameter(db.cmd, "Header_ID", h.ID);
                        db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                        db.AddInParameter(db.cmd, "Remarks", s.Remarks);
                        db.AddInParameter(db.cmd, "Reason", s.Reason);
                        db.AddInParameter(db.cmd, "Outcome", s.Outcome);
                        db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                        db.cmd.ExecuteNonQuery();
                    }
                }
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = true, Message = "OK" };
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = false, Message = $"Error occured at Save Remarks method | {ex.Message}" };
            }
        }

        public CommonResponseModel SaveDetail(FOBHeaderModel h, List<FOBDetailModel> listDetail, int Header_ID, string ServerPath, string SiteUrl)
        {
            try
            {
                int No = 1;
                db.OpenConnection(ref conn);
                foreach (FOBDetailModel d in listDetail)
                {
                    db.cmd.CommandText = "usp_FOBDetail_SaveUpdate";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "No", No);
                    db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                    db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                    db.AddInParameter(db.cmd, "Document_No", d.Document_No);
                    db.AddInParameter(db.cmd, "Business_Place_Code", d.Business_Place_Code);
                    db.AddInParameter(db.cmd, "Business_Place_Name", d.Business_Place_Name);
                    db.AddInParameter(db.cmd, "Text", d.Text);
                    db.AddInParameter(db.cmd, "Document_Date", d.Document_Date);
                    db.AddInParameter(db.cmd, "Posting_Date", d.Posting_Date);
                    db.AddInParameter(db.cmd, "Net_Due_Date", d.Net_Due_Date);
                    db.AddInParameter(db.cmd, "Amount", d.Amount);
                    db.AddInParameter(db.cmd, "Amount_In_Local_Curr", d.Amount_In_Local_Curr);
                    db.AddInParameter(db.cmd, "Currency", d.Currency);
                    db.AddInParameter(db.cmd, "File_Name", d.File_Name);
                    db.AddInParameter(db.cmd, "Attachment_Url", "/Lists/FOB/Attachments/" + h.Item_ID.ToString() + "/" + d.File_Name);
                    db.AddInParameter(db.cmd, "Reference", d.Reference);
                    db.cmd.ExecuteNonQuery();
                    var dPathFile = ServerPath + d.File_Name;
                    sp.UploadFileInCustomList("FOB", h.Item_ID, dPathFile, SiteUrl);
                    No++;
                }
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = true, Message = "OK" };
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonResponseModel { Success = false, Message = $"Error at Save Detail method | {ex.Message}"};
            }
        }

        public CommonSaveResponseModel Save(string SiteUrl, FOBHeaderModel h, List<FOBDetailModel> listDetail, List<FOBRemarksModel> listRemarks, string ServerPath, SPWeb web)
        {
            try
            {
                bool IsNew = string.IsNullOrEmpty(h.Form_No);
                var saveHeaderResponse = SaveHeader(h, listDetail, SiteUrl);
                if (!saveHeaderResponse.Success) return new CommonSaveResponseModel { Success = false, Message = saveHeaderResponse.Message, ID = saveHeaderResponse.ID };

                #region Upload OA Summary Attachment
                string PathFile = ServerPath + h.OA_Summary_FileName;
                sp.UploadFileInCustomList("FOB", h.Item_ID, PathFile, SiteUrl);
                #endregion

                var saveRemarksResponse = SaveRemarks(IsNew, h, listRemarks, web);
                if (!saveRemarksResponse.Success) return new CommonSaveResponseModel { Success = false, ID = 0, Message = saveRemarksResponse.Message };

                var saveDetailResponse = SaveDetail(h, listDetail, saveHeaderResponse.ID, ServerPath, SiteUrl);
                if (!saveDetailResponse.Success) return new CommonSaveResponseModel { Success = false, ID = 0, Message = saveDetailResponse.Message };
                InsertHistoryLog(saveHeaderResponse.Header.ID, saveHeaderResponse.Header.Form_No, 1, sp.GetCurrentUserLogin(SiteUrl), sp.GetCurrentLoginFullName(SiteUrl), "");

                string NAC_WorkflowId = "";
                DataTable dtMenu = new FinanceMenu.Controller.GeneralController().GetDetailMenuByCode("M010");
                foreach (DataRow row in dtMenu.Rows)
                {
                    NAC_WorkflowId = Utility.GetStringValue(row, "NAC_Workflow_ID");
                }

                Task.Run(async () => { await nintexCloudManager.Commercial_StartWorkflow(
                    saveHeaderResponse.Header.Item_ID, saveHeaderResponse.Header.ID, "M010", NAC_WorkflowId); }).Wait();
                return new CommonSaveResponseModel { Success = true, Message = "OK", ID = saveHeaderResponse.ID };
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonSaveResponseModel { Success = false, Message = $"Error at Save method | {ex.Message}" };
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
                db.AddInParameter(db.cmd, "Module_Code", "M010");
                db.AddInParameter(db.cmd, "Action", Action);
                db.AddInParameter(db.cmd, "CurrentLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "CurrLoginName", CurrentLoginName);
                db.AddInParameter(db.cmd, "Comment", Comment);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn, true);
            }
            catch (Exception ex)
            {
                SaveDataLog("M010", Header_ID, Form_No, $"Error insert history log for Service Cost: {ex.Message}");
                db.CloseConnection(ref conn, true);
            }
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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                Console.WriteLine($"Error saving log: {ex.Message}");
            }
        }

        public List<FOBDetailModel> ListOutstandingAP(int PageIndex, string DueOn, string TradingPartnerCode, string Curr, out int RecordCount, out decimal GrandTotal)
        {
            try
            {
                var listBussPlace = new FinanceMenu.Controller.GeneralController().BindingMasterDatabase("MasterBussPlace", "Title", "Name", "Please Select");

                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_OutstandingAP_List";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "DueOn", DueOn);
                db.AddInParameter(db.cmd, "TradingPartnerCode", TradingPartnerCode);
                db.AddInParameter(db.cmd, "Curr", Curr);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);


                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);

                db.CloseConnection(ref conn);
                dt.Columns["Business_Place_Name"].ReadOnly = false;
                dt.Columns["Business_Place_Name"].MaxLength = 100;
                foreach (DataRow row in dt.Rows)
                {
                    string BP_Code = Utility.GetStringValue(row, "Business_Place_Code");
                    var BP_Name = listBussPlace.Where(w => w.Code.ToUpper() == BP_Code.ToUpper());
                    if (BP_Name.Any())
                    {
                        string Name = BP_Name.FirstOrDefault().Name;
                        row["Business_Place_Name"] = Name;
                    }
                    else
                    {
                        row["Business_Place_Name"] = "";
                    }

                }

                return Utility.ConvertDataTableToList<FOBDetailModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }


    }
}