using JRN_IDP.Model;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
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
        //private readonly string connString = "Server=192.168.50.150;Database=IDP_JResources;User ID=sa;Password=sa_P@ssw0rd3L!5!!!;Encrypt=False";
        private readonly string connString = "Server=OCR-DEV;Database=IDP_JRN;User ID=sa;Password=Pa55word;Encrypt=False";
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
            string fileRelativeUrl = fileUrl.ToLower().Replace("%20-%20inv", "");
            Console.WriteLine($"File Relative url: {fileRelativeUrl}");
            string siteName = "p2pdocumentation";
            string siteUrl = $"https://jresourcesid.sharepoint.com/sites/{siteName}";
            string username = "elistec@jresources.com";
            string password = "?Mfu1y^X]zrBrro";
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
            // SharePoint site URL and credentials
            string siteName = "p2pdocumentation";
            string siteUrl = $"https://jresourcesid.sharepoint.com/sites/{siteName}";
            string username = "elistec@jresources.com";
            string password = "?Mfu1y^X]zrBrro";
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
    }
}
