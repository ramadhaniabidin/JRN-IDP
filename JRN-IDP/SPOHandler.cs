using JRN_IDP.Model;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class SPOHandler
    {
        ProsnapHandler prosnap = new ProsnapHandler();
        private readonly string connString = ConfigurationManager.AppSettings["connString"];
        private readonly string connString_JRNAzure = ConfigurationManager.AppSettings["connString_JRNAzure"];
        //private readonly string connString = "Server=OCR-DEV;Database=IDP_JRN;User ID=sa;Password=Pa55word;Encrypt=False";
        public List<SPOFileModel> GetFileFromSPO()
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("SPOFile_GetList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return Utility.ConvertDataTableToList<SPOFileModel>(dt);
        }

        public string GenerateBase64Attachment(string fileUrl)
        {
            // SharePoint site URL and credentials
            //string siteName = "p2pdocumentation";
            //string fileRelativeUrl = fileUrl.ToLower().Replace("inv", "Supporting%20Doc");
            string fileRelativeUrl = fileUrl.ToLower().Replace("%20-%20inv", "");
            //Console.WriteLine($"File Relative url: {fileRelativeUrl}");            
            DecryptionModel decryption = Decrypt("SPO");
            string username = decryption.username_;
            string password = decryption.password_;
            string siteUrl = $"https://jresourcesid.sharepoint.com/sites/p2pdocumentation";
            try
            {
                SecureString secure = new SecureString();
                foreach (var c in password)
                {
                    secure.AppendChar(c);
                }
                using (var context = new ClientContext(siteUrl))
                {
                    context.Credentials = new SharePointOnlineCredentials(username, secure);
                    // Retrieve the file from the document library
                    var web = context.Web;
                    var file = web.GetFileByServerRelativeUrl(fileRelativeUrl);
                    context.Load(file);
                    context.ExecuteQuery();

                    if (file != null)
                    {
                        // Download the file's content
                        ClientResult<Stream> streamResult = file.OpenBinaryStream();
                        context.ExecuteQuery();

                        using (var memoryStream = new MemoryStream())
                        {
                            streamResult.Value.CopyTo(memoryStream);

                            // Convert the file content to Base64
                            byte[] fileBytes = memoryStream.ToArray();
                            string base64Content = Convert.ToBase64String(fileBytes);

                            //Console.WriteLine("Base64 Content:");
                            //Console.WriteLine(base64Content);
                            return base64Content;
                        }
                    }
                    else
                    {
                        Console.WriteLine("File not found!");
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        public string ConvertToBase64(SPOFileModel fileModel)
        {
            DecryptionModel decryption = Decrypt("SPO");
            string username = decryption.username_;
            string password = decryption.password_;
            string siteUrl = "https://jresourcesid.sharepoint.com/sites/p2pdocumentation";
            string baseURL = "https://jresourcesid.sharepoint.com";
            try
            {
                SecureString secure = new SecureString();
                foreach (var c in password)
                {
                    secure.AppendChar(c);
                }
                using (var context = new ClientContext(siteUrl))
                {
                    context.Credentials = new SharePointOnlineCredentials(username, secure);
                    // Retrieve the file from the document library
                    var web = context.Web;
                    var fileUrl = $"{fileModel.Document_Url.Replace(baseURL, "").Trim()}"; // Adjust if site structure differs
                    var file = web.GetFileByServerRelativeUrl(fileUrl);
                    context.Load(file);
                    context.ExecuteQuery();

                    if (file != null)
                    {
                        // Download the file's content
                        ClientResult<Stream> streamResult = file.OpenBinaryStream();
                        context.ExecuteQuery();

                        using (var memoryStream = new MemoryStream())
                        {
                            streamResult.Value.CopyTo(memoryStream);

                            // Convert the file content to Base64
                            byte[] fileBytes = memoryStream.ToArray();
                            string base64Content = Convert.ToBase64String(fileBytes);
                            return base64Content;
                        }
                    }
                    else
                    {
                        Console.WriteLine("File not found!");
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        public void UploadFileToProsnap()
        {
            List<SPOFileModel> files = GetFileFromSPO();
            foreach(SPOFileModel file in files)
            {
                string base64 = ConvertToBase64(file);
                prosnap.UploadFile(file, base64);
            }
        }

        public EncryptionModel GetEncryption()
        {
            DataTable dt = new DataTable();
            using(SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using(SqlCommand cmd = new SqlCommand("usp_Encryption", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<EncryptionModel>(dt)[0];
                    }
                }
            }

        }

        public void TestEncryption()
        {
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            string encryptedUsername = ConfigurationManager.AppSettings["username_"];
            string encryptedPassword = ConfigurationManager.AppSettings["password_"];

            string username = Utility.Decrypt(Convert.FromBase64String(encryptedUsername), key, iv);
            string password = Utility.Decrypt(Convert.FromBase64String(encryptedPassword), key, iv);

            Console.WriteLine($"Username encrypt: {encryptedUsername}");
            Console.WriteLine($"Password decrypt: {username}");
            Console.WriteLine($"Password encrypt: {encryptedPassword}");
            Console.WriteLine($"Password decrypt: {password}");
        }

        public void UpdateCredentials()
        {
            Console.Write("Enter your new username: ");
            string newUsername = Console.ReadLine();
            Console.Write("Enter your new password: ");
            string newPass = Console.ReadLine();
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            string newPassEncrypt = Convert.ToBase64String(Utility.Encrypt(newPass, key, iv));
            string newUsernameEncrypt = Convert.ToBase64String(Utility.Encrypt(newUsername, key, iv));
            UpdateCredentialToSQL(newPassEncrypt, newUsernameEncrypt);
            Console.WriteLine("Update Password success");
        }

        public void UpdateCredentialToSQL(string password, string username)
        {
            using(SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand("usp_UpdateCreds", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@NewPass", password);
                    cmd.Parameters.AddWithValue("@NewUsername", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void TestConnection_JRNAzure()
        {
            using(SqlConnection con = new SqlConnection(connString_JRNAzure))
            {
                DataTable dt = new DataTable();
                con.Open();
                string query = "[usp_SPOFile_GetList_TestConnection]";
                using(SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
                List<SPOFileModel> spoFiles = Utility.ConvertDataTableToList<SPOFileModel>(dt);
                foreach(var file in spoFiles)
                {
                    Console.WriteLine($"FileName: {file.Document_Name}");
                    Console.WriteLine($"FileURL: {file.Document_Url}");
                }
            }
        }

        public void EncryptCreds()
        {
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            Console.Write("Enter your username: ");
            string username = Console.ReadLine();
            Console.Write("Enter your password: ");
            string password = Console.ReadLine();
            byte[] usernameEncyrptBytes = Utility.Encrypt(username, key, iv);
            string usernameEncrypted = Convert.ToBase64String(usernameEncyrptBytes);
            byte[] passEncryptBytes = Utility.Encrypt(password, key, iv);
            string passwordEncrypted = Convert.ToBase64String(passEncryptBytes);
            Console.WriteLine($"Username encrypt: {usernameEncrypted}");
            Console.WriteLine($"Password encrypt: {passwordEncrypted}");
        }

        public DecryptionModel Decrypt(string type_)
        {
            DataTable dt = new DataTable();
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            using(SqlConnection con = new SqlConnection(connString))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand("usp_Creds_GetByType", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@type", type_);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            var decryption = Utility.ConvertDataTableToList<DecryptionModel>(dt)[0];
            string username = Utility.Decrypt(Convert.FromBase64String(decryption.username_), key, iv);
            string password = Utility.Decrypt(Convert.FromBase64String(decryption.password_), key, iv);
            decryption.username_ = username;
            decryption.password_ = password;
            return decryption;
        }
    }
}
