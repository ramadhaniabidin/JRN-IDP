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

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class POContractController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        public string SPList = "Non Commercials";
        private readonly bool isDev = true;

        public List<Master.Model.OptionModel> GetContractUserProcDepts()
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();
            try
            {
                dt = new DataTable();
                #region GET VENDOR
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetDepartment";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                #endregion

                Master.Model.OptionModel data = new Master.Model.OptionModel();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();
                        data.Name = row["Procurement_Department"].ToString();
                        listOption.Add(data);
                    }
                    listOption.OrderBy(o => o.Name).ToList();
                }

                return listOption;

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetMarketingCategories()
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();

            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterMarketingCategory_List";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    data = new Master.Model.OptionModel();
                    data.Code = row["ID"].ToString();
                    data.Name = row["Title"].ToString();
                    listOption.Add(data);
                }

                #endregion
                return listOption.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetVendor(string ProcurementDepartment)
        {
            ContractController cc = new ContractController();
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();
            try
            {
                string SiteUrl = SPContext.Current.Site.Url;
                string currentLogin = sp.GetCurrentUserLogin(SiteUrl);

                dt = new DataTable();
                #region GET VENDOR
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetVendor";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Procurement_Department", ProcurementDepartment);
                db.AddInParameter(db.cmd, "Title", currentLogin);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                #endregion

                Master.Model.OptionModel data = new Master.Model.OptionModel();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();
                        data.Code = row["Vendor_Code"].ToString();
                        data.Name = row["Vendor_Name"].ToString();
                        listOption.Add(data);
                    }
                    listOption.OrderBy(o => o.Name).ToList();
                }

                return listOption;

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetBranches(string VendorCode, string ProcurementDepartment)
        {
            ContractController cc = new ContractController();
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();
            try
            {
                string SiteUrl = SPContext.Current.Site.Url;
                string currentLogin = sp.GetCurrentUserLogin(SiteUrl);

                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetBranch";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", VendorCode);
                db.AddInParameter(db.cmd, "Procurement_Department", ProcurementDepartment);
                db.AddInParameter(db.cmd, "Title", currentLogin);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();
                        data.Code = row["Branch"].ToString();
                        data.Name = row["Branch"].ToString();
                        listOption.Add(data);
                    }

                }
                else
                {
                    throw new Exception("Contract data is empty!");
                }

                return listOption.OrderBy(o => o.Name).ToList();
                #endregion
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POContractDetail> GetContract(string Vendor_Code, string Branch, string Contract_No, string Remarks_Contract, string Procurement_Department)
        {
            try
            {
                List<POContractDetail> listOption = new List<POContractDetail>();

                dt = new DataTable();
                #region GET CONTRACT
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_List";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", Vendor_Code); //Parameter Contract No
                db.AddInParameter(db.cmd, "Branch", Branch); //Parameter Contract No
                db.AddInParameter(db.cmd, "Contract_No", Contract_No); //Parameter Contract No
                db.AddInParameter(db.cmd, "Remarks_Contract", Remarks_Contract); //Parameter Contract No
                db.AddInParameter(db.cmd, "Procurement_Department", Procurement_Department); //Parameter Contract No

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                POContractDetail data = new POContractDetail();

                foreach (DataRow row in dt.Rows)
                {
                    data = new POContractDetail();
                    data.ID = 0;

                    data.Contract_ID = Convert.ToInt32(row["ID"]);
                    data.Contract_No = Convert.ToString(row["Contract_No"]);
                    data.Remarks_Contract = Convert.ToString(row["Remarks"]);
                    data.Period_Start = Convert.ToDateTime(row["Period_Start"]);
                    data.Period_End = Convert.ToDateTime(row["Period_End"]);
                    data.Internal_Order_Code = Convert.ToString(row["Internal_Order_Code"]);
                    data.Internal_Order_Name = Convert.ToString(row["Internal_Order_Name"]);
                    // data.Contract_Period = row["Period_Start"].ToString() + " to " + row["Period_End"].ToString();
                    data.Materials = GetContractDetail(Convert.ToInt32(row["ID"]));
                    data.Attachments = GetContractAttachment(Convert.ToInt32(row["ID"]));

                    listOption.Add(data);
                }

                #endregion

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<MasterMappingCostCenter> GetCostCenter(string Branch)
        {
            List<MasterMappingCostCenter> listOption = new List<MasterMappingCostCenter>();

            try
            {
                dt = new DataTable();
                #region GET CONTRACT
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_MasterMappingCostCenter_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Branch", Branch); //Parameter Contract No

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                MasterMappingCostCenter data = new MasterMappingCostCenter();

                foreach (DataRow row in dt.Rows)
                {
                    data = new MasterMappingCostCenter();

                    data.ID = Convert.ToInt32(row["ID"]);
                    data.Cost_Center = Convert.ToString(row["Cost_Center"]);
                    data.Description = Convert.ToString(row["Description"]);
                    data.Business_Area = Convert.ToString(row["Business_Area"]);
                    data.Branch = Convert.ToString(row["Branch"]);
                    data.Combine = Convert.ToString(row["Combine"]);
                    data.Code = Convert.ToString(row["Cost_Center"]);
                    data.Name = Convert.ToString(row["Combine"]);

                    listOption.Add(data);
                }

                #endregion

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POContractMaterial> GetContractDetail(int Header_Id)
        {
            List<POContractMaterial> listOption = new List<POContractMaterial>();
            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractDetail_List";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Header_Id", Header_Id); //Parameter Contract No

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                #endregion

                POContractMaterial data = new POContractMaterial();

                foreach (DataRow row in dt.Rows)
                {
                    data = new POContractMaterial();

                    data.Contract_Detail_Id = Convert.ToInt32(row["ID"]);
                    data.Contract_ID = Convert.ToInt32(row["Header_ID"]);
                    data.Contract_No = Convert.ToString(row["Contract_No"]);
                    data.No = Convert.ToInt32(row["No"]);
                    data.Material_Number = Convert.ToString(row["Material_Number"]);
                    data.Material_Name = Convert.ToString(row["Material_Name"]);
                    data.Material_Description = Convert.ToString(row["Material_Description"]);
                    data.GL = Convert.ToString(row["GL"]);
                    data.GL_Description = Convert.ToString(row["GL_Description"]);
                    data.Variable_Amount = Convert.ToBoolean(row["Variable_Amount"]);
                    data.Contract_Amount = Convert.ToDecimal(row["Contract_Amount"]);

                    data.CostCenter.Code = "";
                    data.CostCenter.Name = "Please Select";
                    data.CostCenter.Selected = true;

                    listOption.Add(data);
                }

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetContractHeaderVendorByOption(string Vendor_Code)
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();
            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetVendorByOption";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Vendor_Code", Vendor_Code);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();
                        data.Code = row["Vendor_Code"].ToString();
                        data.Name = row["Vendor_Name"].ToString();
                        listOption.Add(data);
                    }

                }
                else
                {
                    throw new Exception("Vendor data not valid!");
                }

                return listOption;

                #endregion
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetContractHeaderBranchByOption(string Vendor_Code)
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();
            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_GetBranchByOption";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", Vendor_Code); //Parameter Contract No

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        data = new Master.Model.OptionModel();
                        data.Code = row["Branch"].ToString();
                        data.Name = row["Branch"].ToString();
                        listOption.Add(data);
                    }

                }
                else
                {
                    throw new Exception("Branch data not valid!");
                }

                return listOption;
                #endregion
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetContractRemarks(string Vendor_Code, string Branch, string Procurement_Department)
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();

            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractHeader_Remarks";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Vendor_Code", Vendor_Code);
                db.AddInParameter(db.cmd, "Branch", Branch);
                db.AddInParameter(db.cmd, "Procurement_Department", Procurement_Department);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();
                foreach (DataRow row in dt.Rows)
                {
                    data = new Master.Model.OptionModel();
                    data.Code = row["Contract_No"].ToString();
                    data.Name = row["Remarks"].ToString();
                    listOption.Add(data);
                }

                #endregion
                return listOption.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<Master.Model.OptionModel> GetContractRemarksExist(string Vendor_Code, string Branch, string Procurement_Department)
        {
            List<Master.Model.OptionModel> listOption = new List<Master.Model.OptionModel>();

            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "SELECT DISTINCT [Contract_No], [Remarks] FROM [ContractHeader] WHERE [Vendor_Code] = '" + Vendor_Code + "' and Branch = '" + Branch + "' AND Procurement_Department = '" + Procurement_Department + "'";
                db.cmd.CommandText = "SELECT DISTINCT [Contract_No], [Remarks] FROM [ContractHeader] WHERE [Vendor_Code] = @vendor_code AND Branch = @branch AND Procurement_Department = @proc_dept";
                db.cmd.CommandType = CommandType.Text;
                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);

                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Master.Model.OptionModel data = new Master.Model.OptionModel();
                foreach (DataRow row in dt.Rows)
                {
                    data = new Master.Model.OptionModel();
                    data.Code = row["Contract_No"].ToString();
                    data.Name = row["Remarks"].ToString();
                    listOption.Add(data);
                }

                #endregion
                return listOption.OrderBy(o => o.Name).ToList(); ;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POContractAttachment> GetContractAttachment(int Header_Id)
        {
            try
            {
                dt = new DataTable();
                #region Validasi Contract No
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_ContractAttachment_List";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Header_Id", Header_Id); //Parameter Contract No

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                #endregion

                POContractAttachment data = new POContractAttachment();


                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<POContractAttachment>(dt);
                }
                else
                {
                    return new List<POContractAttachment>();
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public POContractHeader Save(POContractHeader h, List<POContractDetail> d, string SiteUrl, string ServerPath, string Form_Status, string Notes)
        {
            try
            {
                POContractHeader listHeader = new POContractHeader();
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
                //db.AddInParameter(db.cmd, "Procurement_Department_ID", h.Procurement_Department_ID);
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

                listHeader = Utility.ConvertDataTableToList<POContractHeader>(dt)[0];
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

                    POContractDetail listDetail = new POContractDetail();
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

                        listDetail = Utility.ConvertDataTableToList<POContractDetail>(dt)[0];
                        detail.ID = listDetail.ID;
                        #endregion

                        #region COLLECT PO NUMBER
                        //string dbName = isDev ? "Daikin_Nintex_Development" : "Daikin_nintex";
                        //db.cmd.CommandText = $"UPDATE {dbName}.[dbo].[ContractHeader] SET [PO_Number] = [PO_Number]+'" + h.Form_No + ";' WHERE [ID] = " + detail.Contract_ID;
                        //db.cmd.CommandType = CommandType.Text;
                        //db.cmd.Parameters.Clear();
                        //db.cmd.ExecuteNonQuery();
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
                    //h.Item_ID = SaveSPList(SiteUrl, h, d, Form_Status); //1 Trigger WF
                    if (h.Approval_Status == "1")
                    {
                        #region Old and riskier way to trigger workflow
                        //startNACNonComWorkflow("PO Contract", "M020", h.ID, (int)h.Item_ID);
                        //StartData startData = new StartData();
                        //startData.se_itemid = (int)h.Item_ID;
                        //startData.se_listname = "PO Contract";
                        //startData.se_headerid = h.ID;
                        //startData.se_modulecode = "M020";
                        //NWCParamModel nwcParamModel = new NWCParamModel();
                        //nwcParamModel.startData = startData;
                        //NintexWorkflowCloud nwc = new NintexWorkflowCloud();
                        ////nwc.url = "https://daikin.workflowcloud.com/api/v1/workflow/published/8a6eb64d-43b7-41f8-8ef8-ee7e5c90b2a4/instances?token=AMdaBAo6P3AEmS0pJ9ZAqw8l2Ieq3OjSMhTs8g0FJfYE6Vi8ztkqTyrsWQD1VERi9ycmUz";
                        //nwc.url = "https://daikin.workflowcloud.com/api/v1/workflow/published/a8091cb6-6bd4-42e8-b8b9-be00e066574f/instances?token=GniSz54QFGNBeRa6c0suYL33oZkRFX44jF0hglrl6t5P55STREgkm1kvBG93IpY8MPxCCX";
                        //nwc.param = nwcParamModel;
                        //Task.Run(async () => { await StartNWC(nwc); }).Wait();
                        #endregion
                        #region New and better way
                        Task.Run(async () =>
                        {
                            await new NintexCloudManager().NonCommercial_StartWorkflow_V2((int)h.Item_ID, h.ID, "M020", "PO Contract");
                        }).Wait();
                        #endregion
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
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        
        public void UpdatePONumberContract(string PoNumber, int ID)
        {
            using (var con = new SqlConnection(Utility.GetSqlConnection()))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_ContractHeader_UpdatePONumber", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "PO_Number", Value = PoNumber, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "ID", Value = ID, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {

            string sBody = new JavaScriptSerializer().Serialize(nwc.param);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(nwc.url);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT Header
            //string token = Program.GetToken();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.url);

            request.Content = new StringContent(sBody, Encoding.UTF8, "application/json");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                //return result; //instance guid
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

        public int SaveSPList(string SiteUrl, POContractHeader h, List<POContractDetail> d, string Status)
        {
            ContractController cc = new ContractController();
            try
            {
                int ListItemId = Convert.ToInt32(h.Item_ID); ;
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists[SPList];
                SPContentType ct = list.ContentTypes["PO Contract"];
                SPContentTypeId ctId = ct.Id;
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
                item["Procurement_x0020_Department0"] = h.Procurement_Department;
                item["Procurement_x0020_Department_x00"] = h.Procurement_Department_ID;

                item["Module"] = "M020";
                item["MKT Category Id"] = h.Marketing_Category_ID;

                item["Grand Total"] = h.Grand_Total;
                item["ContentTypeId"] = ctId;
                item["Workflow Status"] = Status.Equals("-") ? "Draft" : "Generated";
                item["Approval Status"] = Status.Equals("-") ? "Draft" : "Generated";

                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;

                return ListItemId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int SaveSPListPOContract(string SiteUrl, POContractHeader h, List<POContractDetail> d, string Status)
        {
            ContractController cc = new ContractController();
            try
            {
                int ListItemId = Convert.ToInt32(h.Item_ID); ;
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists["PO Contract"];
                //SPContentType ct = list.ContentTypes["PO Contract"];
                //SPContentTypeId ctId = ct.Id;
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
                //item["ContentTypeId"] = ctId;
                item["Workflow Status"] = Status.Equals("-") ? "Draft" : "Generated";
                item["Approval Status"] = Status.Equals("-") ? "Draft" : "Generated";

                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;

                return ListItemId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public POContractHeader GetDataPOContractHeader(string Form_No)
        {
            POContractHeader listOption = new POContractHeader();
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_POContractHeader_GetData";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", Form_No);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                if (dt.Rows.Count > 0)
                {
                    listOption = Utility.ConvertDataTableToList<POContractHeader>(dt)[0];

                    listOption.Detail = GetDetail(listOption.ID);
                }
                else
                {
                    listOption = new POContractHeader();
                }

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POContractDetail> GetDetail(int Header_ID)
        {
            List<POContractDetail> listOption = new List<POContractDetail>();
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_POContractDetail_ListByID";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                POContractDetail data = new POContractDetail();

                foreach (DataRow row in dt.Rows)
                {
                    data = new POContractDetail();

                    data.Contract_ID = Convert.ToInt32(row["Contract_ID"]);
                    data.Contract_No = Convert.ToString(row["Contract_No"]);
                    data.Create_PO_From_Period = Convert.ToDateTime(row["Create_PO_From_Period"]);
                    data.Create_PO_To_Period = Convert.ToDateTime(row["Create_PO_To_Period"]);
                    data.Created_Date = Convert.ToDateTime(row["Created_Date"]);
                    data.Form_No = Convert.ToString(row["Form_No"]);
                    data.Header_ID = Convert.ToInt32(row["Header_ID"]);
                    data.Internal_Order_Code = Convert.ToString(row["Internal_Order_Code"]);
                    data.Internal_Order_Name = Convert.ToString(row["Internal_Order_Name"]);
                    data.ID = Convert.ToInt32(row["ID"]);
                    data.No = Convert.ToInt32(row["No"]);
                    //data.Remark = Convert.ToString(row["Remark"]);
                    data.Show = Convert.ToBoolean(0);
                    data.Period_Start = Convert.ToDateTime(row["Period_Start"]);
                    data.Period_End = Convert.ToDateTime(row["Period_End"]);
                    data.Remarks_Contract = Convert.ToString(row["Remarks_Contract"]);

                    data.Materials = GetMaterial(Convert.ToInt32(row["ID"]));
                    data.Attachments = GetContractAttachment(Convert.ToInt32(row["Contract_ID"]));

                    listOption.Add(data);
                }

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<POContractMaterial> GetMaterial(int Detail_ID)
        {
            List<POContractMaterial> listOption = new List<POContractMaterial>();
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_POContractMaterial_ListByID";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Detail_ID", Detail_ID);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                if (dt.Rows.Count > 0)
                {
                    listOption = Utility.ConvertDataTableToList<POContractMaterial>(dt);
                }
                else
                {
                    listOption = new List<POContractMaterial>();
                }

                return listOption;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //public void PO_Approval(NintexApprovalModel model)
        //{
        //    try
        //    {
        //        NintexManager ntx = new NintexManager();
        //        ntx.Approval(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public void POContractGeneratePORelease(string Form_No)
        {
            POReleaseHeader data = new POReleaseHeader();
            try
            {
                string SiteUrl = Utility.SpSiteUrl;


                var Header = GetDataPOContractHeader(Form_No);
                data.Form_No = Header.Form_No.Replace("PC", "RC");

                data.Item_ID = SaveSPListPORelease(SiteUrl, Header, data, "-"); //1 Trigger WF

                var SAPData = GetDataSAPNonCommercialPODataHeader(Form_No);
                data.Purchasing_Document = SAPData.Purchasing_Document;
                data.Company_Code = SAPData.Company_Code;
                data.Purchasing_Doc_Type = SAPData.Purchasing_Doc_Type;
                data.Release_Group = SAPData.Release_Group;
                data.Release_Strategy = SAPData.Release_Strategy;

                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_POReleaseHeader_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Form_No", data.Form_No);
                // db.AddInParameter(db.cmd, "Request_Date ", Header.Request_Date);
                // db.AddInParameter(db.cmd, "Procurement_DepartmentID", Header.Procurement_DepartmentID);
                db.AddInParameter(db.cmd, "Procurement_Department", Header.Procurement_Department);
                db.AddInParameter(db.cmd, "Branch", Header.Branch);
                db.AddInParameter(db.cmd, "Vendor_Code", Header.Vendor_Code);
                db.AddInParameter(db.cmd, "Vendor_Name", Header.Vendor_Name);
                db.AddInParameter(db.cmd, "PO_Header_ID", Header.ID);
                db.AddInParameter(db.cmd, "PO_Item_ID", Header.Item_ID);
                db.AddInParameter(db.cmd, "PO_No", Header.Form_No);
                db.AddInParameter(db.cmd, "Requester_Name", Header.Requester_Name);
                db.AddInParameter(db.cmd, "Requester_Email", Header.Requester_Email);
                db.AddInParameter(db.cmd, "Requester_Department", Header.Requester_Department);

                db.AddInParameter(db.cmd, "Purchasing_Document", data.Purchasing_Document);
                db.AddInParameter(db.cmd, "Company_Code", data.Company_Code);
                db.AddInParameter(db.cmd, "Purchasing_Doc_Type ", data.Purchasing_Doc_Type);
                db.AddInParameter(db.cmd, "Release_Group", data.Release_Group);
                db.AddInParameter(db.cmd, "Release_Strategy", data.Release_Strategy);

                db.AddInParameter(db.cmd, "PIC_Team", Header.PIC_Team);
                db.AddInParameter(db.cmd, "Created_By", Header.Created_By);
                db.AddInParameter(db.cmd, "Modified_By", Header.Modified_By);
                db.AddInParameter(db.cmd, "Item_ID", data.Item_ID);
                int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());
                data.ID = Header_ID;

                //data.Item_ID = SaveSPListPORelease(SiteUrl, Header, data, "1"); //1 Trigger WF
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SAPNonCommercialPODataHeader GetDataSAPNonCommercialPODataHeader(string Form_No)
        {
            try
            {
                dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPNonCommercialPODataHeader_getbyid";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", Form_No);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                if (dt.Rows.Count > 0)
                {
                    return Utility.ConvertDataTableToList<SAPNonCommercialPODataHeader>(dt)[0];
                }
                else
                {
                    return new SAPNonCommercialPODataHeader();
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public int SaveSPListPORelease(string SiteUrl, POContractHeader Header, POReleaseHeader data, string Status)
        {
            ContractController cc = new ContractController();
            try
            {
                int ListItemId = Convert.ToInt32(data.Item_ID);
                SPWeb web = new SPSite(SiteUrl).OpenWeb();
                SPList list = web.Lists[SPList];
                SPContentType ct = list.ContentTypes["PO Release GA IT"];
                SPContentTypeId ctId = ct.Id;
                web.AllowUnsafeUpdates = true;

                SPListItem item;
                string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version />";
                xml += "<Items>";
                xml += "<Item>";
                xml += "<m_DocType type=\"System.String\">Invoice</m_DocType>";
                xml += "<m_atc type=\"System.String\"></m_atc>";
                xml += "</Item>";
                xml += "</Items>";
                xml += "</RepeaterData> ";

                if (ListItemId == 0)
                {
                    item = list.Items.Add();
                    item["Title"] = data.Form_No;

                    item["Request Date"] = DateTime.Now.ToString("M-d-yyyy");
                    item["Requester Branch"] = Header.Branch;
                    item["Module"] = "M018";
                    item["Procurement_x0020_Department0"] = Header.Procurement_Department;
                    item["Request No"] = data.Form_No;
                    item["PO Number"] = Header.Form_No;
                    item["Current Layer"] = "0";
                    item["Details"] = GenerateXML_PORelease(Header.Detail[0].Materials);
                    item["Mandatory Attachment"] = xml;
                    item["Grand Total"] = 0;
                    item["Disc Amount"] = 0;
                    item["Purchasing Document"] = Header.Purchasing_Document;
                    item["ContentTypeId"] = ctId;
                    item["Total Tax Base Amount"] = "0";
                    item["Total VAT Amount"] = "0";
                    item["Vendor Appointed"] = Header.Vendor_Name;
                    item["DigiSign_x0020_Attachment_x0020_"] = Header.DigiSign_Attachment_Url;

                    // item["Workflow Status"] = "Generated";
                    // item["Approval Status"] = "Generated";
                    // item["MKT Category Id"] = h.Marketing_Category_ID;
                }
                else
                {
                    item = list.GetItemById(ListItemId);
                    if (data.ID > 0) item["Transaction ID"] = data.ID;
                    // item["Form Status"] = "1";
                    item["Grand Total"] = 0;
                }
                item.Update();
                ListItemId = item.ID;
                web.AllowUnsafeUpdates = false;

                return ListItemId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GenerateXML_PORelease(List<POContractMaterial> idList)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><RepeaterData><Version/><Items>";
            int seq = 1;
            foreach (POContractMaterial data in idList)
            {
                var WHT = (Convert.ToInt32(data.WHT) == 1) ? "WHT" : "Non WHT";
                int Contract_Amount = Convert.ToInt32(data.WHT) / Convert.ToInt32(data.Qty);
                xml += "<Item>";
                xml += "<No type=\"System.String\">" + seq.ToString() + "</No>";
                //xml += "<Item_Text type=\"System.String\">"+ SecurityElement.Escape(data.Material_Name) +"</Item_Text>";
                xml += "<Item_Text type=\"System.String\">" + SecurityElement.Escape(data.Text) + "</Item_Text>";
                xml += "<ddl_MaterialAnaplan type=\"System.String\">" + SecurityElement.Escape(data.Material_Number) + "</ddl_MaterialAnaplan>";
                xml += "<GL type=\"System.String\">" + SecurityElement.Escape(data.GL_Description) + "</GL>";
                xml += "<ddl_CostCenter type=\"System.String\">" + SecurityElement.Escape(data.Cost_Center) + "</ddl_CostCenter>";
                xml += "<Tax_Type type=\"System.String\">" + SecurityElement.Escape(WHT) + "</Tax_Type>";
                xml += "<Qty type=\"System.String\">" + data.Qty + "</Qty>";
                xml += "<Unit_Price type=\"System.Double\">" + Contract_Amount.ToString() + "</Unit_Price>";
                xml += "<Currency type=\"System.String\">IDR</Currency>";
                xml += "<cvAmount type=\"System.Double\">" + data.Contract_Amount + "</cvAmount>";
                xml += "<cost_center_code type=\"System.String\">" + data.Cost_Center + "</cost_center_code>";
                xml += "<Material_Anaplan_Name type=\"System.String\">" + SecurityElement.Escape(data.Material_Number) + " " + SecurityElement.Escape(data.Material_Name) + "</Material_Anaplan_Name>";
                xml += "<material_anaplan_code type=\"System.String\">" + SecurityElement.Escape(data.Material_Number) + "</material_anaplan_code>";
                xml += "<Amount type=\"System.String\">" + data.Contract_Amount + "</Amount>";
                xml += "</Item>";
                seq++;
            }

            xml += "</Items></RepeaterData>";

            return xml;
        }

        public void GeneratePOContractRelease()
        {
            //POWithContractGetRemarks
            try
            {
                DataTable dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_POReleaseHeader_ListPendingGenerate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                foreach (DataRow row in dt.Rows)
                {
                    string Form_No = Utility.GetStringValue(row, "Nintex_No");
                    Console.WriteLine(Form_No);
                    POContractGeneratePORelease(Form_No);
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}