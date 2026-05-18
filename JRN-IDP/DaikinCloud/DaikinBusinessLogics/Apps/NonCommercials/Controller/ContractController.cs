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
        private readonly string SPList = "Contract";
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
            repo = new ContractRepository(db);
            service = new ContractSharePointService(sp);
            workflowHandler = new ContractWorkflowHandler(ntx);
        }

        public object NonCom_StartWorkflowNAC { get; private set; }

        public int GetAppointedTask(string CurrLogin, string CurrUsername, string FormNo)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "CurrentUserLogin", CurrLogin);
                db.AddInParameter(db.cmd, "CurrentUserName", CurrUsername);
                db.AddInParameter(db.cmd, "Form_No", FormNo);
                db.cmd.CommandText = "usp_GetAppointedTask";

                int ID = db.cmd.ExecuteNonQuery();
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
            SPWeb web = SPContext.Current.Web;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                SPList list = web.Lists[ListName];
                SPListItemCollection items = list.Items;
                dt = new DataTable();
                dt = items.GetDataTable();
            });

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Master.Model.OptionModel data = new Master.Model.OptionModel();
                    if (ListName.Equals("Master Material Anaplan"))
                    {
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
                    return code + d + n;
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


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        MasterUserProcDept data = new MasterUserProcDept();

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
            finally
            {
                db.CloseConnection(ref conn);
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


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        MasterUserProcDept data = new MasterUserProcDept();

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
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public async Task<ContractHeader> SaveAsync(ContractHeader ch, List<ContractDetail> cd, List<ContractAttachment> ca, string serverPath, string dd, string da)
        {
            // Capture user info immediately before any async context loss
            string currentUser = sp.GetCurrentUserLogin(siteUrl);
            string currentFullName = sp.GetCurrentLoginFullName(siteUrl);

            if (ch.ID == 0)
            {
                ch.Form_No = await repo.GetDataHeaderFormNo("CT");
                ch.Item_ID = service.SaveSPList(siteUrl, ch, cd.Count, "-");
            }
            int itemId = Convert.ToInt32(ch.Item_ID);

            using (var _con = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _con.OpenAsync();
                using (var _trans = _con.BeginTransaction())
                {
                    ch.ID = await repo.SaveHeader(_con, _trans, ch, currentUser);
                    await repo.DeleteDetail(_con, _trans, dd);
                    await repo.DeleteAttachment(_con, _trans, da);

                    if (ch.ID > 0)
                    {
                        await repo.CollectPICTeam(_con, _trans, ch.ID);

                        foreach (var detail in cd)
                        {
                            await repo.SaveDetail(_con, _trans, ch, detail);
                        }

                        foreach (var attachment in ca)
                        {
                            await repo.SaveAttachment(_con, _trans, ch, attachment, itemId);
                            service.UploadAttachment(itemId, siteUrl, attachment.Attachment_FileName, serverPath);
                        }
                    }

                    _trans.Commit();
                }
            }

            await workflowHandler.StartWorkflow(ch.ID, itemId);
            await repo.InsertLogFirstSubmit(itemId, currentUser, currentFullName);
            return ch;
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
                SPContentType ct = list.ContentTypes[SPList];
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
                SPList list = web.Lists[SPList];
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

        public async Task<List<ContractHeader>> GetDataContractHeaderAsync(string Form_No)
        {
            return await repo.GetDataContractHeaderAsync(Form_No);
        }

        public List<ContractDetail> GetDataContractDetail(string Form_No)
        {
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

                List<ContractDetail> listOption = Utility.ConvertDataTableToList<ContractDetail>(dt);

                return listOption;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public async Task<List<ContractDetail>> GetDataContractDetailAsync(string Form_No)
        {
            return await repo.GetDataContractDetailAsync(Form_No);
        }

        public List<ContractAttachment> GetDataContractAttachment(string Form_No)
        {
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

                List<ContractAttachment> listOption = Utility.ConvertDataTableToList<ContractAttachment>(dt);

                return listOption;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public async Task<List<ContractAttachment>> GetDataContractAttachmentAsync(string Form_No)
        {
            return await repo.GetDataContractAttachmentAsync(Form_No);
        }
    }
}