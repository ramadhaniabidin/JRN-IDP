using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Apps.NonCommercials.Model;
using Daikin.BusinessLogics.Common;
using System.Data.SqlClient;
using System.Data;
using Microsoft.SharePoint;
using System.Collections;
using System.Security;
using System.Web.Script.Serialization;
using Daikin.BusinessLogics.Common.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using Daikin.BusinessLogics.Apps.NonCommercials.Repository;
using Daikin.BusinessLogics.Apps.NonCommercials.SharePointService;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class POContractController
    {
        private readonly DatabaseManager db;
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        private readonly SharePointManager sp;
        private readonly POContractSharePointService service;
        public string SPList = "Non Commercials";
        private readonly string moduleCode = "M020";
        private readonly POContractRepository repo;
        private readonly NintexCloudManager nintexManager;

        public POContractController()
        {
            db = new DatabaseManager();
            repo = new POContractRepository(db);
            sp = new SharePointManager();
            service = new POContractSharePointService(sp);
            nintexManager = new NintexCloudManager();
        }

        public List<Master.Model.OptionModel> GetContractUserProcDepts()
        {
            var options = repo.GetContractUserProcDepts();
            options = options.OrderBy(o => o.Name).ToList();
            return options;
        }

        public List<Master.Model.OptionModel> GetMarketingCategories()
        {
            var options = repo.GetMarketingCategories();
            options = options.OrderBy(o => o.Name).ToList();
            return options;
        }

        public List<Master.Model.OptionModel> GetVendor(string ProcurementDepartment)
        {
            string currentLogin = sp.GetCurrentUserLogin(SPContext.Current.Site.Url);
            var options = repo.GetVendor(ProcurementDepartment, currentLogin);
            options = options.OrderBy(o => o.Name).ToList();
            options.Insert(0, new Master.Model.OptionModel
            {
                Code = string.Empty,
                Name = "Please Select",
                Selected = true
            });
            return options;
        }

        public List<Master.Model.OptionModel> GetBranches(string VendorCode, string ProcurementDepartment)
        {
            string currentLogin = sp.GetCurrentUserLogin(SPContext.Current.Site.Url);
            var list = repo.GetBranches(VendorCode, ProcurementDepartment, currentLogin);
            list.Insert(0, new Master.Model.OptionModel
            {
                Code = string.Empty,
                Name = "Please Select",
                Selected = true
            });
            return list;
        }

        public List<POContractDetail> GetContract(string Vendor_Code, string Branch, string Contract_No, string Remarks_Contract, string Procurement_Department)
        {
            var contract = repo.GetContract(Vendor_Code, Branch, Contract_No, Remarks_Contract, Procurement_Department);
            contract.Materials = GetContractDetail((int)contract.Contract_ID);
            contract.Attachments = GetContractAttachment((int)contract.Contract_ID);
            return new List<POContractDetail>
            {
                contract
            };
        }

        public List<MasterMappingCostCenter> GetCostCenter(string Branch)
        {
            return repo.GetCostCenter(Branch);
        }

        public List<POContractMaterial> GetContractDetail(int Header_Id)
        {
            return repo.GetContractDetail(Header_Id);
        }

        public List<Master.Model.OptionModel> GetContractHeaderVendorByOption(string Vendor_Code)
        {
            return repo.GetContractHeaderVendorByOption(Vendor_Code);
        }

        public List<Master.Model.OptionModel> GetContractHeaderBranchByOption(string Vendor_Code)
        {
            return repo.GetContractHeaderBranchByOption(Vendor_Code);
        }

        public List<Master.Model.OptionModel> GetContractRemarks(string Vendor_Code, string Branch, string Procurement_Department)
        {
            var remarks = repo.GetContractRemarks(Vendor_Code, Branch, Procurement_Department);
            remarks = remarks.OrderBy(r => r.Name).ToList();
            return remarks;
        }

        public List<Master.Model.OptionModel> GetContractRemarksExist(string Vendor_Code, string Branch, string Procurement_Department)
        {
            var remarks = repo.GetContractRemarksExist(Vendor_Code, Branch, Procurement_Department);
            return remarks.OrderBy(r => r.Name).ToList();
        }

        public List<POContractAttachment> GetContractAttachment(int Header_Id)
        {
            return repo.GetContractAttachment(Header_Id);
        }

        private POContractHeader PreparePOContractSaveHeader(POContractHeader Header, string SiteUrl, string FormStatus)
        {
            Header.Requester_Name = sp.GetCurrentLoginFullName(SiteUrl);
            Header.Requester_Email = sp.GetCurrentLoginEmail(SiteUrl);
            Header.Created_By = sp.GetCurrentUserLogin(SiteUrl, true);
            Header.Approval_Status = FormStatus == "-" ? "8" : "1";
            if (Header.ID == 0)
            {
                Header.Form_No = repo.GenerateFormNumber();
                Header.Item_ID = service.SaveSharePointList(SiteUrl, Header, "-");
            }
            return Header;
        }

        public POContractHeader Save1(POContractHeader h, List<POContractDetail> d, string SiteUrl, string Form_Status, string Notes)
        {
            h = PreparePOContractSaveHeader(h, SiteUrl, Form_Status);
            using (var _conn = new SqlConnection(db.GetSQLConnectionString()))
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    try
                    {
                        var insertedHeader = repo.SaveHeader(h, _conn, trans);
                        h.ID = insertedHeader.ID;
                        h.Created_Date = insertedHeader.Created_Date;

                        if (h.ID > 0)
                        {
                            repo.DeleteExistingDetail(h.ID, h.Form_No, _conn, trans);
                            foreach (var detail in d)
                            {
                                var insertedDetail = repo.SaveDetail(detail, h, _conn, trans);
                                repo.UpdatePONumberContract(h.Form_No, (int)insertedDetail.Contract_ID, _conn, trans);
                                foreach (var material in detail.Materials)
                                {
                                    repo.SaveMaterial(material, insertedDetail, h, _conn, trans);
                                }
                            }
                        }



                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }


            if (Form_Status == "1" || Form_Status == "19")
            {
                repo.InsertHistoryLogAfterSave(h, Notes, Form_Status);
            }

            if (h.Approval_Status == "1")
            {
                var payload = nintexManager.GenerateNACPayload(h.ID, (int)h.Item_ID, moduleCode, "PO Contract");
                nintexManager.TriggerWorkflow(payload);
            }

            repo.CollectPICTeam(h.ID);

            return h;
        }

        public POContractHeader Save(POContractHeader h, List<POContractDetail> d, string SiteUrl, string ServerPath, string Form_Status, string Notes)
        {
            try
            {
                h.Requester_Name = SPContext.Current.Web.CurrentUser.Name;
                h.Requester_Email = SPContext.Current.Web.CurrentUser.Email;
                bool isNew = SiteUrl.Contains("3473");
                ContractController cc = new ContractController();

                #region VALIDATION SAVE AS DRAFT
                if (Form_Status.Equals("-"))
                {
                    h.Approval_Status = "8"; //SAVE AS DRAFT
                }
                else
                {
                    h.Approval_Status = "1";
                }
                #endregion

                #region VALIDATION IF NEW DATA
                if (h.ID == 0)
                {
                    #region GET Form_No
                    h.Form_No = cc.GetDataHeaderFormNo("POContractHeader", "PC");
                    #endregion

                    #region GET ITEM ID SP LIST
                    h.Item_ID = SaveSPListPOContract(SiteUrl, h, d, "-"); //SAVING WF
                    #endregion
                }
                #endregion

                db.OpenConnection(ref conn);
                #region SAVING HEADER
                dt = new DataTable();
                db.cmd.CommandText = "dbo.usp_POContractHeader_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "ID", h.ID);
                db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                db.AddInParameter(db.cmd, "Item_ID", h.Item_ID);
                db.AddInParameter(db.cmd, "Requester_Name", h.Requester_Name);
                db.AddInParameter(db.cmd, "Requester_Email", h.Requester_Email);
                db.AddInParameter(db.cmd, "Requester_Department", h.Procurement_Department);
                db.AddInParameter(db.cmd, "Procurement_Department", h.Procurement_Department);
                db.AddInParameter(db.cmd, "Marketing_Category_ID", h.Marketing_Category_ID);
                db.AddInParameter(db.cmd, "Marketing_Category_Name", h.Marketing_Category_Name);
                db.AddInParameter(db.cmd, "Vendor_Code", h.Vendor_Code);
                db.AddInParameter(db.cmd, "Vendor_Name", h.Vendor_Name);
                db.AddInParameter(db.cmd, "Branch", h.Branch);
                db.AddInParameter(db.cmd, "Cost_Center", h.Cost_Center);
                db.AddInParameter(db.cmd, "Grand_Total", h.Grand_Total);
                db.AddInParameter(db.cmd, "Document_Received", h.Document_Received);
                db.AddInParameter(db.cmd, "Approval_Status", h.Approval_Status);
                db.AddInParameter(db.cmd, "Created_By", h.Created_By);
                db.AddInParameter(db.cmd, "Modified_By", h.Modified_By);
                db.AddInParameter(db.cmd, "PIC_Team", h.Created_By);                    // for PIC_Team
                db.AddInParameter(db.cmd, "Is_New", isNew);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                var listHeader = Utility.ConvertDataTableToList<POContractHeader>(dt)[0];
                h.ID = listHeader.ID;
                h.Created_Date = listHeader.Created_Date;
                #endregion

                if (h.ID > 0)
                {
                    #region DELETE IF MATERIAL AND DETAIL EXIST
                    db.cmd.CommandText = "[dbo].[usp_POContractDetailMaterial_Delete]";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Header_ID", h.ID);
                    db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                    db.cmd.ExecuteNonQuery();
                    #endregion

                    foreach (POContractDetail detail in d)
                    {
                        #region SAVING DETAIL
                        dt = new DataTable();
                        db.cmd.CommandText = "dbo.[usp_POContractDetail_SaveUpdate]";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();
                        db.AddInParameter(db.cmd, "Header_ID", h.ID);
                        db.AddInParameter(db.cmd, "ID", detail.ID);
                        db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                        db.AddInParameter(db.cmd, "No", detail.No);
                        db.AddInParameter(db.cmd, "Internal_Order_Code", detail.Internal_Order_Code);
                        db.AddInParameter(db.cmd, "Internal_Order_Name", detail.Internal_Order_Name);
                        db.AddInParameter(db.cmd, "Contract_ID", detail.Contract_ID);
                        db.AddInParameter(db.cmd, "Contract_No", detail.Contract_No);
                        db.AddInParameter(db.cmd, "Remarks_Contract", detail.Remarks_Contract);
                        db.AddInParameter(db.cmd, "Create_PO_From_Period", detail.Create_PO_From_Period);
                        db.AddInParameter(db.cmd, "Create_PO_To_Period", detail.Create_PO_To_Period);
                        reader = db.cmd.ExecuteReader();
                        dt.Load(reader);
                        db.CloseDataReader(reader);

                        var listDetail = Utility.ConvertDataTableToList<POContractDetail>(dt)[0];
                        detail.ID = listDetail.ID;
                        #endregion

                        #region COLLECT PO NUMBER
                        UpdatePONumberContract(h.Form_No, (int)detail.Contract_ID);
                        #endregion

                        foreach (POContractMaterial m in detail.Materials)
                        {
                            #region SAVING MATERIAL
                            db.cmd.CommandText = "dbo.[usp_POContractMaterial_SaveUpdate]";
                            db.cmd.CommandType = CommandType.StoredProcedure;
                            db.cmd.Parameters.Clear();

                            db.AddInParameter(db.cmd, "ID", m.ID);
                            db.AddInParameter(db.cmd, "Header_ID", h.ID);
                            db.AddInParameter(db.cmd, "Detail_ID", detail.ID);
                            db.AddInParameter(db.cmd, "Form_No", h.Form_No);
                            db.AddInParameter(db.cmd, "Contract_Detail_Id", m.Contract_Detail_Id);
                            db.AddInParameter(db.cmd, "Contract_ID", m.Contract_ID);
                            db.AddInParameter(db.cmd, "Contract_No", m.Contract_No);
                            db.AddInParameter(db.cmd, "Remarks_Contract", detail.Remarks_Contract);
                            db.AddInParameter(db.cmd, "No", m.No);
                            db.AddInParameter(db.cmd, "Material_Number", m.Material_Number);
                            db.AddInParameter(db.cmd, "Material_Name", m.Material_Name);
                            db.AddInParameter(db.cmd, "Material_Description", m.Material_Description);
                            db.AddInParameter(db.cmd, "GL", m.GL);
                            db.AddInParameter(db.cmd, "GL_Description", m.GL_Description);
                            db.AddInParameter(db.cmd, "Qty", m.Qty);
                            db.AddInParameter(db.cmd, "Text", m.Text);
                            db.AddInParameter(db.cmd, "Cost_Center", m.Cost_Center);
                            db.AddInParameter(db.cmd, "WHT", m.WHT);
                            db.AddInParameter(db.cmd, "Variable_Amount", m.Variable_Amount);
                            db.AddInParameter(db.cmd, "Contract_Amount", m.Contract_Amount);

                            db.cmd.ExecuteNonQuery();
                            #endregion
                        }
                    }
                    db.CloseConnection(ref conn);
                    #region Trigger WF
                    if (h.Approval_Status == "1")
                    {
                        nintexManager.NonCommercial_StartWorkflow((int)h.Item_ID, h.ID, "M020", "PO Contract");
                    }


                    #region Insert History Log First Submit
                    if (Form_Status == "1")
                    {
                        db.OpenConnection(ref conn);
                        db.cmd.CommandText = "[dbo].[usp_NonComm_InsertApprovalLog]";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();

                        db.AddInParameter(db.cmd, "ListName", "PO Contract");
                        db.AddInParameter(db.cmd, "ListItemID", h.Item_ID);
                        db.AddInParameter(db.cmd, "Action", 1);
                        db.AddInParameter(db.cmd, "CurrentLogin", h.Created_By);
                        db.AddInParameter(db.cmd, "CurrLoginName", sp.GetCurrentLoginFullName(SiteUrl));
                        db.AddInParameter(db.cmd, "Comment", Notes);
                        db.cmd.ExecuteNonQuery();
                        db.CloseConnection(ref conn);
                    }
                    #endregion

                    #region Insert History Log Submit Revise
                    else if (Form_Status == "19")
                    {
                        db.OpenConnection(ref conn);
                        db.cmd.CommandText = "[dbo].[usp_NonComm_InsertApprovalLog]";
                        db.cmd.CommandType = CommandType.StoredProcedure;
                        db.cmd.Parameters.Clear();

                        db.AddInParameter(db.cmd, "ListName", "PO Contract");
                        db.AddInParameter(db.cmd, "ListItemID", h.Item_ID);
                        db.AddInParameter(db.cmd, "Action", 19);
                        db.AddInParameter(db.cmd, "CurrentLogin", h.Created_By);
                        db.AddInParameter(db.cmd, "CurrLoginName", sp.GetCurrentLoginFullName(SiteUrl));
                        db.AddInParameter(db.cmd, "Comment", Notes);
                        db.cmd.ExecuteNonQuery();
                        db.CloseConnection(ref conn);
                    }
                    #endregion

                    #endregion

                    #region COLLECT PIC TEAM
                    db.OpenConnection(ref conn);
                    db.cmd.CommandText = "dbo.usp_Utility_CollectPICTeamUpdateHeaderTable";
                    db.cmd.CommandType = CommandType.StoredProcedure;
                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "Table_Name", "POContractHeader");
                    db.AddInParameter(db.cmd, "Header_ID", h.ID);
                    db.cmd.ExecuteNonQuery();
                    db.CloseConnection(ref conn);
                    #endregion
                }

                return listHeader;

            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public void UpdatePONumberContract(string PoNumber, int ID)
        {
            repo.UpdatePONumberContract(PoNumber, ID);
        }

        public int SaveSPListPOContract(string SiteUrl, POContractHeader h, List<POContractDetail> d, string Status)
        {
            ContractController cc = new ContractController();
            int ListItemId = Convert.ToInt32(h.Item_ID); ;
            SPWeb web = new SPSite(SiteUrl).OpenWeb();
            SPList list = web.Lists["PO Contract"];
            web.AllowUnsafeUpdates = true;

            string Created_By = sp.GetCurrentUserLogin(SiteUrl);
            h.Created_By = Created_By;

            SPListItem item;

            if (ListItemId == 0)
            {
                item = list.Items.Add();
                item["Form Status"] = Status;
            }
            else
            {
                item = list.GetItemById(ListItemId);
                if (h.ID > 0)
                {
                    item["Transaction ID"] = h.ID;
                    item["Form Status"] = Status;
                }
            }

            item["Title"] = h.Form_No;

            item["Request Date"] = h.Created_Date;
            item["Requester Branch"] = h.Branch;
            item["Requester Department"] = h.Requester_Department;
            item["Requester Name"] = h.Requester_Name;
            item["Requester Email"] = h.Requester_Email;
            item["Requester Account"] = h.Created_By;
            item["Procurement Department"] = h.Procurement_Department;
            item["Procurement Department ID"] = h.Procurement_Department_ID;

            item["Module"] = "M020";
            item["MKT Category Id"] = h.Marketing_Category_ID;

            item["Grand Total"] = h.Grand_Total;
            item["Workflow Status"] = Status.Equals("-") ? "Draft" : "Generated";
            item["Approval Status"] = Status.Equals("-") ? "Draft" : "Generated";

            item.Update();
            ListItemId = item.ID;
            web.AllowUnsafeUpdates = false;

            return ListItemId;
        }

        public POContractHeader GetDataPOContractHeader(string Form_No)
        {
            var data = repo.GetPOContractHeader(Form_No);
            return data;
        }

        public List<POContractDetail> GetDetail(int Header_ID)
        {
            var details = repo.GetPOContractDetail(Header_ID);
            return details;
        }

        public List<POContractMaterial> GetMaterial(int Detail_ID)
        {
            var materials = repo.GetPOContractMaterial(Detail_ID);
            return materials;
        }

    }
}