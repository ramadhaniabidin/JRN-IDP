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
        readonly ProsnapHandler prosnap = new ProsnapHandler();
        private readonly string connString = ConfigurationManager.AppSettings["connString"];
        private readonly string connString_JRNAzure = ConfigurationManager.AppSettings["connString_JRNAzure"];
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

        public SPOFileModel GetP2PDocument(int HeaderID)
        {
            DataTable dt = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                string query = $"SELECT List_Name, Document_Name, Document_Url FROM P2PDocuments WHERE ProSnap_FileID = @HeaderID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return Utility.ConvertDataTableToList<SPOFileModel>(dt)[0];
                    }
                }
            }
        }

        public static string Attachment_GetRelativeURL(string url, string fileName)
        {
            string cond1 = "-inv.pdf";
            string cond2 = " - inv.pdf";
            string cond3 = " -inv.pdf";
            string cond4 = "inv.pdf";
            string newFileName = "";
            if (fileName.ToLowerInvariant().Contains(cond1))
            {
                newFileName = fileName.ToLowerInvariant().Replace(cond1, "").Trim();
            }
            else if (fileName.ToLowerInvariant().Contains(cond2))
            {
                newFileName = fileName.ToLowerInvariant().Replace(cond2, "").Trim();
            }
            else if (fileName.ToLowerInvariant().Contains(cond3))
            {
                newFileName = fileName.ToLowerInvariant().Replace(cond3, "").Trim();
            }
            else if (fileName.ToLowerInvariant().Contains(cond4))
            {
                newFileName = fileName.ToLowerInvariant().Replace(cond4, "").Trim();
            }
            url = url.Replace("%20", " ");
            url = url.ToLowerInvariant().Replace(fileName.ToLowerInvariant(), newFileName);
            url += ".pdf".Trim();
            return url;
        }

        public void P2PDocuments_UpdateAttachmentURL(int HeaderID, string AttachmentURL)
        {
            using(var conn =  new SqlConnection(connString))
            {
                conn.Open();
                string query = "UPDATE P2PDocuments SET Attachment_URL = @AttachmentURL WHERE ProSnap_FileID = @HeaderID";
                using(var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@AttachmentURL", AttachmentURL);
                    cmd.Parameters.AddWithValue("@HeaderID", HeaderID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string GetAttachmentFileName(string invoiceFileName)
        {
            string cond1 = "-inv.pdf";
            string cond2 = " - inv.pdf";
            string cond3 = " -inv.pdf";
            string cond4 = "inv.pdf";
            string attachmentFileName = "";
            if (invoiceFileName.ToLowerInvariant().Contains(cond1))
            {
                attachmentFileName = invoiceFileName.ToLowerInvariant().Replace(cond1, "").Trim();
            }
            else if (invoiceFileName.ToLowerInvariant().Contains(cond2))
            {
                attachmentFileName = invoiceFileName.ToLowerInvariant().Replace(cond2, "").Trim();
            }
            else if (invoiceFileName.ToLowerInvariant().Contains(cond3))
            {
                attachmentFileName = invoiceFileName.ToLowerInvariant().Replace(cond3, "").Trim();
            }
            else if (invoiceFileName.ToLowerInvariant().Contains(cond4))
            {
                attachmentFileName = invoiceFileName.ToLowerInvariant().Replace(cond4, "").Trim();
            }
            return attachmentFileName.ToUpperInvariant().Trim();
        }

        public void TestGetFile(int HeaderID)
        {
            try
            {
                Console.WriteLine("ODIINV24120358INV.pdf");
                string attachmentFileName = GetAttachmentFileName("ODIINV24120358INV.pdf");
                Console.WriteLine(attachmentFileName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string GenerateBase64Attachment(string fileUrl, string fileName, int HeaderID)
        {
            string fileRelativeUrl = Attachment_GetRelativeURL(fileUrl, fileName);  
            DecryptionModel decryption = Decrypt("SPO");
            string siteUrl = $"https://jresourcesid.sharepoint.com/sites/p2pdocumentation";
            P2PDocuments_UpdateAttachmentURL(HeaderID, ("https://jresourcesid.sharepoint.com" + fileRelativeUrl));
            try
            {
                SecureString secure = new SecureString();
                foreach (var c in decryption.password_)
                {
                    secure.AppendChar(c);
                }
                using (var context = new ClientContext(siteUrl))
                {
                    context.Credentials = new SharePointOnlineCredentials(decryption.username_, secure);
                    var web = context.Web;
                    var file = web.GetFileByServerRelativeUrl(fileRelativeUrl);
                    context.Load(file);
                    context.ExecuteQuery();

                    if(file == null)
                    {
                        Console.WriteLine("File not found!");
                        return "";
                    }

                    ClientResult<Stream> streamResult = file.OpenBinaryStream();
                    context.ExecuteQuery();

                    using (var memoryStream = new MemoryStream())
                    {
                        streamResult.Value.CopyTo(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        string base64Content = Convert.ToBase64String(fileBytes);
                        return base64Content;
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
            var decryption = Decrypt("SPO");
            string siteUrl = "https://jresourcesid.sharepoint.com/sites/p2pdocumentation";
            string baseURL = "https://jresourcesid.sharepoint.com";
            try
            {
                SecureString secure = new SecureString();
                foreach (var c in decryption.password_)
                {
                    secure.AppendChar(c);
                }
                using (var context = new ClientContext(siteUrl))
                {
                    context.Credentials = new SharePointOnlineCredentials(decryption.username_, secure);
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

        public void DecryptCreds()
        {
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
            Console.Write("Enter your encrypted username: ");
            string usernameEncrypt = Console.ReadLine();
            Console.Write("Enter your encrypted password: ");
            string passwordEncrypt = Console.ReadLine();
            string realUsername = Utility.Decrypt(Convert.FromBase64String(usernameEncrypt), key, iv);
            string realPassword = Utility.Decrypt(Convert.FromBase64String(passwordEncrypt), key, iv);
            Console.WriteLine($"Your username: {realUsername}");
            Console.WriteLine($"Your password: {realPassword}");
        }

        public void EncryptCredentials(string username, string password)
        {
            EncryptionModel encryption = GetEncryption();
            byte[] key = Encoding.UTF8.GetBytes(encryption.key);
            byte[] iv = Encoding.UTF8.GetBytes(encryption.iv);
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

        public SPOFileModel SPOFile_GetByProSnapID(int ProSnapID)
        {
            DataTable dt = new DataTable();
            using(SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string query = "SELECT TOP 1 Created_By, Document_Name, Attachment_URL FROM P2PDocuments WHERE ProSnap_FileID = @ProSnapID";

                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ProSnapID", ProSnapID);
                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            var spoFile = Utility.ConvertDataTableToList<SPOFileModel>(dt)[0];
            return spoFile;
        }
    }
}
