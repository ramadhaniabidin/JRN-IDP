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
using Microsoft.SharePoint;
using System.Configuration;
using Microsoft.SharePoint.Workflow;

namespace Daikin.BusinessLogics.Apps.Batch.Controller
{

    public class SAP1Controller
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager spManager = new SharePointManager();

        static Utility util = new Utility();
        static string basePath = util.GetConfigValue("NetworkPath");
        NetworkCredential credentials = util.GetNetworkCredential();

        string sapPath = Path.Combine(basePath, "Batch 1", "SAP Parking");

        public string ChangeFormat(string date)
        {
            if (string.IsNullOrEmpty(date))
                return date;

            var dates = date.Split('.');
            return dates[2] + "-" + dates[1] + "-" + dates[0];
        }

        public int UpdateBatchHistory(string formNo, string requestDate, string documentNumberSAP, string postingDate, string status, string filePath)
        {
            var isTrans = true;
            var itemID = 0;
            dt = new DataTable();

            try
            {
                db.OpenConnection(ref conn, isTrans);

                db.cmd.CommandText = "usp_BatchFileHistory_UpdateSAPFeedback";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "formNo", formNo);
                db.AddInParameter(db.cmd, "status", status);
                db.AddInParameter(db.cmd, "requestDate", ChangeFormat(requestDate));
                db.AddInParameter(db.cmd, "documentNumberSAP", documentNumberSAP);
                db.AddInParameter(db.cmd, "postingDate", ChangeFormat(postingDate));
                db.AddInParameter(db.cmd, "filePath", filePath);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn, isTrans);


                if (dt.Rows.Count < 1)
                {
                    db.CloseConnection(ref conn);
                    throw new Exception("There is no item with Form_No \"" + formNo + "\" in BatchFileHistory");
                }

                itemID = Convert.ToInt32(dt.Rows[0][0].ToString());
                return itemID;
            }
            catch (Exception ex)
            {
                isTrans = false;
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        //public void ReadBatchFeedbacks(ref int total, ref int count)
        //{
        //    //DirectoryInfo folder = new DirectoryInfo(sapPath);
        //    //FileInfo[] filePaths = folder.GetFiles().OrderBy(p => p.CreationTime).ToArray();
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

        //public void ReadBatchHandler(string filePath)
        //{
        //    var isError = false;
        //    var itemIDs = new List<int>();
        //    var siteURL = util.GetConfigValue("SiteUrl");
        //    var listName = util.GetConfigValue("TransList");
        //    var workflowName = util.GetConfigValue("TransListWorkflow");

        //    using (new ConnectToSharedFolder(basePath, credentials))
        //    {
        //        try
        //        {

        //            #region For Each Row UpdateBatchFileHistory
        //            var rows = File.ReadAllLines(filePath).ToList();
        //            foreach (var row in rows)
        //            {
        //                var item = row.Split('\t', ';');
        //                var formNo = item[0];
        //                var requestDate = item[1];
        //                var documentNumberSAP = item[2];
        //                var postingDate = item[3];
        //                var status = item[4];

        //                var itemID = UpdateBatchHistory(formNo, requestDate, documentNumberSAP, postingDate, status, filePath);

        //                if (status.Contains("Success"))
        //                {
        //                    //SPSecurity.RunWithElevatedPrivileges(delegate ()
        //                    //{
        //                    //    var spSite = new SPSite(siteURL);
        //                    //    SPWeb spWeb = spSite.OpenWeb();
        //                    //    SPList spList = spWeb.Lists.TryGetList(listName);

        //                    //    spWeb.AllowUnsafeUpdates = true;
        //                    //    var spItem = spList.GetItemById(itemID);
        //                    //    spItem["Form Status"] = "Start";
        //                    //    spItem["Feedback SAP 1"] = true;
        //                    //    spItem.Update();
        //                    //    spWeb.AllowUnsafeUpdates = false;
        //                    //});


        //                    itemIDs.Add(itemID);
        //                }
        //            }
        //            #endregion

        //            #region Move File
        //            var endPath = isError ? "ERROR" : "DONE";
        //            var finalPath = Path.Combine(sapPath, endPath);
        //            var fileName = filePath.Split('\\').Last();
        //            var destFilePath = Path.Combine(finalPath, fileName);

        //            Directory.CreateDirectory(finalPath);
        //            if (File.Exists(destFilePath))
        //            {
        //                var count = 0;
        //                while (true)
        //                {
        //                    count++;
        //                    var name = fileName.Replace(".txt", "");
        //                    var newDestFilePath = Path.Combine(finalPath, name + "_" + count + ".txt");
        //                    if (!File.Exists(newDestFilePath))
        //                    {
        //                        destFilePath = newDestFilePath;
        //                        break;
        //                    }
        //                }

        //            }
        //            File.Move(filePath, destFilePath);
        //            #endregion
        //        }
        //        catch (Exception ex)
        //        {
        //            db.CloseConnection(ref conn);
        //            isError = true;

        //            #region Move File
        //            var endPath = isError ? "ERROR" : "DONE";
        //            var finalPath = Path.Combine(sapPath, endPath);
        //            var fileName = filePath.Split('\\').Last();
        //            var destFilePath = Path.Combine(finalPath, fileName);

        //            Directory.CreateDirectory(finalPath);
        //            if (File.Exists(destFilePath))
        //            {
        //                var count = 0;
        //                while (true)
        //                {
        //                    count++;
        //                    var name = fileName.Replace(".txt", "");
        //                    var newDestFilePath = Path.Combine(finalPath, name + "_" + count + ".txt");
        //                    if (!File.Exists(newDestFilePath))
        //                    {
        //                        destFilePath = newDestFilePath;
        //                        break;
        //                    }
        //                }

        //            }
        //            File.Move(filePath, destFilePath);
        //            #endregion

        //            throw ex;
        //        }

        //    }

        //    //if (!isError)
        //    //    foreach (var itemID in itemIDs)
        //    //        spManager.StartWorkflowBySystemAccount(siteURL, itemID, workflowName, listName);
        //}

    }
}
