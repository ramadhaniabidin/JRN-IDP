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
        private readonly SharePointManager sp;
        private readonly POContractSharePointService service;
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

        public void UpdatePONumberContract(string PoNumber, int ID)
        {
            repo.UpdatePONumberContract(PoNumber, ID);
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