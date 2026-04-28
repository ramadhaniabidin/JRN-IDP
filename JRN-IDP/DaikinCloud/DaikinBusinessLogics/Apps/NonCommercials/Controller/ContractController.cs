using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Apps.NonCommercials.Model;
using Daikin.BusinessLogics.Apps.NonCommercials.SharePointService;
using Daikin.BusinessLogics.Apps.NonCommercials.WorkflowHandler;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class ContractController
    {
        SqlConnection conn = new SqlConnection();
        public string SPList = "Contract";
        private readonly SharePointManager sp = new SharePointManager();
        private readonly NintexCloudManager ntx = new NintexCloudManager();
        private readonly DatabaseManager db;
        private readonly ContractRepository repo;
        private readonly ContractSharePointService service;
        private readonly ContractWorkflowHandler workflowHandler;
        private readonly string siteUrl = SPContext.Current.Site.Url;

        public ContractController()
        {
            var _db = new DatabaseManager();

            db = _db;
            repo = new ContractRepository(db, sp);
            service = new ContractSharePointService(sp);
            workflowHandler = new ContractWorkflowHandler(ntx);
        }

        public object NonCom_StartWorkflowNAC { get; private set; }

        public int GetAppointedTask(string CurrLogin, string CurrUsername, string FormNo)
        {
            try
            {
                int ID = new int();
                db.OpenConnection(ref conn);
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "CurrentUserLogin", CurrLogin);
                db.AddInParameter(db.cmd, "CurrentUserName", CurrUsername);
                db.AddInParameter(db.cmd, "Form_No", FormNo);
                db.cmd.CommandText = "usp_GetAppointedTask";

                ID = db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
                return ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> BindingMasterSPList(string ListName, string codeColumn, string displayColumn)
        {
            var dt = new DataTable();
            List<Master.Model.OptionModel> listOptions = new List<Master.Model.OptionModel>();
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


                Master.Model.OptionModel data = new Master.Model.OptionModel();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();

                        if (ListName.Equals("Master Material Anaplan"))
                        {
                            string SiteUrl = SPContext.Current.Site.Url;
                            data.Short_x0020_Name = Utility.GetStringValue(row, "Procurement_x0020_Department_x001");
                        }

                        data.Code = Utility.GetStringValue(row, codeColumn);
                        data.Name = Utility.GetStringValue(row, displayColumn);
                        data.Active = Utility.GetStringValue(row, "Active");
                        listOptions.Add(data);
                    }
                    listOptions.Sort((x, y) => x.Name.CompareTo(y.Name));
                }
                Master.Model.OptionModel listOption = new Master.Model.OptionModel();
                listOption.Code = "";
                listOption.Name = "Please Select";
                listOption.Selected = true;
                listOption.Active = "1";
                listOptions.Insert(0, listOption);

                return listOptions;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetDataHeaderFormNo(string tableName, string code)
        {

            try
            {
                string d = DateTime.Now.ToString("yyMM");
                int c = 1;
                string n = c.ToString().PadLeft(4, '0');

                db.OpenConnection(ref conn);
                string Form_No = db.Autocounter("Form_No", tableName, "Form_No", code + DateTime.Now.ToString("yyMM"), 4);
                db.CloseConnection(ref conn);

                if (string.IsNullOrEmpty(Form_No))
                {
                    return Form_No = code + d + n;
                }
                else
                {
                    return Form_No;
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<MasterUserProcDept> GetBranches()
        {
            var list = repo.GetBranchesAsync().GetAwaiter().GetResult();
            list = list.OrderBy(b => b.Name).ToList();
            list.Insert(0, new MasterUserProcDept
            {
                Code = "",
                Name = "Please Select",
                Selected = true
            });
            return list;
        }

        public List<MasterUserProcDept> GetDepartments()
        {
            List<MasterUserProcDept> listOptions = new List<MasterUserProcDept>();
            try
            {
                string SiteUrl = SPContext.Current.Site.Url;
                string currentLogin = sp.GetCurrentUserLogin(SiteUrl);

                var dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterUserProcDept_GetDepartment";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Title", currentLogin);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseConnection(ref conn);

                MasterUserProcDept data = new MasterUserProcDept();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new MasterUserProcDept();

                        data.Code = Convert.ToString(row["Procurement_Department_ID"]);
                        data.Name = Convert.ToString(row["Procurement_Department_Title"]);

                        data.Title = Convert.ToString(row["Title"]);
                        data.Procurement_Department_ID = Convert.ToInt32(row["Procurement_Department_ID"]);
                        data.Procurement_Department_Title = Convert.ToString(row["Procurement_Department_Title"]);
                        data.Procurement_Department_Code = Convert.ToString(row["Procurement_Department_Code"]);

                        listOptions.Add(data);
                    }
                    listOptions = listOptions.OrderBy(o => o.Name).ToList();
                }
                MasterUserProcDept listOption = new MasterUserProcDept();
                listOption.Code = "";
                listOption.Name = "Please Select";
                listOption.Selected = true;

                listOptions.Insert(0, listOption);

                return listOptions;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);

                throw ex;
            }
        }

        public List<MasterUserProcDept> GetMasterUserProcDept()
        {
            List<MasterUserProcDept> listOptions = new List<MasterUserProcDept>();
            try
            {
                string SiteUrl = SPContext.Current.Site.Url;
                string currentLogin = sp.GetCurrentUserLogin(SiteUrl);

                var dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterUserProcDept_GetDepartmentContract";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Title", currentLogin);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseConnection(ref conn);

                MasterUserProcDept data = new MasterUserProcDept();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new MasterUserProcDept();

                        data.Code = Convert.ToString(row["Procurement_Department_ID"]);
                        data.Name = Convert.ToString(row["Procurement_Department_Title"]);

                        data.Title = Convert.ToString(row["Title"]);
                        data.Procurement_Department_ID = Convert.ToInt32(row["Procurement_Department_ID"]);
                        data.Procurement_Department_Title = Convert.ToString(row["Procurement_Department_Title"]);
                        data.Procurement_Department_Code = Convert.ToString(row["Procurement_Department_Code"]);
                        data.ContractCount = Convert.ToInt32(row["ContractCount"]);

                        listOptions.Add(data);
                    }
                    listOptions = listOptions.OrderBy(o => o.Name).ToList();
                }
                MasterUserProcDept listOption = new MasterUserProcDept();
                listOption.Code = "";
                listOption.Name = "Please Select";
                listOption.Selected = true;

                listOptions.Insert(0, listOption);

                return listOptions;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);

                throw ex;
            }
        }

        public ContractHeader Save(ContractHeader ch, List<ContractDetail> cd, List<ContractAttachment> ca, string ServerPath, string dh, string dd, string da)
        {
            ContractHeader listOption = new ContractHeader();
            try
            {
                string SiteUrl = SPContext.Current.Site.Url;
                bool isNew = SiteUrl.Contains("3473");
                #region GET Last Form No
                if (ch.ID == 0)
                {
                    ch.Form_No = GetDataHeaderFormNo("ContractHeader", "CT");
                    ch.Item_ID = SaveSPListContract(SiteUrl, ch, cd.Count, "-"); //1 Trigger WF
                }
                #endregion

                #region GET ITEM ID FROM SAVING SP
                int Item_ID = Convert.ToInt32(ch.Item_ID);
                #endregion

                string Created_By = sp.GetCurrentUserLogin(SiteUrl);

                var dt = new DataTable();
                db.OpenConnection(ref conn);

                #region INSERT CONTRACT HEADER
                db.cmd.CommandText = "dbo.usp_ContractHeader_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                if (ch.Contract_Status_ID == 1) ch.Contract_Status_Name = "Submitted";
                db.AddInParameter(db.cmd, "Approval_Status", ch.Approval_Status);
                db.AddInParameter(db.cmd, "Branch", ch.Branch);
                db.AddInParameter(db.cmd, "Contract_No", ch.Contract_No);
                db.AddInParameter(db.cmd, "Contract_Status_ID", ch.Contract_Status_ID);
                db.AddInParameter(db.cmd, "Contract_Status_Name", ch.Contract_Status_Name);
                db.AddInParameter(db.cmd, "Contract_Type_ID", ch.Contract_Type_ID);
                db.AddInParameter(db.cmd, "Contract_Type_Name", ch.Contract_Type_Name);
                db.AddInParameter(db.cmd, "Cost_Center", ch.Cost_Center);
                db.AddInParameter(db.cmd, "Created_By", Created_By);
                db.AddInParameter(db.cmd, "PIC_Team", Created_By);                              // This is for PIC_Team
                db.AddInParameter(db.cmd, "Form_No", ch.Form_No);
                db.AddInParameter(db.cmd, "Internal_Order_Code", ch.Internal_Order_Code);
                db.AddInParameter(db.cmd, "Internal_Order_Name", ch.Internal_Order_Name);
                db.AddInParameter(db.cmd, "Document_Received", ch.Document_Received);
                db.AddInParameter(db.cmd, "Grand_Total", ch.Grand_Total);
                db.AddInParameter(db.cmd, "ID", ch.ID);
                db.AddInParameter(db.cmd, "Item_ID", ch.Item_ID);
                db.AddInParameter(db.cmd, "Modified_By", ch.Modified_By);
                db.AddInParameter(db.cmd, "PO_Number", ch.PO_Number);
                db.AddInParameter(db.cmd, "Period_End", ch.Period_End);
                db.AddInParameter(db.cmd, "Period_Start", ch.Period_Start);
                db.AddInParameter(db.cmd, "Procurement_Department", ch.Procurement_Department);
                db.AddInParameter(db.cmd, "Remarks", ch.Remarks);
                db.AddInParameter(db.cmd, "Request_Date", ch.Request_Date);
                db.AddInParameter(db.cmd, "Requester_Email", ch.Requester_Email);
                db.AddInParameter(db.cmd, "Requester_Name", ch.Requester_Name);
                db.AddInParameter(db.cmd, "Vendor_Code", ch.Vendor_Code);
                db.AddInParameter(db.cmd, "Vendor_Name", ch.Vendor_Name);
                db.AddInParameter(db.cmd, "Is_New", isNew);
                db.AddInParameter(db.cmd, "Reference_No", ch.Reference_No);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                if (dt.Rows.Count > 0)
                {
                    listOption = Utility.ConvertDataTableToList<ContractHeader>(dt)[0];
                    ch.ID = listOption.ID;
                }
                else
                {
                    listOption = new ContractHeader();
                }


                #endregion

                if (!String.IsNullOrEmpty(dd))
                {
                    db.cmd.CommandText = "dbo.usp_ContractDetail_DeleteById";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ID", dd);
                    db.cmd.ExecuteNonQuery();
                }

                if (!String.IsNullOrEmpty(da))
                {
                    db.cmd.CommandText = "dbo.usp_ContractAttachment_DeleteById";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ID", da);
                    db.cmd.ExecuteNonQuery();
                }

                if (ch.ID > 0)
                {
                    db.cmd.CommandText = "dbo.usp_Utility_CollectPICTeam";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Module_ID", "M014");
                    db.AddInParameter(db.cmd, "Header_ID", ch.ID);
                    db.cmd.ExecuteNonQuery();

                    #region INSERT CONTRACT DETAILS
                    foreach (ContractDetail contractdetail in cd)
                    {
                        db.cmd.CommandText = "dbo.[usp_ContractDetail_SaveUpdate]";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();

                        db.AddInParameter(db.cmd, "ID", contractdetail.ID);
                        db.AddInParameter(db.cmd, "No", contractdetail.No);
                        db.AddInParameter(db.cmd, "Header_ID", ch.ID);
                        db.AddInParameter(db.cmd, "Contract_Amount", contractdetail.Contract_Amount);
                        db.AddInParameter(db.cmd, "Material_Description", contractdetail.Material_Description);
                        db.AddInParameter(db.cmd, "Material_Number", contractdetail.Material_Number);

                        string[] name = contractdetail.Material_Name.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] names = name.Select(x => Convert.ToString(x)).ToArray();
                        db.AddInParameter(db.cmd, "Material_Name", names[1].Trim());
                        db.AddInParameter(db.cmd, "Form_No", ch.Form_No);
                        db.AddInParameter(db.cmd, "Contract_No", ch.Contract_No);
                        db.AddInParameter(db.cmd, "Variable_Amount", contractdetail.Variable_Amount);

                        db.cmd.ExecuteNonQuery();
                    }
                    #endregion

                    #region INSERT CONTRACT ATTACHMENTS
                    foreach (ContractAttachment contractattachment in ca)
                    {
                        db.cmd.CommandText = "dbo.[usp_ContractAttachment_SaveUpdate]";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();

                        db.AddInParameter(db.cmd, "ID", contractattachment.ID);
                        db.AddInParameter(db.cmd, "Header_ID", ch.ID);
                        db.AddInParameter(db.cmd, "Form_No", ch.Form_No);
                        db.AddInParameter(db.cmd, "Attachment_FileName", contractattachment.Attachment_FileName);
                        db.AddInParameter(db.cmd, "Attachment_Url", "/Lists/" + SPList + "/Attachments/" + ch.Item_ID.ToString() + "/" + contractattachment.Attachment_FileName);

                        db.cmd.ExecuteNonQuery();

                        var dPathFile = ServerPath + contractattachment.Attachment_FileName;
                        sp.UploadFileInCustomList("Contract", Item_ID, dPathFile, SiteUrl);
                    }
                    #endregion

                    #region Trigger WF
                    Task.Run(async () =>
                    {
                        await new NintexCloudManager().NonCommercial_StartWorkflow_V2((int)ch.Item_ID, ch.ID, "M014", "Contract");
                    }).Wait();
                    #endregion

                    #region Insert History Log First Submit
                    db.cmd.CommandText = "[dbo].[usp_NonComm_InsertApprovalLog]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();

                    db.AddInParameter(db.cmd, "ListName", "Contract");
                    db.AddInParameter(db.cmd, "ListItemID", Item_ID);
                    db.AddInParameter(db.cmd, "Action", 1);
                    db.AddInParameter(db.cmd, "CurrentLogin", Created_By);
                    db.AddInParameter(db.cmd, "CurrLoginName", sp.GetCurrentLoginFullName(SiteUrl));
                    db.AddInParameter(db.cmd, "Comment", "");
                    db.cmd.ExecuteNonQuery();
                    #endregion

                }


                db.CloseConnection(ref conn);
                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public async Task<ContractHeader> SaveAsync(ContractHeader ch, List<ContractDetail> cd, List<ContractAttachment> ca, string serverPath, string dd, string da)
        {
            // Capture user info immediately before any async context loss
            string currentUser = sp.GetCurrentUserLogin();
            string currentFullName = sp.GetCurrentLoginFullName();

            if (ch.ID == 0)
            {
                ch.Form_No = await repo.GetDataHeaderFormNo("CT").ConfigureAwait(false);
                ch.Item_ID = service.SaveSPList(siteUrl, ch, cd.Count, "-");
            }
            int itemId = Convert.ToInt32(ch.Item_ID);

            using (var _con = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _con.OpenAsync().ConfigureAwait(false);
                using (var _trans = _con.BeginTransaction())
                {
                    ch.ID = await repo.SaveHeader(_con, _trans, ch, currentUser).ConfigureAwait(false);
                    await repo.DeleteDetail(_con, _trans, dd).ConfigureAwait(false);
                    await repo.DeleteAttachment(_con, _trans, da).ConfigureAwait(false);

                    if (ch.ID > 0)
                    {
                        await repo.CollectPICTeam(_con, _trans, ch.ID).ConfigureAwait(false);

                        foreach (var detail in cd)
                        {
                            await repo.SaveDetail(_con, _trans, ch, detail).ConfigureAwait(false);
                        }

                        foreach (var attachment in ca)
                        {
                            await repo.SaveAttachment(_con, _trans, ch, attachment, itemId).ConfigureAwait(false);
                            service.UploadAttachment(itemId, siteUrl, attachment.Attachment_FileName, serverPath);
                        }
                    }

                    _trans.Commit();
                }
            }

            await workflowHandler.StartWorkflow(ch.ID, itemId).ConfigureAwait(false);
            await repo.InsertLogFirstSubmit(itemId, currentUser, currentFullName).ConfigureAwait(false);
            return ch;
        }

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {

            string sBody = new JavaScriptSerializer().Serialize(nwc.param);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(nwc.url);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.url);

            request.Content = new StringContent(sBody, Encoding.UTF8, "application/json");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
            }

        }

        private string startNACNonComWorkflow(string listName, string moduleCode, int headerID, int listItemID)
        {
            string response = "";
            try
            {
                NintexCloudManager nintexCloudManager = new NintexCloudManager();
                Task.Run(async () => { await nintexCloudManager.NonCommercial_StartWorkflow(listItemID, headerID, moduleCode, listName); }).Wait();
                var respBody = new
                {
                    Success = true,
                    Message = "OK"
                };
                response = new JavaScriptSerializer().Serialize(respBody);
            }
            catch (Exception ex)
            {
                var respBody = new
                {
                    Success = false,
                    Message = $"{ex.Message}"
                };
                response = new JavaScriptSerializer().Serialize(respBody);
            }
            return response;
        }

        public int SaveSPList(string SiteUrl, ContractHeader ch, int TotalItems, string Status)
        {
            try
            {
                int Item_ID = Convert.ToInt32(ch.Item_ID);
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists[SPList];
                SPContentType ct = list.ContentTypes["Contract"];
                SPContentTypeId ctId = ct.Id;
                web.AllowUnsafeUpdates = true;

                string Requester_Account = sp.GetCurrentUserLogin(SiteUrl);

                SPListItem item;

                if (Item_ID == 0)
                {
                    item = list.Items.Add();
                    item["Title"] = ch.Form_No;
                    item["Contract Remarks"] = ch.Remarks;
                    item["Request Date"] = ch.Request_Date;
                    item["Requester Branch"] = ch.Branch;
                    item["Requester Department"] = ch.Requester_Department;
                    item["Requester Name"] = ch.Requester_Name;
                    item["Requester Email"] = ch.Requester_Email;
                    item["Requester Account"] = Requester_Account;
                    item["Contract Type"] = ch.Contract_Type_ID;
                    item["Module"] = "M014";
                    item["Grand Total"] = ch.Grand_Total;
                    item["ContentTypeId"] = ctId;
                    item["Procurement_x0020_Department0"] = ch.Procurement_Department;
                    item["Workflow Status"] = "Generated";
                    item["Approval Status"] = "Generated";
                    item["Form Status"] = Status;
                }
                else
                {
                    item = list.GetItemById(Item_ID);
                    if (ch.ID > 0) item["Transaction ID"] = ch.ID;
                    item["Procurement_x0020_Department0"] = ch.Procurement_Department;
                    item["Grand Total"] = ch.Grand_Total;
                    item["Approval Status"] = "Generated";
                    item["Form Status"] = Status;
                }

                item.Update();
                Item_ID = item.ID;
                web.AllowUnsafeUpdates = false;

                return Item_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public int SaveSPListContract(string SiteUrl, ContractHeader ch, int TotalItems, string Status)
        {
            try
            {
                int Item_ID = Convert.ToInt32(ch.Item_ID);
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists["Contract"];
                web.AllowUnsafeUpdates = true;

                string Requester_Account = sp.GetCurrentUserLogin(SiteUrl);

                SPListItem item;

                if (Item_ID == 0)
                {

                    item = list.Items.Add();
                    item["Title"] = ch.Form_No;
                    item["Contract Remarks"] = ch.Remarks;
                    item["Request Date"] = ch.Request_Date;
                    item["Requester Branch"] = ch.Branch;
                    item["Requester Department"] = ch.Requester_Department;
                    item["Requester Name"] = ch.Requester_Name;
                    item["Requester Email"] = ch.Requester_Email;
                    item["Requester Account"] = Requester_Account;
                    item["Contract Type"] = ch.Contract_Type_ID;
                    item["Module"] = "M014";
                    item["Grand Total"] = ch.Grand_Total;
                    item["Procurement Department"] = ch.Procurement_Department;
                    item["Workflow Status"] = "Generated";
                    item["Approval Status"] = "Generated";
                    item["Form Status"] = Status;
                }
                else
                {
                    item = list.GetItemById(Item_ID);
                    if (ch.ID > 0) item["Transaction ID"] = ch.ID;
                    item["Procurement Department"] = ch.Procurement_Department;
                    item["Grand Total"] = ch.Grand_Total;
                    item["Approval Status"] = "Generated";
                    item["Form Status"] = Status;
                }

                item.Update();
                Item_ID = item.ID;
                web.AllowUnsafeUpdates = false;

                return Item_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<ContractHeader> GetDataContractHeader(string Form_No)
        {
            try
            {
                var dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", Form_No);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<ContractHeader>(dt);
                }
                else
                {
                    return new List<ContractHeader>();
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<ContractDetail> GetDataContractDetail(string Form_No)
        {
            List<ContractDetail> listOption = new List<ContractDetail>();
            try
            {
                var dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractDetail_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", Form_No);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                listOption = Utility.ConvertDataTableToList<ContractDetail>(dt);

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<ContractAttachment> GetDataContractAttachment(string Form_No)
        {
            List<ContractAttachment> listOption = new List<ContractAttachment>();
            try
            {
                var dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractAttachment_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", Form_No);

                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                listOption = Utility.ConvertDataTableToList<ContractAttachment>(dt);

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}