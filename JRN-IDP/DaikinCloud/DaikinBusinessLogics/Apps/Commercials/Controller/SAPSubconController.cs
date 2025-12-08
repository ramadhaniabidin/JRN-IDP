using Daikin.BusinessLogics.Apps.Batch;
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
    public class SAPSubconController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        Utility util = new Utility();
        private readonly BatchController batch = new BatchController();

        #region Index Commercial Subcon Header
        public int H_IDX_PurchasingDoc = 0;
        public int H_IDX_NintexNo = 1;
        public int H_IDX_CompanyCode = 2;
        public int H_IDX_PurchasingDocType = 3;
        public int H_IDX_CreatedOn = 4;
        public int H_IDX_VendorCode = 5;
        public int H_IDX_DocumentDate = 6;
        public int H_IDX_ReleaseGroup = 7;
        public int H_IDX_ReleaseStrategy = 8;
        public int H_IDX_TotalValUponRelease = 9;
        public int H_IDX_Currency = 10;
        public int H_IDX_ExchangeRate = 11;
        public int H_IDX_PurchasingGroup = 12;
        public int H_IDX_SVO_No = 13;
        #endregion

        #region  Index Commercial Subcon Detail
        public int D_IDX_PurchasingDoc = 0;
        public int D_IDX_ItemCode = 1;
        public int D_IDX_MaterialCode = 2;
        public int D_IDX_ShortText = 3;
        public int D_IDX_CompanyCode = 4;
        public int D_IDX_Plant = 5;
        public int D_IDX_ReqTrackNum = 6;
        public int D_IDX_MaterialGroup = 7;
        public int D_IDX_OrderQty = 8;
        public int D_IDX_OrderUnit = 9;
        public int D_IDX_NetOrderPrice = 10;
        public int D_IDX_PriceUnit = 11;
        public int D_IDX_NetOrderValue = 12;
        public int D_IDX_GrossOrderValue = 13;
        public int D_IDX_AcctAssignCat = 14;
        public int D_IDX_PurchaseRequisition = 15;
        public int D_IDX_ItemOfRequisition = 16;
        public int D_IDX_MaterialType = 17;
        #endregion

        #region Index Commercial Subcon GR
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

        #region Index Commercial Subcon Feedback MIRO
        public int IDX_FM_PurchasingDocument = 0;
        public int IDX_FM_Status = 1;
        public int IDX_FM_MIRO = 2;
        public int IDX_FM_FiscalYear = 3;
        public int IDX_FM_DocumentNo = 4;
        public int IDX_FM_DocumentYear = 5;
        public int IDX_FM_PostingDate = 6;
        public int IDX_FM_DueDate = 8;
        public int IDX_FM_DocumentSAPDate = 7;
        #endregion

        #region Index Feedback Release
        public int IDX_FR_NintexNo = 0;
        public int IDX_FR_PO = 1;
        public int IDX_FR_ReleaseCode = 2;
        public int IDX_FR_Status = 3;
        #endregion  

        #region Subcon
        //GR PO Subcon (MIGO)
        public void SaveCommercialSubconSAP_GR(string[] data)
        {
            try
            {
                //usp_SAPCommercialPOSubconGR_Save
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "usp_SAPCommercialPOSubconGR_Save";
                db.cmd.CommandText = "SAP.usp_SAPCommercialPOSubconGR_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Purchasing_Document", data[GR_IDX_PurchasingDoc]);
                db.AddInParameter(db.cmd, "Nintex_No", data[GR_IDX_NintexNo]);
                db.AddInParameter(db.cmd, "Item_Code", data[GR_IDX_ItemCode]);
                db.AddInParameter(db.cmd, "Movement_Type", data[GR_IDX_MovementType]);
                db.AddInParameter(db.cmd, "Posting_Date", data[GR_IDX_PostingDate]);
                db.AddInParameter(db.cmd, "Qty", data[GR_IDX_Qty]);
                db.AddInParameter(db.cmd, "Amount", data[GR_IDX_Amount].Replace(",", ""));
                db.AddInParameter(db.cmd, "Currency", data[GR_IDX_Currency]);
                db.AddInParameter(db.cmd, "Amount_In_LC", data[GR_IDX_AmountInLC].Replace(",", ""));
                db.AddInParameter(db.cmd, "Local_Currency", data[GR_IDX_LocalCurrency]);
                db.AddInParameter(db.cmd, "Reference", data[GR_IDX_Reference]);
                db.AddInParameter(db.cmd, "Entry_Date", data[GR_IDX_EntryDate]);
                db.AddInParameter(db.cmd, "Material", data[GR_IDX_Material]);
                db.AddInParameter(db.cmd, "Plant_Code", data[GR_IDX_PlantCode]);

                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);


            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        //SAP Send PO Data, Nintex Absorb then Save to DB
        public int SaveCommercialSubconSAP_Header(string[] data, int Item_ID)
        {
            try
            {
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "usp_SAPCommercialPOSubconHeader_Save";
                db.cmd.CommandText = "[SAP].usp_SAPCommercialPOSubconHeader_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Purchasing_Document", data[H_IDX_PurchasingDoc].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Nintex_No", data[H_IDX_NintexNo].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Company_Code", data[H_IDX_CompanyCode].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Purchasing_Document_Type", data[H_IDX_PurchasingDocType].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Created_On", data[H_IDX_CreatedOn].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Vendor_Code", data[H_IDX_VendorCode].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Document_Date", data[H_IDX_DocumentDate].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Release_Group", data[H_IDX_ReleaseGroup].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Release_Strategy", data[H_IDX_ReleaseStrategy].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Total_Value_Upon_Release", data[H_IDX_TotalValUponRelease].Replace(",", "").TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Currency", data[H_IDX_Currency].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Exchange_Rate", data[H_IDX_ExchangeRate].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Purchasing_Group", data[H_IDX_PurchasingGroup].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "SVO_No", data[H_IDX_SVO_No].TrimStart().TrimEnd());

                db.AddInParameter(db.cmd, "Item_ID", Item_ID);

                int Header_ID = Convert.ToInt32(db.cmd.ExecuteScalar());

                db.CloseConnection(ref conn);
                return Header_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void WriteRelease()
        {
            //sampe dsini
        }

        //PO Data Detail from SAP
        public void SaveCommercialSubconSAP_Detail(string[] data, int Header_ID, string Nintex_No)
        {
            try
            {
                db.OpenConnection(ref conn);
                //db.cmd.CommandText = "usp_SAPCommercialPOSubconDetail_Save";
                db.cmd.CommandText = "SAP.usp_SAPCommercialPOSubconDetail_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Header_ID", Header_ID);
                db.AddInParameter(db.cmd, "Nintex_No", Nintex_No);
                db.AddInParameter(db.cmd, "Item_Code", data[D_IDX_ItemCode].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Material_Code", data[D_IDX_MaterialCode].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Short_Text", data[D_IDX_ShortText].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Company_Code", data[D_IDX_CompanyCode].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Plant_Code", data[D_IDX_Plant].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Req_Tracking_No", data[D_IDX_ReqTrackNum].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Material_Group", data[D_IDX_MaterialGroup].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Order_Qty", data[D_IDX_OrderQty].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Order_Unit", data[D_IDX_OrderUnit].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Net_Order_Price", data[D_IDX_NetOrderPrice].TrimStart().TrimEnd().Replace(",", ""));
                db.AddInParameter(db.cmd, "Price_Unit", data[D_IDX_PriceUnit].TrimStart().TrimEnd().Replace(",", ""));
                db.AddInParameter(db.cmd, "Net_Order_Value", data[D_IDX_NetOrderValue].TrimStart().TrimEnd().Replace(",", ""));
                db.AddInParameter(db.cmd, "Gross_Order_Value", data[D_IDX_GrossOrderValue].TrimStart().TrimEnd().Replace(",", ""));
                db.AddInParameter(db.cmd, "Acct_Assignment_Cat", data[D_IDX_AcctAssignCat].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Purchase_Requisition", data[D_IDX_PurchaseRequisition].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Item_Of_Requisition", data[D_IDX_ItemOfRequisition].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Material_Type", data[D_IDX_MaterialType].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Purchasing_Document", data[D_IDX_PurchasingDoc].TrimStart().TrimEnd());

                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public void UpdatePOSubconHeaderAttachment(int item_id, string attachment_url)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_POSubconHeader_UpdateAttachment";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "item_id", item_id);
                db.AddInParameter(db.cmd, "attachment_url", attachment_url);
                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void SaveAttachmentPOSubcon()
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation("7");
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.pdf"))
                    {
                        string file_name = System.IO.Path.GetFileNameWithoutExtension(file);

                        Console.WriteLine(file);
                        int Item_ID = new POSubconController().GetItemId(file_name);
                        new SharePointManager().UploadFileInCustomList("Commercials", Item_ID, file, Utility.SpSiteUrl);
                        file_name = System.IO.Path.GetFileName(file);
                        UpdatePOSubconHeaderAttachment(Item_ID, "/Lists/Commercials/Attachments/" + Item_ID.ToString() + "/" + file_name);

                        string DoneFilePath = folder + "\\DONE\\" + file_name;
                        if (System.IO.File.Exists(DoneFilePath))
                        {
                            System.IO.File.Delete(DoneFilePath);
                        }
                        System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                Console.WriteLine(ex.Message);
            }
        }


        public void SaveAttachmentPOSubcon(string Purchasing_Document, int Item_ID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation("7");
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.pdf"))
                    {
                        string file_name = System.IO.Path.GetFileNameWithoutExtension(file);

                        if (file_name.ToUpper() == Purchasing_Document.ToUpper())
                        {
                            Console.WriteLine(file);
                            new SharePointManager().UploadFileInCustomList("Commercials", Item_ID, file, Utility.SpSiteUrl);
                            file_name = System.IO.Path.GetFileName(file);
                            UpdatePOSubconHeaderAttachment(Item_ID, "/Lists/Commercials/Attachments/" + Item_ID.ToString() + "/" + file_name);

                            string DoneFilePath = folder + "\\DONE\\" + file_name;
                            if (System.IO.File.Exists(DoneFilePath))
                            {
                                System.IO.File.Delete(DoneFilePath);
                            }
                            System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void ProcessEachFileSubconGR(string FolderPath)
        {
            List<string> listPurchasingDoc = new List<string>();
            foreach(string file in System.IO.Directory.EnumerateFiles(FolderPath, "*.txt"))
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(file);
                    foreach(string line in lines)
                    {
                        ProcessEachLineSubconGR(file, line);
                    }
                }
                catch
                {

                }

            }
        }

        public void ProcessEachLineSubconGR(string File, string Line)
        {
            string file_name = System.IO.Path.GetFileName(File);
            string[] split_data = Line.Split(';');
            string[] lines = System.IO.File.ReadAllLines(File);
            foreach(string line in lines)
            {
                try
                {
                    //listPurchDoc.Add(split_data[0]);
                    SaveCommercialSubconSAP_GR(split_data);
                    Utility.SaveLog("Read Commercial Subcon GR", split_data[0], File, "", 1);
                }
                catch (Exception ex)
                {
                    Utility.SaveLog("Fail Commercial Subcon GR", split_data[0], File, ex.Message, 0);
                }
            }

        }

        public void ReadCommercialSubconGR(string SAPFolderID)
        {
            var folderLocation = batch.GetFolderLocation_V2(SAPFolderID);
            string folderPath = folderLocation.PathLocation;
            ProcessFolderGRFiles(folderPath);
        }

        public void UpdateDeliveryDate(List<string> purchasingDocs, string file)
        {
            foreach (string pd in purchasingDocs)
            {
                List<POSubconDetailModel> listDetail = new POSubconController().listDetailByNintexNo(pd);
                if (listDetail.Count > 0)
                {
                    string xml_RS = new POSubconController().GenerateXML_RS(listDetail, false);

                    int ListItemId = new POSubconController().GetItemId(pd);
                    //Trigger Workflow and Continue to Finance Receiver
                    if (ListItemId > 0) new POSubconController().UpdateXML_List(xml_RS, ListItemId);
                    else Utility.SaveLog("Read Commercial Subcon GR", pd, file, "Item Id not found", 0);

                }
            }
        }

        public string ProcessGRLine(string line, string filePath)
        {
            string[] columns = line.Split(';');
            string purchasingDoc = columns[0];
            try
            {
                SaveCommercialSubconSAP_GR(columns);
                Utility.SaveLog("Read Commercial Subcon GR", purchasingDoc, filePath, "", 1);
            }
            catch(Exception ex)
            {
                Utility.SaveLog("Fail Commercial Subcon GR", purchasingDoc, filePath, ex.Message, 0);
            }
            return purchasingDoc;
        }

        public void ProcessFolderGRFiles(string folderPath)
        {
            foreach(string filePath in System.IO.Directory.EnumerateFiles(folderPath, "*.txt"))
            {
                ProcessGRFile(filePath, folderPath);
            }
        }

        public void ProcessGRFile(string filePath, string folderPath)
        {
            HashSet<string> purchasingDocs = new HashSet<string>();
            string fileName = System.IO.Path.GetFileName(filePath);
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach(var line in lines)
                {
                    string purchasingDoc = ProcessGRLine(line, filePath);
                    purchasingDocs.Add(purchasingDoc);
                }
                UpdateDeliveryDate(purchasingDocs.ToList(), filePath);
                MoveFileToFolder(folderPath, fileName, "DONE");
            }
            catch(Exception ex)
            {
                Utility.SaveLog("Read Commercial Subcon GR", "-", filePath, ex.Message, 0);
                MoveFileToFolder(folderPath, fileName, "ERROR");
            }
        }

        public void MoveFileToFolder(string folderPath, string fileName, string category)
        {
            string destPath = System.IO.Path.Combine(folderPath, category, fileName);
            if(System.IO.File.Exists(destPath))
            {
                System.IO.File.Delete(destPath);
            }
            System.IO.File.Move(System.IO.Path.Combine(folderPath, fileName), destPath);
        }

        //public void GetPrintOut(string SAPFolderID, string Nintex_No, int item_id)
        //{
        //    try
        //    {
        //        dt = new DataTable();
        //        dt = new BatchController().GetFolderLocation(SAPFolderID);
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            string moduleCode = Utility.GetStringValue(row, "Module_Code");
        //            string folder = Utility.GetStringValue(row, "Path_Location");
        //            string Purchasing_Document = string.Empty;
        //            string file_without_ext = string.Empty;
        //            foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.pdf"))
        //            {
        //                string file_name = System.IO.Path.GetFileName(file);
        //                file_without_ext = System.IO.Path.GetFileNameWithoutExtension(file);
        //                try
        //                {
        //                    if (file_without_ext.ToUpper() == Nintex_No.ToUpper())
        //                    {
        //                        byte[] byteArr = System.IO.File.ReadAllBytes(file);
        //                        new SharePointManager().UploadFileInCustomList("Commercials", item_id, byteArr, Utility.SpSiteUrl, file_name);

        //                        string DoneFilePath = folder + "\\DONE\\" + file_name;
        //                        if (System.IO.File.Exists(DoneFilePath))
        //                        {
        //                            System.IO.File.Delete(DoneFilePath);
        //                        }
        //                        System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);
        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    Utility.SaveLog("Subcon Attachment Failed", Nintex_No, file, ex.Message, 0);
        //                    string ErrorFilePath = folder + "\\ERROR\\" + file_name;
        //                    if (System.IO.File.Exists(ErrorFilePath))
        //                    {
        //                        System.IO.File.Delete(ErrorFilePath);
        //                    }
        //                    System.IO.File.Move(folder + "\\" + file_name, ErrorFilePath);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.SaveLog("Read Commercial Subcon - ReadAllLines", "", "", ex.Message, 0);
        //    }
        //}
        public void ReadCommercialSubcon(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    string Purchasing_Document = string.Empty;
                    string Nintex_No = "";
                    int Header_ID = 0;
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string file_name = System.IO.Path.GetFileName(file);
                        try
                        {
                            string[] lines = System.IO.File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                if (Nintex_No != split_data[0])
                                {
                                    Nintex_No = split_data[0];
                                    //Save Header
                                    Header_ID = SaveCommercialSubconSAP_Header(split_data, 0);
                                }
                                else
                                {
                                    //Save Detail
                                    SaveCommercialSubconSAP_Detail(split_data, Header_ID, Nintex_No);
                                }

                                Utility.SaveLog("Read Commercial Subcon", split_data[0], file, "", 1);
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
                            Utility.SaveLog("Read Commercial Subcon", Nintex_No, file, ex.Message, 0);
                            string ErrorFilePath = folder + "\\ERROR\\" + file_name;
                            if (System.IO.File.Exists(ErrorFilePath))
                            {
                                System.IO.File.Delete(ErrorFilePath);
                            }
                            System.IO.File.Move(folder + "\\" + file_name, ErrorFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("Read Commercial Subcon - ReadAllLines", "", "", ex.Message, 0);
            }
        }

        public int SaveFeedbackMIRO(string[] data)
        {
            int Item_ID = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPFeedbackPOSubcon_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data[IDX_FM_PurchasingDocument].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Status", data[IDX_FM_Status].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "MIRO_No", data[IDX_FM_MIRO].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "MIRO_Year", data[IDX_FM_FiscalYear].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Document_No", data[IDX_FM_DocumentNo].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Document_Year", data[IDX_FM_DocumentYear].TrimStart().TrimEnd());

                //25 oct 2023 -- additional due date information
                db.AddInParameter(db.cmd, "Due_Date", Utility.GetValueFromArray(data, IDX_FM_DueDate).TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Document_SAP_Date", Utility.GetValueFromArray(data, IDX_FM_DocumentSAPDate).TrimStart().TrimEnd());

                db.AddOutParameter(db.cmd, "@Item_ID", SqlDbType.Int);

                db.cmd.ExecuteNonQuery();

                Item_ID = Convert.ToInt32(db.cmd.Parameters["@Item_ID"].Value);

                db.CloseConnection(ref conn);
                return Item_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public int SaveFeedbackRelease(string[] data)
        {
            int Item_ID = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_SAPFeedbackPOSubconRelease_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data[IDX_FR_NintexNo].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Status", data[IDX_FR_Status].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "PO_No", data[IDX_FR_PO].TrimStart().TrimEnd());
                db.AddInParameter(db.cmd, "Release_Code", data[IDX_FR_ReleaseCode].TrimStart().TrimEnd());

                db.AddOutParameter(db.cmd, "@Item_ID", SqlDbType.Int);

                db.cmd.ExecuteNonQuery();

                Item_ID = Convert.ToInt32(db.cmd.Parameters["@Item_ID"].Value);

                db.CloseConnection(ref conn);
                return Item_ID;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void ReadFeedbackRelease(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);

                foreach (DataRow row in dt.Rows)
                {
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string[] lines = System.IO.File.ReadAllLines(file);
                        string file_name = System.IO.Path.GetFileName(file);
                        Console.WriteLine(file_name);

                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');
                            int item_id = SaveFeedbackRelease(split_data);
                            if (split_data[IDX_FR_Status].ToUpper().Contains("SUCCESS"))
                            {
                                if (item_id > 0) new POSubconController().ResumeApproval(item_id, "1", "2");
                                string DoneFilePath = folder + "\\DONE\\" + file_name;
                                if (System.IO.File.Exists(DoneFilePath))
                                {
                                    System.IO.File.Delete(DoneFilePath);
                                }
                                System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);

                            }
                            else
                            {
                                string DoneFilePath = folder + "\\ERROR\\" + file_name;
                                if (System.IO.File.Exists(DoneFilePath))
                                {
                                    System.IO.File.Delete(DoneFilePath);
                                }
                                System.IO.File.Move(folder + "\\" + file_name, DoneFilePath);

                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //After Accounting Manager approved, Nintex kirim File Batch MIRO ke SAP lalu SAP akan kirimkan feedbac
        //Kemudian Trigger Workflow kembali untuk melanjutkan approval berikutnya
        public void ReadFeedbackMIRO(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    string Purchasing_Document = string.Empty;
                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string file_name = System.IO.Path.GetFileName(file);
                        try
                        {
                            string[] lines = System.IO.File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                int item_id = SaveFeedbackMIRO(split_data);
                                if (split_data[IDX_FM_Status].ToUpper().Contains("SUCCESS"))
                                {
                                    if (item_id > 0) new POSubconController().ResumeApproval(item_id, "1", "4");
                                }
                                Utility.SaveLog("Read Feedback MIRO Subcon", split_data[0], file, "", 1);
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
                            Utility.SaveLog("Read Feedback MIRO Subcon", "", file, ex.Message, 0);
                            string ErrorFilePath = folder + "\\ERROR\\" + file_name;
                            if (System.IO.File.Exists(ErrorFilePath))
                            {
                                System.IO.File.Delete(ErrorFilePath);
                            }
                            System.IO.File.Move(folder + "\\" + file_name, ErrorFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("Read Feedback MIRO Subcon", "", "", ex.Message, 0);
            }
        }

        #endregion

    }
}