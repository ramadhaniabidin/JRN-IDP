using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.APILogics.Common
{
    public class BatchModel
    {
        public string BatchFile
        {
            get; set;
        }
    }
    public class SAPBatchLogic
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        public void CreateBatchFile(string SAPFolderID, int headerID, string fileName)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = GetFolderLocation(SAPFolderID);

                foreach (DataRow r in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(r, "Module_Code");
                    string PathLocation = Utility.GetStringValue(r, "Path_Location");
                    //DataTable dtInfo = GetProcBranch(SAPFolderID, headerID);

                    //For List Non Commercials Only
                    //foreach (DataRow row in dtInfo.Rows)
                    //{
                    //    if (!string.IsNullOrEmpty(Utility.GetStringValue(row, "BranchCode")))
                    //    {
                    //        PathLocation += @"\" + Utility.GetStringValue(row, "BranchCode") + @"\" + Utility.GetStringValue(row, "ProcDept");
                    //    }
                    //}
                    //---------------------------------------


                    var list = GetBatchFileContents(moduleCode, headerID);
                    if (list.Count > 0)
                    {
                        //var credentials = new NetworkCredential(@"daikin\lrosandy", "Aircon123");
                        var credentials = new Utility().GetNetworkCredential();
                        using (new ConnectToSharedFolder(PathLocation, credentials))
                        {
                            var formNo = list[0].BatchFile.Split('\t', ';')[0];
                            var targetPath = PathLocation;
                            var targetFile = Path.Combine(targetPath, fileName + ".txt");

                            SaveBatchFileHistory(moduleCode, headerID, formNo, targetFile);

                            #region Create Batch File
                            Directory.CreateDirectory(targetPath);
                            using (TextWriter tw = new StreamWriter(targetFile))
                                foreach (var row in list)
                                    tw.WriteLine(row.BatchFile);
                            #endregion
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

        public void SaveBatchFileHistory(string moduleCode, int headerID, string formNo, string targetFile)
        {
            var isTrans = true;
            try
            {
                db.OpenConnection(ref conn, isTrans);
                db.cmd.CommandText = "usp_BatchFileHistory_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", moduleCode);
                db.AddInParameter(db.cmd, "Header_ID", headerID);
                db.AddInParameter(db.cmd, "Form_No", formNo);
                db.AddInParameter(db.cmd, "Generated_File_Path", targetFile);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn, isTrans);
            }
            catch (Exception ex)
            {
                isTrans = false;
                db.CloseConnection(ref conn);
                throw ex;
            }
        }


        public List<BatchModel> GetBatchFileContents(string moduleCode, int headerID)
        {
            DataTable dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_CreateBatchFile";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", moduleCode);
                db.AddInParameter(db.cmd, "Header_ID", headerID);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<BatchModel>(dt);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public DataTable GetFolderLocation(string ID)
        {
            try
            {
                DataTable dt = new DataTable();
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_mastersapfolderlocation_getbyid";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "id", ID);
                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dt;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public DataTable GetProcBranch(string SAPFolderID, int headerID)
        {
            DataTable dtx = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_GetProcBranch";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "SAPFolderID", SAPFolderID);
                db.AddInParameter(db.cmd, "Header_ID", headerID);
                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dtx;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}
