using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.NonCommercials.Controller
{
    public class POReleaseController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        NintexCloudManager nacManager = new NintexCloudManager();

        #region NAC
        public static string GetToken()
        {
            string url = "https://us.nintex.io/authentication/v1/token";

            HttpClient client = new HttpClient();
            var requestBody = new
            {
                client_id = "bd3797e5-2c76-4834-90f1-70060fe10844",
                client_secret = "sKtUsKRNNtWsJLROQtUsPMtWVsMNtRTsLOtWRWsPtVsRtRsNtSPsMLItT2IsRtVsFRtTsNtUsFMOtUsOFtRsQRJFtT2utUsItRsOtSVsO2N",
                grant_type = "client_credentials"
            };
            var jsonBody = new JavaScriptSerializer().Serialize(requestBody);
            var HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = client.PostAsync(url, HttpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            Console.WriteLine(responseObject);
            string accessToken = responseObject["access_token"];

            return accessToken;
        }
        #endregion

        //Workflow stopped after Accounting Manager approved, then scheduler send txt file
        //After get feedback release, resume Approval to Finance Receiver, Finance Verifier 1, Finance Verifier 2, Tax Verification
        public void ResumeApproval(int Item_ID, string Module_Code)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NonCommercial_GetStoppedApproval";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Module_Code", Module_Code);
                db.AddInParameter(db.cmd, "Item_ID", Item_ID);
                reader = db.cmd.ExecuteReader();

                int Header_ID = 0;
                while (reader.Read())
                {
                    Header_ID = reader.GetInt32(0);
                }

                string ListName = Module_Code == "M018" ? "PO Release GA IT" : "";


                SPWeb web = new SPSite(Utility.SpSiteUrl_DEV).OpenWeb();
                SPList list = web.Lists[ListName];
                web.AllowUnsafeUpdates = true;

                SPListItem item;

                if (Item_ID > 0)
                {
                    item = list.GetItemById(Item_ID);
                    item["Form Status"] = "Start";
                    item.Update();
                    web.AllowUnsafeUpdates = false;
                }

                Task.Run(async () => { await nacManager.NonCommercial_StartWorkflow(Item_ID, Header_ID, Module_Code, ListName); }).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
