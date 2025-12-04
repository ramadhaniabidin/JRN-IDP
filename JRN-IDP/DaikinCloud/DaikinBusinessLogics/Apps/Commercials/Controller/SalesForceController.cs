using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Apps.Commercials.Controller
{
    public class SalesForceController
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        public string GetAttachmentToken()
        {
            string url = ConfigurationManager.AppSettings["SF_OAuthURL"];
            HttpClient client = new HttpClient();
            var formData = new Dictionary<string, string>
            {
                { "username", ConfigurationManager.AppSettings["SF_Username"] },
                { "password", ConfigurationManager.AppSettings["SF_Password"] },
                { "grant_type", "password" },
                { "client_id", ConfigurationManager.AppSettings["SF_ClientID"] },
                { "client_secret", ConfigurationManager.AppSettings["SF_ClientSecret"] }
            };
            var httpContent = new FormUrlEncodedContent(formData);
            var response = client.PostAsync(url, httpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseObject = serializer.Deserialize<Dictionary<string, object>>(responseJson);
            if (responseObject.ContainsKey("access_token"))
            {
                return responseObject["access_token"].ToString();
            }
            else
            {
                throw new Exception("Access token not found in the response.");
            }
        }

        public List<Dictionary<string, object>> GetListAttachmentID(string poNumber)
        {
            try
            {
                string token = GetAttachmentToken();
                string url = $"https://daikinindonesia--dev.sandbox.my.salesforce.com/services/apexrest/POContentVersion/{poNumber}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("");
                var formData = new Dictionary<string, string>
                {
                };
                var response = client.PostAsync(url, new FormUrlEncodedContent(formData)).Result;
                var responseJson = response.Content.ReadAsStringAsync().Result;
                var base64Serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
                var responseObject = base64Serializer.Deserialize<List<Dictionary<string, object>>>(responseJson);
                return responseObject;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<string> GetAttachmentAttributes(List<Dictionary<string, object>> list, string type)
        {
            List<string> attributes = new List<string>();
            foreach (var item in list)
            {
                var att = item[type] != null ? CleanXMLString(item[type].ToString()) : "";
                attributes.Add(att);
            }
            return attributes;
        }

        public string CleanXMLString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            string cleaned = new string(input.Where(c =>
                c == '\t' || c == '\n' || c == '\r' || (c >= 32 && c <= 126)).ToArray());
            return SecurityElement.Escape(cleaned);
        }

        public string GetBase64(string id)
        {
            try
            {
                string token = GetAttachmentToken();
                string url = ConfigurationManager.AppSettings["SF_BaseURL"] + $"/services/data/v54.0/sobjects/ContentVersion/{id}/VersionData";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Content = new StringContent("");
                var response = client.GetAsync(url).Result;
                byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                return Convert.ToBase64String(fileBytes);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateAttachmentFromBase64String(string listName, int itemID, string base64, string fileName, string extension)
        {
            try
            {
                string siteURL = Utility.SpSiteUrl_DEV;
                string fileNameWithExtension = $"{fileName}.{extension}";
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPSite site = new SPSite(siteURL))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            web.AllowUnsafeUpdates = true;
                            SPList list = web.Lists[listName];
                            SPListItem item = list.GetItemById(itemID);
                            SPAttachmentCollection attachmentList = item.Attachments;
                            int attachmentCount = attachmentList.Count;
                            if (attachmentCount > 0)
                            {
                                for (int i = 0; i < attachmentCount; i++)
                                {
                                    string currentAttachment = attachmentList[i];
                                    if (currentAttachment.ToUpper() == fileNameWithExtension.ToUpper())
                                    {
                                        attachmentList.Delete(fileNameWithExtension);
                                        break;
                                    }
                                }
                            }
                            byte[] contentData = Convert.FromBase64String(base64);
                            item.Attachments.Add(fileNameWithExtension, contentData);
                            item.Update();
                            web.AllowUnsafeUpdates = false;
                        }
                    }
                });
                //SalesForce_UpdateAttachment_InsertLog(itemID, fileName, extension, 1, "OK");
            }
            catch (Exception ex)
            {
                //SalesForce_UpdateAttachment_InsertLog(itemID, fileName, extension, 0, ex.Message);
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetAttachmentResponse(string poNumber)
        {
            try
            {
                string token = GetAttachmentToken();
                string url = $"https://daikinindonesia.my.salesforce.com/services/apexrest/POAttachment/{poNumber}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("");
                var formData = new Dictionary<string, string>
                {
                };
                var response = client.PostAsync(url, new FormUrlEncodedContent(formData)).Result;
                var responseJson = response.Content.ReadAsStringAsync().Result;
                var base64Serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };
                var responseObject = base64Serializer.Deserialize<List<Dictionary<string, object>>>(responseJson);
                return responseObject;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SalesForce_UpdateAttachment_InsertLog(int itemID, string fileName, string extension, int status, string message)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_SalesForce_UpdateAttachment_InsertLog";
                db.cmd.CommandType = System.Data.CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Item_ID", itemID);
                db.AddInParameter(db.cmd, "File_Name", fileName);
                db.AddInParameter(db.cmd, "Extension", extension);
                db.AddInParameter(db.cmd, "Status", status);
                db.AddInParameter(db.cmd, "Message", message);
                db.cmd.ExecuteNonQuery();
                db.CloseConnection(ref conn);
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}
