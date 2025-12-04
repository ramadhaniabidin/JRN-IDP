using Daikin.BusinessLogics.Apps.Batch;
using Daikin.BusinessLogics.Common;
using Daikin.JobSchedulersLogic.Controller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.SubCon.Controller
{
    public class SAPController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        Utility util = new Utility();


        #region Index SAP Feedback Release
        public const int R_IDX_NintexNo = 0;
        public const int R_IDX_PONo = 1;
        public const int R_IDX_ReleaseCode = 2;
        public const int R_IDX_Status = 3;
        #endregion


        #region Index SAP PO GR
        public int GR_IDX_PurchasingDoc = 0;
        public int GR_IDX_NintexNo = 1;
        public int GR_IDX_ItemCode = 2;
        public int GR_IDX_MovementType = 3;
        public int GR_IDX_PostingDate = 4;
        public int GR_IDX_Qty = 5;
        public int GR_IDX_Amount = 6;
        public int GR_IDX_Currency = 7;
        public int GR_IDX_AmountInLC = 8;
        public int GR_IDX_LocalCurrency = 9;
        public int GR_IDX_Reference = 10;
        public int GR_IDX_EntryDate = 11;
        public int GR_IDX_Material = 12;
        public int GR_IDX_PlantCode = 13;
        #endregion


        #region Index SAP Feedback MIRO
        public int M_IDX_NintexNo = 0;
        public int M_IDX_Status = 1;
        public int M_IDX_MIRONo = 2;
        public int M_IDX_MIROYear = 3;
        public int M_IDX_DocumentNo = 4;
        public int M_IDX_DocumentYear = 5;
        public int M_IDX_MIRODate = 6;
        public int M_IDX_DueDate = 8;
        public int M_IDX_DocumentSAPDate = 7;
        #endregion


        #region Index SAP Feedback PO Create
        public int POC_IDX_NintexNo = 0;
        public int POC_IDX_DocumentNo = 1;
        public int POC_IDX_DateProcess = 2;
        public int POC_IDX_Status = 3;
        #endregion


        #region Index SAP PO Data Header
        public int PH_IDX_PurchasingDoc = 0;
        public int PH_IDX_Nintex_No = 1;
        public int PH_IDX_CompanyCode = 2;
        public int PH_IDX_PurchasingDocType = 3;
        public int PH_IDX_CreatedDate = 4;
        public int PH_IDX_Vendor = 5;
        public int PH_IDX_DocumentDate = 6;
        public int PH_IDX_ReleaseGroup = 7;
        public int PH_IDX_ReleaseStrategy = 8;
        public int PH_IDX_TotalVal = 9;
        public int PH_IDX_Currency = 10;
        public int PH_IDX_ExchRate = 11;
        #endregion


        #region Index SAP PO Data Detail
        public int PD_IDX_PurchasingDoc = 0;
        public int PD_IDX_Item = 1;
        public int PD_IDX_Material = 2;
        public int PD_IDX_ShortText = 3;
        public int PD_IDX_Company = 4;
        public int PD_IDX_Plant = 5;
        public int PD_IDX_ReqTrackNum = 6;
        public int PD_IDX_MaterialGroup = 7;
        public int PD_IDX_OrderQty = 8;
        public int PD_IDX_OrderUnit = 9;

        public int PD_IDX_NetOrderPrice = 10;
        public int PD_IDX_PriceUnit = 11;
        public int PD_IDX_NetOrderValue = 12;
        public int PD_IDX_GrossOrderValue = 13;
        public int PD_IDX_AcctAssignmentCat = 14;
        public int PD_IDX_PurchaseReq = 15;
        public int PD_IDX_ItemOfReq = 16;
        public int PD_IDX_MaterialType = 17;
        public int PD_IDX_TaxCode = 18;
        #endregion


        #region Index SAP PO Vendor Data
        public int PVD_IDX_Vendor = 0;
        public int PVD_IDX_Title = 1;
        public int PVD_IDX_VendorName = 2;
        public int PVD_IDX_VendorPIC = 3;
        #endregion

        #region Index SAP PO Vendor Bank Data
        public int PVB_IDX_Vendor = 0;
        public int PVB_IDX_BankKey = 1;
        public int PVB_IDX_BankAccount = 2;
        public int PVB_IDX_PartnerBankId = 3;
        public int PVB_IDX_AccountHolder = 4;
        #endregion

        #region SAP PO Vendor
        public void SaveSAPVendorData(string[] data, string SAPFolderID)
        {
            try
            {
                db.OpenConnection(ref conn);
                if (SAPFolderID == "5")
                    db.cmd.CommandText = "usp_SAPNonCommercialVendorData_Save";
                else
                    db.cmd.CommandText = "[SAP].usp_SAPCommercialVendorData_Save";

                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", data[PVD_IDX_Vendor]);
                db.AddInParameter(db.cmd, "Title", data[PVD_IDX_Title]);
                db.AddInParameter(db.cmd, "Vendor_Name", data[PVD_IDX_VendorName]);
                db.AddInParameter(db.cmd, "Vendor_PIC", data[PVD_IDX_VendorPIC]);
                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public void SaveSAPVendorBankData(string[] data, string SAPFolderID)
        {
            try
            {
                db.OpenConnection(ref conn);
                if (SAPFolderID == "5")
                    db.cmd.CommandText = "usp_SAPNonCommercialVendorBankData_Save";
                else
                    db.cmd.CommandText = "[SAP].usp_SAPCommercialVendorBankData_Save";

                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Vendor_Code", data[PVB_IDX_Vendor].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Bank_Key", data[PVB_IDX_BankKey].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Bank_Account", data[PVB_IDX_BankAccount].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Partner_Bank_ID", data[PVB_IDX_PartnerBankId].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Account_Holder", data[PVB_IDX_AccountHolder].TrimStart().TrimEnd());
                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //Non Commercials & Subcon Commercials
        public void ReadSAPVendorData(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    string Vendor_Code = string.Empty;
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string file_name = System.IO.Path.GetFileName(file);
                        try
                        {
                            Console.WriteLine("Insert data SAP Vendor - In Progress");
                            string[] lines = System.IO.File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                if (Vendor_Code != split_data[0])
                                {
                                    Vendor_Code = split_data[0];
                                    //5 - Non Commercials
                                    //12 - Subcon
                                    SaveSAPVendorData(split_data, SAPFolderID);
                                    Utility.SaveLog("Read SAP Vendor Data", split_data[0], file, "", 1);
                                }
                                else
                                {
                                    SaveSAPVendorBankData(split_data, SAPFolderID);
                                    Utility.SaveLog("Read SAP Vendor Bank Data", split_data[0], file, "", 1);
                                }

                                Console.WriteLine(line);

                            }
                            string DoneFilePath = folder + "\\DONE\\" + file_name;
                            if (System.IO.File.Exists(DoneFilePath))
                            {
                                System.IO.File.Delete(DoneFilePath);
                            }
                            System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Insert SAP Vendor - {ex}");
                            Utility.SaveLog("Read SAP Vendor Data", "-", file, ex.Message, 0);
                            string ErrorFilePath = folder + "\\ERROR\\" + file_name;
                            if (System.IO.File.Exists(ErrorFilePath))
                            {
                                System.IO.File.Delete(ErrorFilePath);
                            }
                            System.IO.File.Move(folder + "\\" + file_name, ErrorFilePath);
                        }
                    }
                }
                Console.WriteLine("Insert Data SAP Vendor - Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insert SAP Vendor - {ex}");
                throw ex;
            }
        }
        #endregion
    }
}
