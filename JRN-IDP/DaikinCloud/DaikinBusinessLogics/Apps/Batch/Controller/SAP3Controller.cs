using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daikin.BusinessLogics.Common;
using System.Data.SqlClient;
using System.Data;
using Daikin.BusinessLogics.Apps.Batch.Model;
using System.IO;
using System.Net;

namespace Daikin.BusinessLogics.Apps.Batch.Controller
{
    public class SAP3Controller
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        static Utility util = new Utility();
        static string basePath = util.GetConfigValue("NetworkPath");
        NetworkCredential credentials = util.GetNetworkCredential();

        string sapPath = Path.Combine(basePath, "Batch 2", "SAP");

        public void UpdateBatchHistory(string documentNumber, string transactionNo, string status, string modifiedBy)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);

                db.cmd.CommandText = "usp_BatchFile2HistoryDetail_UpdateFeedback";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Document_Number", documentNumber);
                db.AddInParameter(db.cmd, "Transaction_No", transactionNo);
                db.AddInParameter(db.cmd, "Status", status);
                db.AddInParameter(db.cmd, "Modified_By", modifiedBy);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                if (dt.Rows.Count < 1)
                {
                    throw new Exception("There is no item with Document_Number \"" + documentNumber + "\" of \"" + transactionNo + "\" in BatchFile2History");
                }
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //public void ReadBatchFeedbacks(ref int total, ref int count)
        //{
        //    var filePaths = Directory.GetFiles(sapPath);
        //    foreach (var filePath in filePaths)
        //    {
        //        try
        //        {
        //            total++;
        //            ReadBatchHandler(filePath);
        //            count++;
        //        }
        //        catch (Exception ex)
        //        {
        //            Utility.WriteToFile(ex.ToString());
        //            Utility.WriteToFile("   FilePath: " + filePath, false);
        //        }

        //    }
        //}

        //public void ReadBatchHandler(string filePath, string modifiedBy = "")
        //{
        //    if (string.IsNullOrEmpty(modifiedBy))
        //        modifiedBy = credentials.UserName;

        //    using (new ConnectToSharedFolder(basePath, credentials))
        //    {

        //        #region For Each Row UpdateBatchFileHistory
        //        var rows = File.ReadAllLines(filePath).ToList();
        //        var rowsError = new List<string>();
        //        foreach (var row in rows)
        //        {
        //            var item = row.Split('\t', ';');
        //            //var no = item[0];
        //            //var identification = item[1];
        //            //var runDate = ChangeFormat(item[2]);
        //            var documentNumber = item[3];
        //            //var fiscalYear = item[4];
        //            var status = item[5];
        //            var transactionNo = item[6].Replace(".txt", "");
        //            try
        //            {
        //                UpdateBatchHistory(documentNumber, transactionNo, status, modifiedBy);
        //            }
        //            catch (Exception ex)
        //            {
        //                db.CloseConnection(ref conn);
        //                rowsError.Add(row);

        //                Utility.WriteToFile(ex.ToString());
        //                Utility.WriteToFile(filePath, false);
        //            }
        //        }
        //        #endregion

        //        var allError = rowsError.Count == rows.Count;
        //        #region Create File if error
        //        if (rowsError.Count > 0 && !allError)
        //        {
        //            var r_endPath = "ERROR";
        //            var r_finalPath = Path.Combine(sapPath, r_endPath);
        //            var r_fileName = filePath.Split('\\').Last();
        //            var r_ext = ".txt";
        //            var r_newFileName = r_fileName.Replace(r_ext, "") + "_ERROR" + r_ext;
        //            var r_destFilePath = Path.Combine(r_finalPath, r_newFileName);

        //            Directory.CreateDirectory(r_finalPath);
        //            if (File.Exists(r_destFilePath))
        //            {
        //                var count = 0;
        //                var name = r_newFileName.Replace(".txt", "");
        //                while (true)
        //                {
        //                    count++;
        //                    var newDestFilePath = Path.Combine(r_finalPath, name + "_" + count + ".txt");
        //                    if (!File.Exists(newDestFilePath))
        //                    {
        //                        r_destFilePath = newDestFilePath;
        //                        break;
        //                    }
        //                }

        //            }
        //            File.WriteAllLines(r_destFilePath, new List<string>(rowsError));
        //            //File.Move(filePath, r_destFilePath);
        //        }
        //        #endregion

        //        #region Move File
        //        var endPath = allError ? "ERROR" : "DONE";
        //        var finalPath = Path.Combine(sapPath, endPath);
        //        var fileName = filePath.Split('\\').Last();
        //        var destFilePath = Path.Combine(finalPath, fileName);

        //        Directory.CreateDirectory(finalPath);
        //        if (File.Exists(destFilePath))
        //        {
        //            var count = 0;
        //            var name = fileName.Replace(".txt", "");
        //            while (true)
        //            {
        //                count++;
        //                var newDestFilePath = Path.Combine(finalPath, name + "_" + count + ".txt");
        //                if (!File.Exists(newDestFilePath))
        //                {
        //                    destFilePath = newDestFilePath;
        //                    break;
        //                }
        //            }

        //        }
        //        File.Move(filePath, destFilePath);
        //        #endregion

        //        if (allError)
        //            throw new Exception("All items in the file failed to process.");
        //        else if (rowsError.Count > 0)
        //            throw new Exception("Some items in the file failed to process.");
        //    }
        //}

    }
}
