using Daikin.JobSchedulers.Common;
using Daikin.JobSchedulersLogic.Common.Model;
using Daikin.JobSchedulersLogic.Model;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using Daikin.JobSchedulersLogic.Controller;
using Daikin.JobSchedulersLogic.Common;

namespace Daikin.JobSchedulers
{
    class Program
    {
        static DatabaseManager db = new DatabaseManager();
        static SqlConnection conn = new SqlConnection();
        static SqlDataReader reader = null;
        static DataTable dt = new DataTable();
        private readonly static NintexCloudManager ntxManager = new NintexCloudManager();
        private readonly static List<string> fobServiceCostActions = new List<string> { "1", "2", "3", "4" };
        private readonly static List<string> nonCommercialActions = new List<string> { "8", "9", "14", "17", "18", "20" };
        private readonly static List<string> poSubconActions = new List<string> { "6", "7", "15", "16", "13", "30" };

        public static string GetSiteURL()
        {
            try
            {
                return ConfigurationManager.AppSettings["SiteUrl_DEV"];
            }
            catch
            {
                return "http://spdev:3473";
            }
        }
        public static string GetFunctionCode()
        {
            try
            {
                return ConfigurationManager.AppSettings["FunctionCode"];
            }
            catch
            {
                return "0";
            }
        }

        static bool isFOBServiceCostActions(string F_Code)
        {
            return fobServiceCostActions.Contains(F_Code);
        }

        static bool isNonCommercialActions(string F_Code)
        {
            return nonCommercialActions.Contains(F_Code);
        }

        static bool isPOSubcon(string F_Code)
        {
            return poSubconActions.Contains(F_Code);
        }

        static void Main(string[] args)
        {
            try
            {
                string F_Code = GetFunctionCode();
                SAPController sap = new SAPController();
                SAPSubconController sapSubcon = new SAPSubconController();
                POSubconController poSubcon = new POSubconController();
                BusinessPartnerController bp = new BusinessPartnerController();
                CommonLogic func = new CommonLogic();
                string ldap = ConfigurationManager.AppSettings["LDAP"];
                string user = ConfigurationManager.AppSettings["NetworkUser"];
                string pass = ConfigurationManager.AppSettings["NetworkPass"];
                #region Generate Autocode
                if (F_Code == "0")
                {
                    var siteURL = ConfigurationManager.AppSettings["SiteUrl"];
                    var result = GenerateCode_V2(siteURL);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Generate Code " + result + " items");
                }
                #endregion

                #region FOB, LC / SERVICE COST
                else if (isFOBServiceCostActions(F_Code))
                {
                    FOBServiceCostActions(F_Code);
                }
                #endregion  

                #region Sync Sharepoint to Database
                else if (F_Code == "5")
                {
                    new SyncSharePointList().Sync("https://sp3.daikin.co.id:3473");
                }
                #endregion

                #region NON COMMERCIAL
                else if (isNonCommercialActions(F_Code))
                {
                    NonCommercialActions(F_Code);
                }
                #endregion

                #region COMMERCIAL SUBCON
                else if (isPOSubcon(F_Code))
                {
                    POSubconActions(F_Code, bp);
                }
                #endregion

                #region Batch 1 Feedback
                else if (F_Code == "33") //Batch 1 Feedback 
                {
                    var count = 0;
                    var total = 0;
                    SAP2Controller ctrl = new SAP2Controller();
                    try
                    {
                        ctrl.ReadBatchFeedbacks(ref total, ref count);
                        Utility.WriteToFile($"Batch 2 - Successfully Read SAP Feedbacks {count} of {total} {(total == 1 ? "file" : "files")}");
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteToFile(ex.ToString());
                    }

                }
                #endregion

                #region Batch 2 Feedback
                else if (F_Code == "34") //Batch 2 Feedback
                {
                    SAP3Controller ctrl = new SAP3Controller();

                    var count = 0;
                    var total = 0;

                    try
                    {
                        ctrl.ReadBatchFeedbacks(ref total, ref count);
                        Utility.WriteToFile($"Batch 2 - Successfully Read SAP Feedbacks {count} of {total} {(total == 1 ? "file" : "files")}");
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteToFile(ex.ToString());
                    }
                }
                #endregion

                #region PIB SAP Feedback
                else if (F_Code == "36") //PIB SAP Feedback
                {
                    try
                    {
                        new SAPPIBController().ReadFeedback_BM("22");
                        new SAPPIBController().ReadFeedback_Tax("23");
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteToFile("Failed to read PIB SAP Feedback " + ex.Message);

                    }
                }
                #endregion

                #region SAP BP Feedback
                else if (F_Code == "40")
                {
                    try
                    {
                        new SAPController().ReadFeedbackSAP_PAL("33");
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteToFile("Failed to read PAL SAP Feedback " + ex.Message);
                    }
                }
                #endregion

                #region Get All user properties
                else if (F_Code == "GET USER PROPS")
                {
                    try
                    {
                        Console.Write("Insert email: ");
                        string email = Console.ReadLine();
                        var attributes = func.GetAllAdAttributesByEmail(email, ldap, user, pass);
                        Console.WriteLine($"Job Title: {attributes["title"]}");
                        Console.WriteLine($"Department: {attributes["department"]}");
                        var manager = attributes["manager"];
                        var managerData = func.GetManagerData(manager.ToString());
                        Console.WriteLine($"Manager distinguisnedName: {manager}");
                        Console.WriteLine($"Email: {managerData["Manager Email"]}");
                        Console.WriteLine($"Full Name: {managerData["Manager Name"]}");
                        Console.WriteLine();
                        foreach (var key in attributes.Keys)
                        {
                            Console.WriteLine($"{key}: {attributes[key]}");
                        }
                        Console.ReadKey();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.ReadKey();
                    }
                }
                #endregion

                #region Get user groups
                else if (F_Code == "GET USER GROUPS")
                {
                    Console.Write("GET USER GROUPS\n");
                    Console.Write("Insert user email: ");
                    string account = Console.ReadLine();
                    var groups = func.GetUserGroups(account);
                    foreach (var group in groups)
                    {
                        Console.WriteLine($"Group name: {group}");
                    }
                    Console.WriteLine("Complete");
                    Console.ReadKey();
                }

                #region Get all groups
                else if (F_Code == "GET ALL GROUPS")
                {
                    Console.WriteLine(F_Code);
                    var groups = func.GetAllADGroups();
                    foreach (var group in groups)
                    {
                        Console.WriteLine($"Group name: {group}");
                    }
                    Console.WriteLine("Complete");
                    Console.ReadKey();
                }
                #endregion

                #region Get User Manager
                else if (F_Code == "GET USER MANAGER")
                {
                    Console.WriteLine(F_Code);
                    Console.Write("Insert user email: ");
                    string email = Console.ReadLine();
                    string managerDistinguish = func.GetManagerDistinguishedName(email);
                    var managerData = func.GetManagerData(managerDistinguish);
                    Console.WriteLine($"Manager name: {managerData["Manager Name"]}");
                    Console.WriteLine($"Manager email: {managerData["Manager Email"]}");
                    Console.WriteLine("Complete");
                    Console.ReadKey();
                }
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - " + ex);
            }

        }

        static void FOBServiceCostActions(string F_Code)
        {
            SAPController sap = new SAPController();
            switch (F_Code)
            {
                case "1":
                    sap.Read_SAP_Inbound();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Inbound ");
                    break;

                case "2":
                    sap.Read_SAP_OutstandingAP();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Outstanding AP ");
                    break;

                case "3":
                    sap.Read_SAP_Rebate();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Rebate AP ");
                    break;

                case "4":
                    sap.ReadFeedbackSAP_ServiceCost();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Feedback LC ");
                    break;
            }
        }

        static void NonCommercialActions(string F_Code)
        {
            SAPController sap = new SAPController();
            POHeaderController po = new POHeaderController();
            switch (F_Code)
            {
                case "9":
                    sap.ReadFeedbackPOCreate(2);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Feedback PO Create");

                    sap.ReadSAPVendorData(5);

                    sap.ReadSAPPOData("4");
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP PO & Vendor Data");

                    break;

                case "14":
                    sap.ProcessDigiSign(3);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully DigiSign Process PO Non Comm");
                    break;

                case "8":
                    sap.ReadFeedbackMIRO("16");
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Feedback MIRO");
                    break;

                case "17":
                    sap.ReadSAPPOGR(13).GetAwaiter().GetResult();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read PO GR Data Non Commercials");
                    break;

                case "18":
                    sap.ReadFeedbackPORelease(17).GetAwaiter().GetResult();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read Feedback SAP PO Release");
                    break;

                case "20":
                    po.InsertVendorBankToSPList();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Insert Vendor Bank to SP List");
                    break;
            }
        }

        static void POSubconActions(string F_Code, BusinessPartnerController bp)
        {
            SAPController sap = new SAPController();
            SAPSubconController sapSubcon = new SAPSubconController();
            POSubconController poSubcon = new POSubconController();
            switch (F_Code)
            {
                case "6":
                    sap.ReadSAPVendorData(12);
                    WriteToFile(DateTime.Now.ToString("dd MM yyyy HH:mm:ss tt") + " - Successfully Read Commercial Subcon");

                    sapSubcon.ReadCommercialSubcon(10);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read Commercial Subcon");

                    break;

                case "7":
                    poSubcon.SaveBulkSPList_V2();
                    WriteToFile(DateTime.Now.ToString("dd MM yyyy HH:mm:ss tt") + " - Successfully Save SP List Commercial Subcon");
                    break;

                case "15":
                    sapSubcon.SaveAttachmentPOSubcon(7);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Save Attachment PO Subcon");
                    break;

                case "16":
                    sapSubcon.ReadFeedbackMIRO(6);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read Feedback MIRO Subcon");
                    break;

                case "13":
                    sapSubcon.ReadCommercialSubconGR(11);
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Read SAP Commercial Subcon GR");
                    break;

                case "30":
                    bp.ProcessPendingBP();
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - Successfully Process Pending Business Partner");
                    break;
            }
        }

        public static List<AutoCodeBatch> GetListData()
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NWC_AutoCodeBatch_GetList"; //prod
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                var list = Utility.ConvertDataTableToList<AutoCodeBatch>(dt);
                return list;
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public async static Task<List<AutoCodeBatch>> GetListDataAsync(SqlConnection _conn, SqlTransaction _trans)
        {
            if (_conn.State == ConnectionState.Closed) await _conn.OpenAsync().ConfigureAwait(false);
            using (var cmd = new SqlCommand("usp_NWC_AutoCodeBatch_GetList", _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (var _r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                using (var _t = new DataTable())
                {
                    _t.Load(_r);
                    return Utility.ConvertDataTableToList<AutoCodeBatch>(_t);
                }
            }
        }

        public static void StartWorkflowAfterGenerateCode(string Module_Code, int Item_ID, int Transaction_ID, string List_Name)
        {
            Task.Run(async () =>
            {
                var nwc = ntxManager.GenerateNACPayload(Transaction_ID, Item_ID, Module_Code, List_Name);
                await ntxManager.StartNWC(nwc);
            }).Wait();
        }

        public async static Task StartWorkflowAfterGenerateCode(string Module_Code, int Item_ID, int Transaction_ID, string List_Name, SqlConnection _conn, SqlTransaction _trans)
        {
            var nwc = await ntxManager.GenerateNACPayload(Transaction_ID, Item_ID, Module_Code, List_Name, _conn, _trans)
                .ConfigureAwait(false);
            await ntxManager.StartNWC(nwc, _conn, _trans).ConfigureAwait(false);
        }

        public static void AutoCodeUpdateFlag(int id, string message, int code)
        {
            using (var _con = new SqlConnection(Utility.GetSqlConnection()))
            {
                _con.Open();
                using (var cmd = new SqlCommand("usp_AutoCodeBatch_UpdateFlag", _con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@ID", Value = id, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@Generated", Value = code, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@SysMessage", Value = message, SqlDbType = SqlDbType.VarChar, Size = -1, Direction = ParameterDirection.Input });
                    cmd.ExecuteNonQuery();
                }
                _con.Close();
            }
        }

        public async static Task AutoCodeUpdateFlag(int id, string message, int code, SqlConnection _conn, SqlTransaction _trans)
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync().ConfigureAwait(false);
            }
            using (var cmd = new SqlCommand("usp_AutoCodeBatch_UpdateFlag", _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@ID", Value = id, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@Generated", Value = code, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@SysMessage", Value = message, SqlDbType = SqlDbType.VarChar, Size = -1, Direction = ParameterDirection.Input });
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public static string GenerateAutoCode(AutoCodeBatch item)
        {
            try
            {
                using (var _con = new SqlConnection(Utility.GetSqlConnection()))
                {
                    _con.Open();
                    using (var cmd = new SqlCommand("usp_Utility_AutoCounter", _con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@FieldName", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@TableName", Value = item.TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@FieldCriteria", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@ValueCriteria", Value = item.Format, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter { ParameterName = "@LengthOfString", Value = item.LengthOfString, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                        using (var _reader = cmd.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(_reader);
                            return Utility.ConvertDataTableToList<GeneratedCode>(dt)[0].AutoCode;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                AutoCodeUpdateFlag(item.ID, ex.Message, 2);
                return string.Empty;
            }
        }

        public async static Task<string> GenerateAutoCode(AutoCodeBatch item, SqlConnection _conn, SqlTransaction _trans)
        {
            string code = "";
            if (_conn.State == ConnectionState.Closed) await _conn.OpenAsync().ConfigureAwait(false);
            using (var cmd = new SqlCommand("usp_Utility_AutoCounter", _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@FieldName", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@TableName", Value = item.TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@FieldCriteria", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@ValueCriteria", Value = item.Format, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@LengthOfString", Value = item.LengthOfString, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                using (var _reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await _reader.ReadAsync().ConfigureAwait(false))
                    {
                        code = await _reader.IsDBNullAsync(_reader.GetOrdinal("AutoCode")) ? "" : Convert.ToString(_reader["AutoCode"]);
                    }
                }
            }
            return code;
        }

        public static void UpdateTransactionItem(AutoCodeBatch item, string autoCode)
        {
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                _conn.Open();
                using (var cmd = new SqlCommand("usp_NWC_AutoCodeBatch_UpdateItem", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@TransID", Value = item.TransID, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@TableName", Value = item.TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@DetailName", Value = item.DetailName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@ColumnName", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@AutoCode", Value = autoCode, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.ExecuteNonQuery();
                }
                _conn.Close();
            }
        }

        public async static Task UpdateTransactionItem(AutoCodeBatch item, string autoCode, SqlConnection _conn, SqlTransaction _trans)
        {
            if (_conn.State == ConnectionState.Closed) await _conn.OpenAsync().ConfigureAwait(false);
            using (var cmd = new SqlCommand("usp_NWC_AutoCodeBatch_UpdateItem", _conn, _trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@TransID", Value = item.TransID, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@TableName", Value = item.TableName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@DetailName", Value = item.DetailName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@ColumnName", Value = item.ColumnName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@AutoCode", Value = autoCode, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public static void UpdateListItem(string SiteUrl, AutoCodeBatch item, string autoCode)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                SPSite spSite = new SPSite(SiteUrl);
                SPWeb spWeb = spSite.OpenWeb();
                SPList spList = spWeb.Lists.TryGetList(item.ListName);

                spWeb.AllowUnsafeUpdates = true;
                SPListItem spItem = spList.GetItemById(item.ItemID.Value);
                spItem["Title"] = autoCode;
                spItem["Request No"] = autoCode;
                spItem["Approval Status"] = "Generated";
                spItem["Approval Status ID"] = 3;
                spItem["Workflow Status"] = "Approval";
                spItem["Form Status"] = "Start";
                spItem.Update();
                spWeb.AllowUnsafeUpdates = false;
            });
        }

        public static void UpdateListItem(AutoCodeBatch item, string autoCode)
        {
            string siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteUrl))
                using (SPWeb web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPList list = web.Lists.TryGetList(item.ListName);
                    SPListItem spItem = list.GetItemById(item.ItemID.Value);
                    spItem["Title"] = autoCode;
                    spItem["Request No"] = autoCode;
                    spItem["Approval Status"] = "Generated";
                    spItem["Approval Status ID"] = 3;
                    spItem["Workflow Status"] = "Approval";
                    spItem["Form Status"] = "Start";
                    spItem.Update();
                    web.AllowUnsafeUpdates = false;
                }
            });
        }


        public static void InsertHistoryLog(AutoCodeBatch item, string SiteUrl)
        {
            HashSet<string> ModuleHL = new HashSet<string> { "M001", "M016", "M002", "M003", "M004", "M012", "M025" };
            SPWeb web = new SPSite(SiteUrl).OpenWeb();
            SPList listData = web.Lists[item.ListName];
            SPListItem listItem = listData.GetItemById(item.ItemID.Value);
            if (!ModuleHL.Contains(item.ModuleCode))
            {
                try
                {
                    using (var _con = new SqlConnection(Utility.GetSqlConnection()))
                    {
                        _con.Open();
                        using (var cmd = new SqlCommand("usp_NWC_insertHistoryLog", _con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@listName", Value = item.ListName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@listItemID", Value = item.ItemID.Value, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@action", Value = 1, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLogin", Value = listItem["Author"].ToString(), SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLoginName", Value = listItem["Requester_x0020_Name"].ToString(), SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                            cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLayer", Value = "0", SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                            cmd.ExecuteNonQuery();
                        }
                        _con.Close();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async static Task InsertHistoryLog(AutoCodeBatch item, SqlConnection _conn, SqlTransaction _trans)
        {
            string siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
            HashSet<string> ModuleHL = new HashSet<string> { "M001", "M016", "M002", "M003", "M004", "M012", "M025" };
            using (SPSite site = new SPSite(siteUrl))
            using (SPWeb web = site.OpenWeb())
            {
                SPListItem listItem = web.Lists[item.ListName].GetItemById(item.ItemID.Value);
                if (_conn.State == ConnectionState.Closed) await _conn.OpenAsync().ConfigureAwait(false);
                using (var cmd = new SqlCommand("usp_NWC_insertHistoryLog", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@listName", Value = item.ListName, SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@listItemID", Value = item.ItemID.Value, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@action", Value = 1, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLogin", Value = listItem["Author"].ToString(), SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLoginName", Value = listItem["Requester_x0020_Name"].ToString(), SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@currentLayer", Value = "0", SqlDbType = SqlDbType.VarChar, Direction = ParameterDirection.Input });
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public static string GenerateCode_V2(string SiteUrl)
        {
            var total = 0;
            var count = 0;
            foreach (var item in GetListData())
            {
                total++;
                try
                {
                    string generatedCode = GenerateAutoCode(item);
                    AutoCodeUpdateFlag(item.ID, "Generated", 1);
                    UpdateTransactionItem(item, generatedCode);
                    UpdateListItem(SiteUrl, item, generatedCode);
                    InsertHistoryLog(item, SiteUrl);
                    StartWorkflowAfterGenerateCode(item.ModuleCode, (int)item.ItemID, (int)item.TransID, item.ListName);
                    count++;
                }
                catch (Exception ex)
                {
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + " - " + ex.Message);
                    WriteToFile("   " + SiteUrl + " - " + item.ListName + " - " + item.ItemID.Value);
                    WriteToFile("   " + item.TableName + " - " + item.DetailName + " - " + item.TransID);
                    AutoCodeUpdateFlag(item.ID, ex.Message, 2);
                }
            }
            return $"{count} of {total}";
        }

        public async static Task<string> GenerateCode()
        {
            var count = 0;
            var total = 0;
            string SiteUrl = ConfigurationManager.AppSettings["SiteUrl"];
            using (var _conn = new SqlConnection(Utility.GetSqlConnection()))
            {
                await _conn.OpenAsync().ConfigureAwait(false);
                var list = await GetListDataAsync(_conn, null).ConfigureAwait(false);
                foreach (var item in list)
                {
                    total++;
                    using (var _trans = _conn.BeginTransaction())
                    {
                        try
                        {
                            string generatedCode = await GenerateAutoCode(item, _conn, _trans).ConfigureAwait(false);
                            await AutoCodeUpdateFlag(item.ID, "Generated", 1, _conn, _trans).ConfigureAwait(false);
                            await UpdateTransactionItem(item, generatedCode, _conn, _trans);
                            UpdateListItem(SiteUrl, item, generatedCode);
                            await InsertHistoryLog(item, _conn, _trans);
                            await StartWorkflowAfterGenerateCode(item.ModuleCode, item.ItemID.Value, item.ID, item.ListName, _conn, _trans).ConfigureAwait(false);
                            _trans.Commit();
                            count++;
                        }
                        catch (Exception ex)
                        {
                            WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + " - " + ex.Message);
                            WriteToFile("   " + SiteUrl + " - " + item.ListName + " - " + item.ItemID.Value);
                            WriteToFile("   " + item.TableName + " - " + item.DetailName + " - " + item.TransID);
                            await AutoCodeUpdateFlag(item.ID, ex.Message, 2, _conn, _trans).ConfigureAwait(false);
                        }
                    }
                }
            }
            return $"{count} of {total}";
        }

        public static string GenerateCode(string SiteUrl)
        {
            var list = GetListData();
            var total = list.Count;
            var count = 0;
            foreach (var item in list)
            {
                var isError = false;
                var errorMessage = "";

                SPSite spSite = null;
                SPListItem spItem = null;

                var isTrans = true;
                try
                {
                    db.OpenConnection(ref conn, isTrans);
                    #region GenerateCode
                    db.cmd.CommandText = "usp_Utility_AutoCounter";
                    db.cmd.CommandType = CommandType.StoredProcedure;

                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "FieldName", item.ColumnName);
                    db.AddInParameter(db.cmd, "TableName", item.TableName);
                    db.AddInParameter(db.cmd, "FieldCriteria", item.ColumnName);
                    db.AddInParameter(db.cmd, "ValueCriteria", item.Format);
                    db.AddInParameter(db.cmd, "LengthOfString", item.LengthOfString);

                    reader = db.cmd.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(reader);
                    db.CloseDataReader(reader);

                    string autoCode = Utility.ConvertDataTableToList<GeneratedCode>(dt)[0].AutoCode;
                    #endregion

                    #region Update AutoCodeBatch
                    db.cmd.CommandText = "usp_AutoCodeBatch_UpdateFlag";
                    db.cmd.CommandType = CommandType.StoredProcedure;

                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "ID", item.ID);
                    db.AddInParameter(db.cmd, "Generated", 1);
                    db.AddInParameter(db.cmd, "SysMessage", "Generated");

                    db.cmd.ExecuteNonQuery();
                    #endregion

                    #region Update Item
                    db.cmd.CommandText = "usp_NWC_AutoCodeBatch_UpdateItem"; //prod
                    db.cmd.CommandType = CommandType.StoredProcedure;

                    db.cmd.Parameters.Clear();
                    db.AddInParameter(db.cmd, "TransID", item.TransID);
                    db.AddInParameter(db.cmd, "TableName", item.TableName);
                    db.AddInParameter(db.cmd, "DetailName", item.DetailName);
                    db.AddInParameter(db.cmd, "ColumnName", item.ColumnName);
                    db.AddInParameter(db.cmd, "AutoCode", autoCode);

                    db.cmd.ExecuteNonQuery();
                    #endregion

                    db.CloseConnection(ref conn, isTrans);


                    #region Update ListItem
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                    {
                        spSite = new SPSite(SiteUrl);
                        SPWeb spWeb = spSite.OpenWeb();
                        SPList spList = spWeb.Lists.TryGetList(item.ListName);

                        spWeb.AllowUnsafeUpdates = true;
                        spItem = spList.GetItemById(item.ItemID.Value);
                        spItem["Title"] = autoCode;
                        spItem["Request No"] = autoCode;
                        spItem["Approval Status"] = "Generated";
                        spItem["Approval Status ID"] = 3;
                        spItem["Workflow Status"] = "Approval";
                        spItem["Form Status"] = "Start";
                        spItem.Update();
                        spWeb.AllowUnsafeUpdates = false;
                    });
                    Console.WriteLine(autoCode);
                    #endregion

                    #region InsertHistoryLog
                    SPWeb web = new SPSite(Utility.SpSiteUrl_DEV).OpenWeb();
                    SPList listData = web.Lists[item.ListName];
                    web.AllowUnsafeUpdates = true;

                    SPListItem listItem = listData.GetItemById(item.ItemID.Value);

                    string CurrentLogin = listItem["Author"].ToString();
                    string CurrentLoginName = listItem["Requester_x0020_Name"].ToString();


                    HashSet<string> ModuleHL = new HashSet<string> { "M001", "M016", "M002", "M003", "M004", "M012", "M025" };

                    #region Untuk module selain Purchase Request
                    if (!ModuleHL.Contains(item.ModuleCode)) // Selain Module PR dan Claim Reimburse, karena untuk module PR dan modul2 non comm yg lain insert history log ada di web service save update
                    {
                        try
                        {
                            db.OpenConnection(ref conn);
                            db.cmd.CommandText = "[dbo].[usp_NWC_insertHistoryLog]";
                            db.cmd.CommandType = CommandType.StoredProcedure;
                            db.cmd.Parameters.Clear();

                            db.AddInParameter(db.cmd, "listName", item.ListName);
                            db.AddInParameter(db.cmd, "listItemID", item.ItemID.Value);
                            db.AddInParameter(db.cmd, "action", 1);
                            db.AddInParameter(db.cmd, "currentLogin", CurrentLogin);
                            db.AddInParameter(db.cmd, "currentLoginName", CurrentLoginName);
                            db.AddInParameter(db.cmd, "currentLayer", "0");

                            db.cmd.ExecuteNonQuery();
                            db.CloseConnection(ref conn);
                        }

                        catch (Exception ex)
                        {
                            db.CloseConnection(ref conn);
                            throw ex;
                        }
                    }
                    #endregion
                    #endregion

                    #region Start Workflow
                    StartWorkflowAfterGenerateCode(item.ModuleCode, (int)item.ItemID, (int)item.TransID, item.ListName);

                    #endregion


                    count++;
                }
                catch (Exception ex)
                {
                    isTrans = false;
                    WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + " - " + ex.Message);
                    WriteToFile("   " + SiteUrl + " - " + item.ListName + " - " + item.ItemID.Value);
                    WriteToFile("   " + item.TableName + " - " + item.DetailName + " - " + item.TransID);

                    isError = true;
                    errorMessage = ex.ToString();
                }
                finally
                {
                    db.CloseConnection(ref conn, isTrans);
                }

                if (isError)
                {
                    isTrans = true;
                    db.OpenConnection(ref conn, isTrans);
                    try
                    {
                        #region Update AutoCodeBatch if error
                        db.cmd.CommandText = "usp_AutoCodeBatch_UpdateFlag";
                        db.cmd.CommandType = CommandType.StoredProcedure;

                        db.cmd.Parameters.Clear();
                        db.AddInParameter(db.cmd, "ID", item.ID);
                        db.AddInParameter(db.cmd, "Generated", 2);
                        db.AddInParameter(db.cmd, "SysMessage", errorMessage);

                        db.cmd.ExecuteNonQuery();
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        isTrans = false;
                        WriteToFile(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - " + ex.Message);
                    }
                    finally
                    {
                        db.CloseConnection(ref conn, isTrans);
                    }
                }
                else
                {
                    #region Start Workflow
                    NintexWorkflowCloud nwc = ntxManager.GenerateNACPayload((int)item.TransID, (int)item.ItemID, item.ModuleCode, item.ListName);
                    ntxManager.StartNWC(nwc).GetAwaiter().GetResult();
                    #endregion
                }

            }

            return count + " of " + total;
        }

        public static void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\SchedulerLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public async static void StartWorkflowBySystemAccount(NintexWorkflowCloud nwc)
        {
            try
            {
                string sBody = new JavaScriptSerializer().Serialize(nwc.param);

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(nwc.url);

                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT Header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.url);
                request.Content = new StringContent(sBody, Encoding.UTF8, "application/json");

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {

            string sBody = new JavaScriptSerializer().Serialize(nwc.param);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(nwc.url);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT Header

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.url);

            request.Content = new StringContent(sBody, Encoding.UTF8, "application/json");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
            }

        }

        public void InsertApprovalLog(string listName, int listItemID, int action, string CurrentLogin, string CurrentLoginName, string currentLayer)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_NWC_insertHistoryLog]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "listName", listName);
                db.AddInParameter(db.cmd, "listItemID", listItemID);
                db.AddInParameter(db.cmd, "action", action);
                db.AddInParameter(db.cmd, "currentLogin", CurrentLogin);
                db.AddInParameter(db.cmd, "currentLoginName", CurrentLoginName);
                db.AddInParameter(db.cmd, "currentLayer", currentLayer);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }

            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

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

    }
}
