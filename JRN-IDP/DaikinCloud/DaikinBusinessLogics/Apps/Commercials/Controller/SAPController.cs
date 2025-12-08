using Daikin.BusinessLogics.Apps.Batch.Controller;
using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class SAPController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        private readonly SharePointManager sp = new SharePointManager();
        private readonly Utility util = new Utility();

        #region Index SAP Txt File AP
        //Company Code	Year 	Vendor	Partner Bank Account 	Business Area	Business Place	Document Number 	Posting Date 	Document Date	Due On	
        //Reference 	Assigment	Text	Amount	Currency	Amount in Local Currency	Local Currency
        public int Idx_Company_Code = 0;
        public int Idx_Year = 1;
        public int Idx_Vendor = 2;
        public int Idx_Partner_Bank_Account = 3;
        public int Idx_Business_Area = 4;
        public int Idx_Business_Place = 5;
        public int Idx_Document_Number = 6;
        public int Idx_Posting_Date = 7;
        public int Idx_Document_Date = 8;
        public int Idx_Due_On = 9;
        public int Idx_Reference = 10;
        public int Idx_Assignment = 11;
        public int Idx_Text = 12;
        public int Idx_Amount = 15;
        public int Idx_Currency = 16;
        public int Idx_Amount_Local_Curr = 13;
        public int Idx_Local_Curr = 14;
        #endregion


        #region Index Rebate
        public int R_idx_CompanyCode = 0;
        public int R_idx_FiscalYear = 1;
        public int R_idx_VendorCode = 2;
        public int R_idx_PartnerBankAccount = 3;
        public int R_idx_BusinessArea = 4;
        public int R_idx_DocumentNo = 6;
        public int R_idx_PostingDate = 7;
        public int R_idx_DocumentDate = 8;
        public int R_idx_DueOn = 9;
        public int R_idx_Reference = 10;
        public int R_idx_Assignment = 11;
        public int R_idx_ItemText = 12;
        public int R_idx_Amount = 13;
        public int R_idx_Currency = 14;
        public int R_idx_AmountInLocalCurr = 15;
        public int R_idx_LocalCurr = 16;
        public int R_idx_ReviseIndicator = 17;
        #endregion

        #region Index Feedback SAP Service Cost
        public int F_LC_IDX_NintexNo = 0;
        public int F_LC_IDX_InvoiceDocNum = 1;
        public int F_LC_IDX_FiscalYear = 2;
        public int F_LC_IDX_MIRO_No = 3;
        public int F_LC_IDX_PostingDate = 4;
        public int F_LC_IDX_DocumentDate = 5;
        public int F_LC_IDX_EntryDate = 6;
        public int F_LC_IDX_PostingTime = 7;
        #endregion


        public void SaveFeedbackSAP_ServiceCost(string[] data)
        {
            try
            {
                DataTable dtx = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_SAPFeedbackServiceCost_SaveUpdate]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data[F_LC_IDX_NintexNo]);
                db.AddInParameter(db.cmd, "Invoice_Document_No", data[F_LC_IDX_InvoiceDocNum]);
                db.AddInParameter(db.cmd, "Fiscal_Year", data[F_LC_IDX_FiscalYear]);
                db.AddInParameter(db.cmd, "Posting_Date", data[F_LC_IDX_PostingDate]);
                db.AddInParameter(db.cmd, "Document_Date", data[F_LC_IDX_DocumentDate]);
                db.AddInParameter(db.cmd, "Entry_Date", data[F_LC_IDX_EntryDate]);
                db.AddInParameter(db.cmd, "Posting_Time", data[F_LC_IDX_PostingTime]);
                db.AddInParameter(db.cmd, "MIRO_No", data[F_LC_IDX_MIRO_No]);

                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                foreach (DataRow row in dtx.Rows)
                {
                    int ItemID = Utility.GetIntValue(row, "Item_ID");
                    new ServiceCostController().NotifAccountingMIRO(ItemID, data[F_LC_IDX_MIRO_No], data[F_LC_IDX_NintexNo]);
                }
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void ReadFeedbackSAP_ServiceCost()
        {
            try
            {
                string folder = util.GetConfigValue("NetworkPath");
                folder += @"\LC\SAP Feedback";

                foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                {
                    string file_name = System.IO.Path.GetFileName(file);
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');

                            #region Read & Save
                            SaveFeedbackSAP_ServiceCost(split_data);
                            #endregion

                            Utility.SaveLog("Read Feedback SAP LC", split_data[0], file, "", 1);
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
                        Utility.SaveLog("Read Feedback SAP LC", "", file, ex.Message, 0);
                        string ErrorFilePath = folder + "\\ERROR\\" + file_name;
                        if (System.IO.File.Exists(ErrorFilePath))
                        {
                            System.IO.File.Delete(ErrorFilePath);
                        }
                        System.IO.File.Move(folder + "\\" + file_name, ErrorFilePath);
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("Read Feedback SAP LC - ReadAllLines", "", "", ex.Message, 0);
            }
        }

        public List<BatchModel> GetBatchFileContents(string moduleCode, int headerID, int No, bool isOpen = false)
        {
            dt = new DataTable();
            try
            {
                if (!isOpen)
                    db.OpenConnection(ref conn);

                db.cmd.CommandText = "usp_Utility_CreateBatchFile";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", moduleCode);
                db.AddInParameter(db.cmd, "Header_ID", headerID);
                db.AddInParameter(db.cmd, "No", No);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<BatchModel>(dt);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
            finally
            {
                if (!isOpen)
                    db.CloseConnection(ref conn);
            }
        }

        public void Read_SAP_OutstandingAP()
        {
            try
            {
                string folder = util.GetConfigValue("NetworkPath");
                folder += @"\AP";

                foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                {
                    string file_name = System.IO.Path.GetFileName(file);
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');
                            SaveSAPOutstandingAP(split_data);

                            Utility.SaveLog("SAP Outstanding AP", split_data[0], file, "", 1);
                            Console.WriteLine(line);

                        }

                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\DONE\\" + file_name);

                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog("SAP Outstanding AP", "", file, ex.Message, 0);
                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\ERROR\\" + file_name);
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("SAP Outstanding AP - ReadAllLines", "", "", ex.Message, 0);
            }
        }


        public void SaveSAPInbound(SAPInboundTXTModel data)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPInboundBL_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Inbound", data.Inbound);
                db.AddInParameter(db.cmd, "BL", data.BL);
                db.AddInParameter(db.cmd, "FOB", data.FOB);
                db.AddInParameter(db.cmd, "Document_Date", data.Document_Date);
                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }

        }


        public void Read_SAP_Inbound()
        {
            try
            {
                string folder = util.GetConfigValue("NetworkPath");
                folder += @"\Inbound";

                foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                {
                    string file_name = System.IO.Path.GetFileName(file);
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');

                            #region Read & Save
                            SAPInboundTXTModel data = new SAPInboundTXTModel();
                            Console.WriteLine(data.Inbound + " - " + data.BL);
                            data.Inbound = split_data[0];
                            data.BL = split_data[1];
                            data.FOB = split_data[2];
                            data.Document_Date = split_data[3];
                            SaveSAPInbound(data);
                            #endregion

                            Utility.SaveLog("SAP Inbound", split_data[0], file, "", 1);
                            Console.WriteLine(line);

                        }

                        string folderDone = folder + "\\DONE\\" + file_name;

                        if (System.IO.File.Exists(folderDone))
                        {
                            System.IO.File.Delete(folderDone);
                        }
                        System.IO.File.Move(folder + "\\" + file_name, folderDone);
                    
                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog("SAP Inbound", "", file, ex.Message, 0);
                        string folderError = folder + "\\ERROR\\" + file_name;
                        if (System.IO.File.Exists(folderError))
                        {
                            System.IO.File.Delete(folderError);
                        }
                        System.IO.File.Move(folder + "\\" + file_name, folderError);
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("SAP Inbound - ReadAllLines", "", "", ex.Message, 0);
            }
        }

        public void Read_SAP_Rebate()
        {
            try
            {
                string folder = util.GetConfigValue("NetworkPath");
                folder += @"\Rebate";

                foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                {
                    string file_name = System.IO.Path.GetFileName(file);
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');

                            #region Read & Save
                            Console.WriteLine(split_data[0]);
                            SaveSAPRebate(split_data);
                            #endregion

                            Utility.SaveLog("SAP Rebate AP", split_data[0], file, "", 1);
                            Console.WriteLine(line);

                        }

                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\DONE\\" + file_name);

                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog("SAP Rebate AP", "", file, ex.Message, 0);
                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\ERROR\\" + file_name);
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("SAP Rebate AP - ReadAllLines", "", "", ex.Message, 0);
            }
        }

        public void SaveSAPRebate(string[] data)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPRebateAP_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Company_Code", data[R_idx_CompanyCode]);
                db.AddInParameter(db.cmd, "Fiscal_Year", data[R_idx_FiscalYear]);
                db.AddInParameter(db.cmd, "Vendor_Code", data[R_idx_VendorCode]);
                db.AddInParameter(db.cmd, "Partner_Bank_Account", data[R_idx_PartnerBankAccount]);
                db.AddInParameter(db.cmd, "Business_Area", data[R_idx_BusinessArea]);
                db.AddInParameter(db.cmd, "Document_No", data[R_idx_DocumentNo]);
                db.AddInParameter(db.cmd, "Posting_Date", data[R_idx_PostingDate]);
                db.AddInParameter(db.cmd, "Document_Date", data[R_idx_DocumentDate]);
                db.AddInParameter(db.cmd, "Due_On", data[R_idx_DueOn]);
                db.AddInParameter(db.cmd, "Reference", data[R_idx_Reference]);
                db.AddInParameter(db.cmd, "Assignment", data[R_idx_Assignment]);
                db.AddInParameter(db.cmd, "Item_Text", data[R_idx_ItemText]);
                db.AddInParameter(db.cmd, "Amount", data[R_idx_Amount].Replace(",", ""));
                db.AddInParameter(db.cmd, "Currency", data[R_idx_Currency]);
                db.AddInParameter(db.cmd, "Amount_In_Local_Currency", data[R_idx_AmountInLocalCurr].Replace(",",""));
                db.AddInParameter(db.cmd, "Local_Currency", data[R_idx_LocalCurr]);
                db.AddInParameter(db.cmd, "Revise_Indicator", data[R_idx_ReviseIndicator]);

                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void SaveSAPOutstandingAP(string[] data)
        {
            //usp_OutstandingAP_SaveUpdate
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_OutstandingAP_SaveUpdate";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Company_Code", data[Idx_Company_Code]);
                db.AddInParameter(db.cmd, "Year", data[Idx_Year]);
                db.AddInParameter(db.cmd, "Vendor", data[Idx_Vendor]);
                db.AddInParameter(db.cmd, "Partner_Bank_Account", data[Idx_Partner_Bank_Account]);
                db.AddInParameter(db.cmd, "Business_Area", data[Idx_Business_Area]);
                db.AddInParameter(db.cmd, "Business_Place", data[Idx_Business_Place]);
                db.AddInParameter(db.cmd, "Document_No", data[Idx_Document_Number]);
                db.AddInParameter(db.cmd, "Posting_Date", data[Idx_Posting_Date]);
                db.AddInParameter(db.cmd, "Document_Date", data[Idx_Document_Date]);
                db.AddInParameter(db.cmd, "Due_On", data[Idx_Due_On]);
                db.AddInParameter(db.cmd, "Reference", data[Idx_Reference]);
                db.AddInParameter(db.cmd, "Assignment", data[Idx_Assignment]);
                db.AddInParameter(db.cmd, "Text", data[Idx_Text]);
                db.AddInParameter(db.cmd, "Amount", Convert.ToDecimal(data[Idx_Amount]));
                db.AddInParameter(db.cmd, "Currency", data[Idx_Currency]);
                db.AddInParameter(db.cmd, "Amount_In_Local_Curr", Convert.ToDecimal(data[Idx_Amount_Local_Curr]));
                db.AddInParameter(db.cmd, "Local_Currency", data[Idx_Local_Curr]);

                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

    }
}
