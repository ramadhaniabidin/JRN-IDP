using Daikin.BusinessLogics.Apps.Commercials.Controller;
using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Batch.Controller
{
    public class BatchController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        private readonly SalesForceController sfc = new SalesForceController();
        private readonly Utility ut = new Utility();
        private readonly string PATH_LOCATION_KEY = "Path_Location";
        private readonly string MODULE_CODE_KEY = "Module_Code";
        private readonly string HEADER_ID_KEY = "Header_ID";
        private readonly string BRANCH_CODE_KEY = "BranchCode";

        public string GetReportBase64(string Report_ID, string Extension)
        {
            string endpoint = ConfigurationManager.AppSettings["SF_BaseURL"] + $"/services/data/v60.0/analytics/reports/{Report_ID}?export=1&encod=UTF-8&xf={Extension}";
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sfc.GetAttachmentToken());
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Content = new StringContent("");
            var response = client.GetAsync(endpoint).Result;
            byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
            return Convert.ToBase64String(fileBytes);
        }

        public async Task<string> GetReportBase64Async(string reportId, string extension)
        {
            string baseUrl = ConfigurationManager.AppSettings["SF_BaseURL"];
            string endpoint = $"{baseUrl}/services/data/v60.0/analytics/reports/{reportId}?export=1&encod=UTF-8&xf={extension}";

            using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sfc.GetAttachmentToken());
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                try
                {
                    using (var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string errorBody = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException($"Request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {errorBody}");
                        }
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var ms = new MemoryStream())
                        {
                            await stream.CopyToAsync(ms);
                            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw new TimeoutException("The request timed out after 5 minutes.", ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"HTTP request error: {ex.Message}", ex);
                }
            }
        }


        public void UploadReportToSharedFolder(string FolderPath, string FileName, string Extension, string Base64)
        {
            var credential = new Utility().GetNetworkCredential();
            using (new ConnectToSharedFolder(FolderPath, credential))
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                byte[] fileBytes = Convert.FromBase64String(Base64);
                string filePath = Path.Combine(FolderPath, $"{FileName}.{Extension}");
                File.WriteAllBytes(filePath, fileBytes);
            }
        }


        public void CreateReportFile(string Folder_ID)
        {
            dt = new DataTable();
            dt = GetFolderLocation(Folder_ID);
            foreach(DataRow row in dt.Rows)
            {
                string folderPath = Utility.GetStringValue(row, PATH_LOCATION_KEY);
                List<ReportModel> reports = GetReportAttribute();
                foreach(var report in reports)
                {
                    UploadReportToSharedFolder(folderPath, report.Report_Name, report.Extension, GetReportBase64(report.Report_ID, report.Extension));
                }
            }
        }

        public void CreateReportFromBase64(string Report_Name, string Report_ID, string Extension)
        {
            string Report_Path = ut.GetConfigValue("SF_ReportPath");
            string base64 = GetReportBase64(Report_ID, Extension);
            UploadReportToSharedFolder(Report_Path, Report_Name, Extension, base64);
        }

        public async Task CreateReportFromBase64Async(string Report_Name, string Report_ID, string Extension)
        {
            string Report_Path = ut.GetConfigValue("SF_ReportPath");
            string base64 = await GetReportBase64Async(Report_ID, Extension);
            UploadReportToSharedFolder(Report_Path, Report_Name, Extension, base64);
        }

        public List<ReportModel> GetReportAttribute()
        {
            using (var con = new SqlConnection(Utility.GetSQLConnDev()))
            {
                con.Open();
                string query = "SELECT * FROM [Report_Configuration] WHERE [Active] = @active";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "@active", Value = 1, SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input });
                    using (var _reader = cmd.ExecuteReader())
                    {
                        dt = new DataTable();
                        dt.Load(_reader);
                        return Utility.ConvertDataTableToList<ReportModel>(dt);
                    }
                }
            }
        }

        public void UpdatePOReleaseDetail(int HeaderID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_UpdatePOReleaseDetail_AfterReadSAP";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, HEADER_ID_KEY, HeaderID);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch(Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<BatchModel> GetBatchFileContentsSC(string moduleCode, int headerID, int No)
        {
            DataTable dtBatch = new DataTable();
            using (SqlConnection _conn = new SqlConnection(db.GetSQLConnectionString()))
            {
                _conn.Open();
                using (SqlCommand command = _conn.CreateCommand())
                {
                    command.CommandText = "usp_Utility_CreateBatchFile";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue(MODULE_CODE_KEY, moduleCode);
                    command.Parameters.AddWithValue(HEADER_ID_KEY, headerID);
                    command.Parameters.AddWithValue("No", No);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        dtBatch.Load(dr);
                    }
                    return Utility.ConvertDataTableToList<BatchModel>(dtBatch);
                }
            }
        }

        public void CreateBatchFileSC(int No, string moduleCode, int headerID,
            string basePath, string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(basePath))
                basePath = new Utility().GetConfigValue("NetworkPath");
            try
            {

                var list = GetBatchFileContentsSC(moduleCode, headerID, No);
                if (list.Count > 0)
                {
                    var credentials = new Utility().GetNetworkCredential();
                    using (new ConnectToSharedFolder(basePath, credentials))
                    {
                        var formNo = list[0].BatchFile.Split('\t', ';')[0].Substring(0, 10);
                        string targetPath = basePath + filePath;
                        var targetFile = System.IO.Path.Combine(targetPath, fileName + ".txt");
                        new BatchController().SaveBatchFileHistory(moduleCode, headerID, formNo, targetFile, false);

                        #region Create Batch File
                        System.IO.Directory.CreateDirectory(targetPath);
                        using (System.IO.TextWriter tw = new System.IO.StreamWriter(targetFile))
                            foreach (var row in list)
                                tw.WriteLine(row.BatchFile);
                        #endregion
                    }
                }
            }
            finally
            {
                db.CloseConnection(ref conn);
            }

        }



        public string CreateBatchFile(string moduleCode, int headerID, string basePath, string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(basePath))
                basePath = new Utility().GetConfigValue("NetworkPath");

            var isTrans = true;
            var targetFile = "";
            try
            {
                db.OpenConnection(ref conn, isTrans);

                var list = GetBatchFileContents(moduleCode, headerID, true);
                if (list.Count > 0)
                {
                    var credentials = new Utility().GetNetworkCredential();
                    using (new ConnectToSharedFolder(basePath, credentials))
                    {
                        var formNo = list[0].BatchFile.Split('\t', ';')[0];
                        var targetPath = basePath + filePath;
                        targetFile = $"{targetPath}\\{fileName}.txt";

                        SaveBatchFileHistory(moduleCode, headerID, formNo, targetFile, true);

                        #region Create Batch File
                        Directory.CreateDirectory(targetPath);
                        using (TextWriter tw = new StreamWriter(targetFile))
                            foreach (var row in list)
                                tw.WriteLine(row.BatchFile);
                        #endregion
                    }
                }
                return targetFile;
            }
            catch (Exception)
            {
                isTrans = false;
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn, isTrans);
            }

        }

        public void GenerateTxtFile(string Message, string SAPFolderID, string FileName)
        {
            dt = new BatchController().GetFolderLocation(SAPFolderID);
            foreach (DataRow row in dt.Rows)
            {
                string folder = Utility.GetStringValue(row, PATH_LOCATION_KEY);
                string filepath = folder + @"\" + FileName + ".txt";
                if (!File.Exists(filepath))
                {
                    var credentials = new Utility().GetNetworkCredential();
                    using (new ConnectToSharedFolder(folder, credentials))
                    {
                        using (StreamWriter sw = File.CreateText(filepath))
                        {
                            sw.WriteLine(Message);
                        }
                    }
                }
            }
        }


        public DataTable GetFolderLocation(string ID)
        {
            try
            {
                dt = new DataTable();
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
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
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
                db.AddInParameter(db.cmd, HEADER_ID_KEY, headerID);
                reader = db.cmd.ExecuteReader();
                dtx.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return dtx;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void CreateBatchFileDynamic(string SP_Name, string SAPFolderID, int headerID, string fileName)
        {
            var isTrans = true;

            try
            {
                dt = new DataTable();
                dt = GetFolderLocation(SAPFolderID);

                foreach (DataRow r in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(r, MODULE_CODE_KEY);
                    string PathLocation = Utility.GetStringValue(r, PATH_LOCATION_KEY);
                    DataTable dtInfo = GetProcBranch(SAPFolderID, headerID);

                    //For List Non Commercials Only
                    foreach (DataRow row in dtInfo.Rows)
                    {
                        if (!string.IsNullOrEmpty(Utility.GetStringValue(row, BRANCH_CODE_KEY)))
                        {
                            string branchCode = Utility.GetStringValue(row, BRANCH_CODE_KEY);
                            string procDept = Utility.GetStringValue(row, "ProcDept");
                            PathLocation = Path.Combine(PathLocation, branchCode, procDept);
                        }
                    }
                    //---------------------------------------



                    db.OpenConnection(ref conn, isTrans);

                    var list = GetBatchFileContents(SP_Name, moduleCode, headerID, true);
                    if (list.Count > 0)
                    {
                        var credentials = new Utility().GetNetworkCredential();
                        using (new ConnectToSharedFolder(PathLocation, credentials))
                        {
                            var formNo = list[0].BatchFile.Split('\t', ';')[0];
                            var targetPath = PathLocation;
                            var targetFile = Path.Combine(targetPath, fileName + ".txt");

                            SaveBatchFileHistory(moduleCode, headerID, formNo, targetFile, true);

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
            catch (Exception)
            {
                isTrans = false;
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn, isTrans);
            }
        }

        //Create Batch File New
        public void CreateBatchFile(string SAPFolderID, int headerID, string fileName)
        {

            var isTrans = true;
            try
            {
                dt = new DataTable();
                dt = GetFolderLocation(SAPFolderID);

                foreach (DataRow r in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(r, MODULE_CODE_KEY);
                    string PathLocation = Utility.GetStringValue(r, "Path_Location");
                    DataTable dtInfo = GetProcBranch(SAPFolderID, headerID);

                    //For List Non Commercials Only
                    foreach (DataRow row in dtInfo.Rows)
                    {
                        if (!string.IsNullOrEmpty(Utility.GetStringValue(row, BRANCH_CODE_KEY)))
                        {
                            string branchCode = Utility.GetStringValue(row, BRANCH_CODE_KEY);
                            string procDept = Utility.GetStringValue(row, "ProcDept");
                            PathLocation = Path.Combine(PathLocation, branchCode, procDept);
                        }
                    }
                    //---------------------------------------



                    db.OpenConnection(ref conn, isTrans);

                    var list = GetBatchFileContents(moduleCode, headerID, true);
                    if (list.Count > 0)
                    {
                        var credentials = new Utility().GetNetworkCredential();
                        using (new ConnectToSharedFolder(PathLocation, credentials))
                        {
                            var formNo = list[0].BatchFile.Split('\t', ';')[0];
                            var targetPath = PathLocation;
                            var targetFile = Path.Combine(targetPath, fileName + ".txt");
                            SaveBatchFileHistory_V2(moduleCode, headerID, formNo, targetFile);

                            #region Create Batch File
                            Directory.CreateDirectory(targetPath);
                            using (TextWriter tw = new StreamWriter(targetFile))
                                foreach (var row in list)
                                    tw.WriteLine(row.BatchFile);
                            #endregion
                        }
                    }
                    db.CloseConnection(ref conn);
                }
            }
            catch (Exception)
            {
                isTrans = false;
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn, isTrans);
            }

        }

        //Create Batch File With No = affiliate not claim ada jurnal 1 dan 2
        public void CreateBatchFileWithNo(string SAPFolderID, int headerID, string fileName, int no)
        {

            var isTrans = true;
            try
            {
                dt = new DataTable();
                dt = GetFolderLocation(SAPFolderID);

                foreach (DataRow r in dt.Rows)
                {
                    string moduleCode = Utility.GetStringValue(r, MODULE_CODE_KEY);
                    string PathLocation = Utility.GetStringValue(r, PATH_LOCATION_KEY);
                    DataTable dtInfo = GetProcBranch(SAPFolderID, headerID);

                    //For List Non Commercials Only
                    foreach (DataRow row in dtInfo.Rows)
                    {
                        if (!string.IsNullOrEmpty(Utility.GetStringValue(row, BRANCH_CODE_KEY)))
                        {
                            string branchCode = Utility.GetStringValue(row, BRANCH_CODE_KEY);
                            string procDept = Utility.GetStringValue(row, "ProcDept");
                            PathLocation = Path.Combine(PathLocation, branchCode, procDept);
                        }
                    }
                    //---------------------------------------



                    db.OpenConnection(ref conn, isTrans);

                    var list = GetBatchFileContentsWithNo(moduleCode, headerID, no, true);
                    if (list.Count > 0)
                    {
                        var credentials = new Utility().GetNetworkCredential();
                        using (new ConnectToSharedFolder(PathLocation, credentials))
                        {
                            var formNo = list[0].BatchFile.Split('\t', ';')[0];
                            var targetPath = PathLocation;
                            var targetFile = Path.Combine(targetPath, fileName + ".txt");

                            SaveBatchFileHistory(moduleCode, headerID, formNo, targetFile, true);

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
            catch (Exception)
            {
                isTrans = false;
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn, isTrans);
            }

        }

        public List<BatchModel> GetBatchFileContents(string SP_Name, string moduleCode, int headerID, bool isOpen = false)
        {
            dt = new DataTable();
            try
            {
                if (!isOpen)
                    db.OpenConnection(ref conn);

                db.cmd.CommandText = SP_Name;
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CODE_KEY, moduleCode);
                db.AddInParameter(db.cmd, HEADER_ID_KEY, headerID);

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

        public List<BatchModel> GetBatchFileContents(string moduleCode, int headerID, bool isOpen = false)
        {
            dt = new DataTable();
            try
            {
                if (!isOpen)
                    db.OpenConnection(ref conn);

                db.cmd.CommandText = "SAP.[usp_Utility_CreateBatchFile]";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CODE_KEY, moduleCode);
                db.AddInParameter(db.cmd, HEADER_ID_KEY, headerID);

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


        public List<BatchModel> GetBatchFileContentsWithNo(string moduleCode, int headerID, int no, bool isOpen = false)
        {
            dt = new DataTable();
            try
            {
                if (!isOpen)
                    db.OpenConnection(ref conn);

                db.cmd.CommandText = "SAP.[usp_Utility_CreateBatchFile]";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CODE_KEY, moduleCode);
                db.AddInParameter(db.cmd, HEADER_ID_KEY, headerID);
                db.AddInParameter(db.cmd, "No", no);

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

        public void SaveBatchFileHistory_V2(string moduleCode, int headerID, string formNo, string targetFile)
        {
            string connString = Utility.GetSqlConnection();
            using(SqlConnection _conn = new SqlConnection(connString))
            {
                _conn.Open();
                using(SqlCommand cmd = new SqlCommand("usp_BatchFileHistory_Save", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Module_Code", moduleCode);
                    cmd.Parameters.AddWithValue("@Header_ID", headerID);
                    cmd.Parameters.AddWithValue("@Form_No", formNo);
                    cmd.Parameters.AddWithValue("@Generated_File_Path", targetFile);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SaveBatchFileHistory(string moduleCode, int headerID, string formNo, string targetFile, bool isOpen = false)
        {
            var isTrans = true;
            try
            {
                if (!isOpen)
                    db.OpenConnection(ref conn, isTrans);
                db.cmd.CommandText = "usp_BatchFileHistory_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, MODULE_CODE_KEY, moduleCode);
                db.AddInParameter(db.cmd, HEADER_ID_KEY, headerID);
                db.AddInParameter(db.cmd, "Form_No", formNo);
                db.AddInParameter(db.cmd, "Generated_File_Path", targetFile);

                db.cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                isTrans = false;
                throw;
            }
            finally
            {
                if (!isOpen)
                    db.CloseConnection(ref conn, isTrans);
            }
        }
    }
}
