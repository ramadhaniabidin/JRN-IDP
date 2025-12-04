using Daikin.BusinessLogics.Apps.Batch;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class SAPController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager sp = new SharePointManager();
        Utility util = new Utility();
        string ListName = "Non Commercials";

        #region Index SAP Feedback Release
        public const int R_IDX_NintexNo = 0;
        public const int R_IDX_PONo = 1;
        public const int R_IDX_ReleaseCode = 2;
        public const int R_IDX_Status = 3;
        #endregion

        public void SaveFeedbackRelease(string[] data, out int Item_ID, out int Processed)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_SAPFeedbackPORelease_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Nintex_No", data[R_IDX_NintexNo]);
                db.AddInParameter(db.cmd, "PO_No", data[R_IDX_PONo]);
                db.AddInParameter(db.cmd, "Release_Code", data[R_IDX_ReleaseCode]);
                db.AddInParameter(db.cmd, "Status", data[R_IDX_Status]);

                reader = db.cmd.ExecuteReader();
                DataTable dtx = new DataTable();
                dtx.Load(reader);

                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                Item_ID = 0;
                Processed = 0;

                foreach (DataRow row in dtx.Rows)
                {
                    Item_ID = Utility.GetIntValue(row, "Item_ID");
                    Processed = Utility.GetIntValue(row, "Processed");
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public void ReadFeedbackPORelease(string SAPFolderID)
        {
            try
            {
                dt = new DataTable();
                dt = new BatchController().GetFolderLocation(SAPFolderID);
                int i = 0;
                foreach(DataRow row in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(row, "Module_Code");
                    string folder = Utility.GetStringValue(row, "Path_Location");
                    foreach(string file in Directory.EnumerateFiles(folder, ".txt"))
                    {
                        string file_name = Path.GetFileName(file);
                        if(i == 5)
                        {
                            return;
                        }
                        try
                        {
                            string[] lines = File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                string[] split_data = line.Split(';');
                                int Item_ID = 0; int Processed = 0;
                                SaveFeedbackRelease(split_data, out Item_ID, out Processed);

                                if (Item_ID > 0 && Processed > 0)
                                {
                                    Console.WriteLine(split_data[0] + " already processed");
                                }

                                if (split_data[R_IDX_Status].ToUpper().Contains("SUCCESS"))
                                {
                                    if (Item_ID > 0 && Processed == 0) new POReleaseController().ResumeApproval(Item_ID, moduleCode);
                                }


                                Utility.SaveLog("Read Feedback Release Group Non Commercials", split_data[0], file, "", 1);
                                Console.WriteLine(line);

                            }
                            string DoneFilePath = folder + "\\DONE\\" + file_name;
                            if (File.Exists(DoneFilePath))
                            {
                                File.Delete(DoneFilePath);
                            }
                            File.Move(folder + "\\" + file_name, DoneFilePath);
                        }
                        catch(Exception ex)
                        {
                            Utility.SaveLog("Read Feedback PO Non Commercials", "-", file, ex.Message, 0);
                            string errorFilePath = folder + "\\ERROR\\" + file_name;
                            if (File.Exists(errorFilePath))
                            {
                                File.Delete(errorFilePath);
                            }
                            File.Move(folder + "\\" + file_name, errorFilePath);
                        }
                        i++;
                    }
                }
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}
