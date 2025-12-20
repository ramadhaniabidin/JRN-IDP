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

namespace Daikin.BusinessLogics.Apps.Batch.Controller
{
    public class SAP2Controller
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        SharePointManager spManager = new SharePointManager();


        static Utility util = new Utility();
        static string basePath = util.GetConfigValue("NetworkPath");
        NetworkCredential credentials = util.GetNetworkCredential();

        string sapPath = Path.Combine(basePath, "Batch 1", "SAP Posting");

        public string ChangeFormat(string date)
        {
            var dates = date.Split('.');

            return dates[2] + "-" + dates[1] + "-" + dates[0];
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

        //2700000013;2023-11-01;2023.09.07;2023.09.13;09:05:19;BR23090189;     858500
        //Doc No;Post Date;Doc Date;Due Date;Time;Reference/Nintex No; Amount

        public const int IDX_DOC_NO = 0;
        public const int IDX_POST_DATE = 1;
        public const int IDX_DOC_DATE = 2;
        public const int IDX_DUE_DATE = 3;
        public const int IDX_TIME = 4;
        public const int IDX_REFERENCE = 5;
        public const int IDX_AMOUNT = 6;

        //public void ReadBatchHandler(string filePath)
        //{
        //    var itemIDs = new List<int>();
        //    var siteURL = util.GetConfigValue("SiteUrl");
        //    var listName = util.GetConfigValue("TransList");
        //    var workflowName = util.GetConfigValue("TransListWorkflow");

        //    using (new ConnectToSharedFolder(basePath, credentials))
        //    {
        //        #region For Each Row UpdateBatchFileHistory
        //        var rows = File.ReadAllLines(filePath).ToList();
        //        var rowsError = new List<string>();
        //        foreach (var row in rows)
        //        {


        //            //2700000013;2023-11-01;2023.09.07;2023.09.13;09:05:19;BR23090189;     858500
        //            //Doc No;Post Date;Doc Date;Due Date;Time;Reference/Nintex No; Amount

        //            var item = row.Split(';');
        //            var financeDocument = item[IDX_DOC_NO];
        //            var postingDate = item[IDX_POST_DATE] + " " + item[IDX_TIME];
        //            var reference = item[IDX_REFERENCE];
        //            var amount = item[IDX_AMOUNT].Replace(",", "").Trim();

        //            //var Due_Date = Utility.GetValueFromArray(item, 5);
        //            //var Document_SAP_Date = Utility.GetValueFromArray(item, 6);

        //            var Due_Date = item[IDX_DUE_DATE];
        //            var Document_SAP_Date = item[IDX_DOC_DATE];

        //            try
        //            {
        //                var itemID = UpdateBatchHistory(reference, financeDocument, postingDate, amount, "Success Post",
        //                             filePath, Due_Date, Document_SAP_Date);

        //                if (itemID > 0)
        //                {
        //                    //SPSecurity.RunWithElevatedPrivileges(delegate ()
        //                    //{
        //                    //    var spSite = new SPSite(siteURL);
        //                    //    SPWeb spWeb = spSite.OpenWeb();
        //                    //    SPList spList = spWeb.Lists.TryGetList(listName);

        //                    //    spWeb.AllowUnsafeUpdates = true;
        //                    //    var spItem = spList.GetItemById(itemID);
        //                    //    spItem["Form Status"] = "Start";
        //                    //    spItem["Batch2 Park"] = true;
        //                    //    spItem["Finance Status"] = "8";
        //                    //    spItem.Update();
        //                    //    spWeb.AllowUnsafeUpdates = false;
        //                    //});
        //                    itemIDs.Add(itemID);
        //                }
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

        //        //foreach (var itemID in itemIDs)
        //        //    spManager.StartWorkflowBySystemAccount(siteURL, itemID, workflowName, listName);

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
        //                var name = r_newFileName.Replace(r_ext, "");
        //                while (true)
        //                {
        //                    count++;
        //                    var newDestFilePath = Path.Combine(r_finalPath, name + "_" + count + r_ext);
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
