using Daikin.BusinessLogics.Apps.Batch.Controller;
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
    public class SAPPIBController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        Utility util = new Utility();

        #region  Index SAP Tax 01
        public int T_Document_No = 0;
        public int T_Document_Date = 1;
        public int T_Document_Time = 2;
        public int T_Nintex_No = 3;
        public int T_Ref_No = 4;

        #endregion

        #region Index SAP BeaMasuk 02
        public int BM_Nintex_No = 0;
        public int BM_MIRO_No = 1;
        public int BM_MIRO_Period = 2;
        public int BM_Document_No = 3;
        public int BM_Document_Period = 4;
        public int BM_Date1 = 5;
        public int BM_Date2 = 6;
        public int BM_Date3 = 7;
        public int BM_Ref_No = 8;
        #endregion
        public void SaveFeedback_Tax(string[] data)
        {
            var siteURL = util.GetConfigValue("SiteUrl");
            var listName = "Workflow Trans Approval";
            var workflowName = "Workflow Trans Approval";

            try
            {
                DataTable dtx = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_SAPFeedbackTax_SaveUpdate]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data[T_Nintex_No]);
                db.AddInParameter(db.cmd, "Document_No", data[T_Document_No]);
                db.AddInParameter(db.cmd, "Document_Date", data[T_Document_Date]);
                db.AddInParameter(db.cmd, "Document_Time", data[T_Document_Time]);
                db.AddInParameter(db.cmd, "Ref_No", data[T_Ref_No]);

                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                foreach (DataRow row in dtx.Rows)
                {
                    int ItemID = Utility.GetIntValue(row, "Trans_Approval_Item_ID");
                    sp.StartWorkflowBySystemAccount(siteURL, ItemID, workflowName, listName);
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void SaveFeedback_BM(string[] data)
        {
            try
            {
                DataTable dtx = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_SAPFeedbackPIBBeaMasuk_SaveUpdate]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data[BM_Nintex_No]);
                db.AddInParameter(db.cmd, "MIRO_No", data[BM_MIRO_No]);
                db.AddInParameter(db.cmd, "MIRO_Period", data[BM_MIRO_Period]);
                db.AddInParameter(db.cmd, "Document_No", data[BM_Document_No]);
                db.AddInParameter(db.cmd, "Document_Period", data[BM_Document_Period]);
                db.AddInParameter(db.cmd, "Date1", data[BM_Date1]);
                db.AddInParameter(db.cmd, "Date2", data[BM_Date2]);
                db.AddInParameter(db.cmd, "Date3", data[BM_Date3]);
                db.AddInParameter(db.cmd, "Ref_No", data[BM_Ref_No]);

                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                foreach (DataRow row in dtx.Rows)
                {
                    int ItemID = Utility.GetIntValue(row, "Item_ID");
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void ReadFeedback_BM(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");

                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string file_name = System.IO.Path.GetFileName(file);
                        try
                        {
                            string[] lines = System.IO.File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                SaveFeedback_BM(split_data);

                                Utility.SaveLog("Read Feedback PIB BeaMasuk", split_data[0], file, "", 1);
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
                            Utility.SaveLog("Read Feedback PIB BeaMasuk", "-", file, ex.Message, 0);
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

                throw ex;
            }
        }

        public void ReadFeedback_Tax(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                foreach (DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");

                    foreach (string file in System.IO.Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        string file_name = System.IO.Path.GetFileName(file);
                        try
                        {
                            string[] lines = System.IO.File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                SaveFeedback_Tax(split_data);

                                Utility.SaveLog("Read Feedback PIB Tax", split_data[0], file, "", 1);
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
                            Utility.SaveLog("Read Feedback PIB Tax", "-", file, ex.Message, 0);
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

                throw ex;
            }
        }


    }
}
