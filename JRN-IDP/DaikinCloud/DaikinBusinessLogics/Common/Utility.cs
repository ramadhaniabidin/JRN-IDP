using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common
{
    public class Utility
    {
        public const string LevelAccessMessage = "You don't have permission to trigger this action";
        public const string SpSiteUrl = "https://sp3.daikin.co.id:3473/";
        public const string Old_SpSiteUrl = "https://sp3.daikin.co.id:8443/";
        public const string SpSiteUrl_DEV = "http://spdev:3473/";
        private const string ENCRYPTION_KEY = "G21Express";
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();

        #region Encryption/decryption
        /// <summary>
        /// The salt value used to strengthen the encryption.
        /// </summary>
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes("DaikinNintex");

        /// <summary>
        /// Encrypts any string using the Rijndael algorithm.
        /// </summary>
        /// <param name="inputText">The string to encrypt.</param>
        /// <returns>A Base64 encrypted string.</returns>
        public static string Encrypt(string inputText)
        {
            using (var aes = Aes.Create())
            {
                var keyGen = new Rfc2898DeriveBytes(ENCRYPTION_KEY, Salt, 10000);

                aes.Key = keyGen.GetBytes(32);
                aes.IV = keyGen.GetBytes(16);

                using (var memory = new MemoryStream())
                using (var crypto = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var writer = new StreamWriter(crypto, Encoding.Unicode))
                {
                    writer.Write(inputText);
                    writer.Flush();
                    crypto.FlushFinalBlock();
                    return Convert.ToBase64String(memory.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts a previously encrypted string.
        /// </summary>
        /// <param name="inputText">The encrypted string to decrypt.</param>
        /// <returns>A decrypted string.</returns>
        public static string Decrypt(string inputText)
        {
            byte[] encryptedText = Convert.FromBase64String(inputText);
            using (var aes = Aes.Create())
            {
                var keyGenerator = new Rfc2898DeriveBytes(ENCRYPTION_KEY, Salt, 10000);
                aes.Key = keyGenerator.GetBytes(32);
                aes.IV = keyGenerator.GetBytes(16);
                using (var memoryStram = new MemoryStream(encryptedText))
                {
                    using (var cryptoStream = new CryptoStream(memoryStram, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cryptoStream, Encoding.Unicode))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion

        public List<string> ListRemarks(SPWeb web, string Module_Name)
        {
            SPList list = web.Lists["Master Finance Check"];
            var q = new SPQuery { Query = @"<Where><Eq><FieldRef Name='Module' /><Value Type='Text'>" + Module_Name + "</Value></Eq></Where>" };
            List<string> listRem = new List<string>();

            var r = list.GetItems(q);
            foreach (SPListItem i in r)
            {
                listRem.Add(i["Title"].ToString());
            }
            return listRem;
        }

        public string CutOffInputSystem(string ModuleCode)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlDataReader reader = null;
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_CutOffInputSystem_ByModuleCode";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Module_Code", ModuleCode);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dt.Rows)
                {
                    int TotalDays = Utility.GetIntValue(row, "TotalDays");
                    if (TotalDays >= 0)
                    {
                        string val_message = Utility.GetStringValue(row, "Val_Message");
                        return val_message;
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }

        }

        public CommonResponseModel UpdateDocumentReceived(string ID, string Module_ID)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_Utility_UpdateDocumentReceived";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ID", ID);
                db.AddInParameter(db.cmd, "Module_ID", Module_ID);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
                return new CommonResponseModel
                {
                    Success = true, Message = "OK"
                };
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                return new CommonResponseModel
                {
                    Success = false,
                    Message = $"Error occurred at UpdateDocumentReceived method in Utility | {ex.Message}"
                };
            }
        }

        public string GetBranchCurrentLogin(string CurrentLogin)
        {
            string Branch = string.Empty;
            try
            {
                db.OpenConnection(ref conn);
                Branch = db.GetValueFromQuery("exec [dbo].[usp_FinancePayment_GetMappingBranchByCurrentLogin] '" + CurrentLogin + "'", "Branch");
                db.CloseConnection(ref conn);
                return Branch;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public List<Model.ApproverRoleModel> GetCommercialsPendingRoles()
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_Commercials_PendingApproverRoles]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dtBranch = new DataTable();
                dtBranch.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<Model.ApproverRoleModel>(dtBranch);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;

            }

        }

        public string GetPendingApprover(string listName, int itemID)
        {
            string Approver = string.Empty;
            try
            {
                db.OpenConnection(ref conn);
                Approver = db.GetValueFromQuery("exec [dbo].[usp_NWC_getPendingApprover] '" + listName + "', " + itemID, "Pending_Approver_Name");
                db.CloseConnection(ref conn);
                return Approver;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        public void GetPendingApproverReplace(string listName, int itemID, string userName)
        {
            db.CloseConnection(ref conn);
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.usp_NWC_GetApproverReplace";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "listName", listName);
                db.AddInParameter(db.cmd, "itemID", itemID);
                db.AddInParameter(db.cmd, "userName", userName);

                SqlDataReader reader = db.cmd.ExecuteReader();
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }
        



        public List<string> GetMappingBranchCurrentLogin(string CurrentLogin)
        {
            List<string> listBranch = new List<string>();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "[dbo].[usp_FinancePayment_GetMappingBranchByCurrentLogin]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PIC_Account", CurrentLogin);
                SqlDataReader reader = db.cmd.ExecuteReader();
                DataTable dtBranch = new DataTable();
                dtBranch.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                foreach (DataRow row in dtBranch.Rows)
                {
                    listBranch.Add(Utility.GetStringValue(row, "Branch"));
                }
                return listBranch;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

        #region DB
        public static string GetStringValue(DataRow value, string key)
        {
            string result = string.Empty;
            try
            {
                return value[key].ToString();
            }
            catch
            {
                return result;
            }
        }

        public static bool GetBoolValue(DataRow value, string key)
        {
            bool result = false;
            try
            {
                string val = value[key].ToString();
                return Boolean.Parse(val);
            }
            catch
            {
                return result;
            }
        }

        public static int GetIntValue(DataRow value, string key)
        {
            try
            {
                string val = value[key].ToString().Split('.')[0];
                return int.Parse(val);
            }
            catch
            {
                return 0;
            }
        }

        public static decimal GetDecimalValue(DataRow value, string key)
        {
            try
            {
                string val = value[key].ToString();
                return Convert.ToDecimal(val);
            }
            catch
            {
                return 0;
            }
        }

        public static DateTime GetDateValue(DataRow value, string key)
        {
            DateTime date = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            try
            {
                return DateTime.Parse(value[key].ToString());
            }
            catch
            {
                return date;
            }
        }

        public string GetUntilOrEmpty(string text, string stopAt = "", string orStopAt = "")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);
                int charLocation2 = text.IndexOf(orStopAt, StringComparison.Ordinal);


                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static DataTable ToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static List<T> ConvertDataTableToList<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        var value = dr[column.ColumnName];
                        if (!Convert.IsDBNull(value))
                            pro.SetValue(obj, value, null);
                        break;
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        public static string ToHtmlTable(DataTable dt)
        {
            string strHtml = "<table><tr>" + Environment.NewLine;
            foreach (DataColumn col in dt.Columns)
            {
                strHtml += Environment.NewLine + "<th>" + col.ColumnName + "</th>";
            }
            strHtml += Environment.NewLine + "</tr>";
            foreach (DataColumn dc in dt.Columns)
            {
                strHtml += Environment.NewLine + "<tr>";
                foreach (DataRow row in dt.Rows)
                {
                    strHtml += Environment.NewLine + "<td>" + row[dc] + "</td>";
                }
                strHtml += Environment.NewLine + "</tr>";
            }
            strHtml += "</table>";
            return strHtml;
        }

        public static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        #endregion

        #region Log

        public static void SaveLog(string Process_Name, string Nintex_No, string Source_File, string Sys_Message, int Status)
        {
            DatabaseManager db = new DatabaseManager();
            SqlConnection conn = new SqlConnection();
            db.OpenConnection(ref conn);
            db.cmd.CommandText = "usp_BatchProcessLog_Save";
            db.cmd.CommandType = CommandType.StoredProcedure;
            db.cmd.Parameters.Clear();

            db.AddInParameter(db.cmd, "Process_Name", Process_Name);
            db.AddInParameter(db.cmd, "Nintex_No", Nintex_No);
            db.AddInParameter(db.cmd, "Source_File", Source_File);
            db.AddInParameter(db.cmd, "Sys_Message", Sys_Message);
            db.AddInParameter(db.cmd, "Status", Status);

            db.cmd.ExecuteNonQuery();

            db.CloseConnection(ref conn);
        }

        public static void WriteToFile(string Message, bool timestampPrefix = true)
        {
            if (timestampPrefix)
                Message = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss tt") + " - " + Message;
            else
                Message = "   " + Message;

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
        #endregion

        #region Get App.Config
        public string GetConfigValue(string key)
        {
            var result = "";

            try
            {
                result = ConfigurationManager.AppSettings[key].ToString();
            }
            catch
            {
                //pass
            }

            #region commented-out codoe
            if (string.IsNullOrEmpty(result))
                switch (key)
                {
                    case "SiteUrl":
                        return "https://sp3.daikin.co.id:8443";
                    case "NetworkUser":
                        return @"daikin\nintex2021";
                    case "NetworkPass":
                        return "Mq151k.$rqUd";
                    case "NetworkPath":
                        return @"\\dbs\Nintex";
                    case "SystemUser":
                        return @"daikin\sp_farm";
                    case "TransList":
                        return @"Claim Rembursement";
                    case "TransListWorkflow":
                        return @"Claim Rembursement Workflow";
                    default:
                        break;
                }
            #endregion
            return result;
        }

        public NetworkCredential GetNetworkCredential()
        {
            var user = new Utility().GetConfigValue("NetworkUser");
            var pass = new Utility().GetConfigValue("NetworkPass");

            var cred = new NetworkCredential(user, pass);
            return cred;
        }

        public static string GetSqlConnection()
        {
            return ConfigurationManager.ConnectionStrings["cnstr"].ToString();
        }
        #endregion

        public IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {

        #region Big freaking list of mime types
        // combination of values from Windows 7 Registry and 
        // from C:\Windows\System32\inetsrv\config\applicationHost.config
        // some added, including .7z and .dat
        {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/octet-stream"},
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        #endregion

        };

        public string GetMimeType(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string mime;

            return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
        }

        public string ConvertDatatableToXML(DataTable dt)
        {
            MemoryStream str = new MemoryStream();
            dt.WriteXml(str, true);
            str.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(str);
            string xmlstr;
            xmlstr = sr.ReadToEnd();
            return (xmlstr);
        }

        public static string ConvertValueToString(SPListItem listItem, string columnName, bool isValueNull)
        {
            return isValueNull ? "NULL " : $"'{listItem[columnName]}'";
        }

        public static string ConvertValueToNumber(SPListItem listItem, string columnName, bool isValueNull)
        {
            return isValueNull ? "NULL " : $"CAST('{listItem[columnName]}' AS NUMERIC)";
        }

        public static string ConvertValueToDateTime(SPListItem listItem, string columnName, bool isValueNull)
        {
            DateTime dateTime = isValueNull ? DateTime.Parse("1900-01-01") : (DateTime)listItem[columnName];
            string formatDateTime = "yyyy-MM-dd HH:mm:ss";
            return $"'{dateTime.ToString(formatDateTime)}'";
        }

        public static string ConvertValueToBit(SPListItem listItem, string columnName)
        {
            return listItem[columnName].ToString().ToUpperInvariant() == "TRUE" ? "1" : "0";
        }

        public static string ConvertValueToLookup(SPListItem listItem, string columnName)
        {
            var splits = listItem[columnName]?.ToString().Split('#');
            return splits?.Length > 1 ? $"'{splits[1]}'" : "NULL";
        }

        static public string getColumnValue(int SPColumnType, DataRow row, SPListItem listItem)
        {
            string val = "";
            string columnName = Utility.GetStringValue(row, "Sharepoint_Column_Name");
            bool isValueNull = listItem[columnName] == null;

            #region Values
            if (SPColumnType == 1 || SPColumnType == 2 || SPColumnType == 9) //Single or Multiple Line of Text or Choice
            {
                val = ConvertValueToString(listItem, columnName, isValueNull);
            }
            else if (SPColumnType == 3) //Number
            {
                val = ConvertValueToNumber(listItem, columnName, isValueNull);
            }
            else if (SPColumnType == 4) //datetime
            {
                val = ConvertValueToDateTime(listItem, columnName, isValueNull);
            }
            else if (SPColumnType == 6) //Yes or No
            {
                val = ConvertValueToBit(listItem, columnName);
            }
            else if (SPColumnType == 7 || SPColumnType == 5) //Person or Group || Lookup
            {
                val = ConvertValueToLookup(listItem, columnName);
            }
            #endregion

            return val;
        }

        public static SPUser GetSPUser(SPListItem item, string key)
        {
            SPFieldUser field = item.Fields[key] as SPFieldUser;

            if (field != null)
            {
                SPFieldUserValue fieldValue = field.GetFieldValue(item[key].ToString()) as SPFieldUserValue;

                if (fieldValue != null)
                    return fieldValue.User;
            }
            return null;
        }

        public static SPUser GetUserByFullName(string fullName, SPWeb web)
        {
            SPPrincipalInfo objInfo = SPUtility.ResolvePrincipal(web, fullName, SPPrincipalType.SecurityGroup | SPPrincipalType.User, SPPrincipalSource.All, null, false);
            if (objInfo == null)
                throw new SPException(SPResource.GetString("User can't be found", new object[] { objInfo.LoginName }));
            return web.SiteUsers[objInfo.LoginName];
        }

        public static string GetValueFromArray(string[] data, int index)
        {
            try
            {
                return data[index];
            }
            catch
            {
                return "";
            }
        }

    }
}