using Daikin.BusinessLogics.Common.Model;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common
{
    public class SPListModel
    {
        public string List_Name { get; set; }
        public int Item_ID { get; set; }
        public string PathFile { get; set; }
        public string File_Name { get; set; }
        public string Url_Site { get; set; }
        public byte[] Content_Data { get; set; }
    }
    public class SharePointManager
    {

        public List<ApproverRoleModel> BindingMasterApproverSPListCondition(string ListName)
        {
            DataTable dt = new DataTable();
            List<ApproverRoleModel> listOption = new List<ApproverRoleModel>();
            try
            {
                SPWeb web = SPContext.Current.Web;
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {

                    SPList list = web.Lists[ListName];
                    SPListItemCollection items = list.Items;
                    dt = new DataTable();
                    dt = items.GetDataTable();
                });


                ApproverRoleModel data = new ApproverRoleModel();


                foreach (DataRow row in dt.Rows)
                {
                    data = new ApproverRoleModel();
                    data.Position_ID = Utility.GetIntValue(row, "Position_x003a_ID");
                    data.Position_Name = Utility.GetStringValue(row, "Position_x003a_Title");
                    data.Order_ID = Utility.GetIntValue(row, "Order_x0020_Id");
                    bool ItemExists = listOption.Any(item => item.Position_ID == data.Position_ID);
                    if (!ItemExists)
                    {
                        listOption.Add(data);
                    }
                }
                data = new ApproverRoleModel();
                data.Position_ID = 0;
                data.Position_Name = "All";
                data.Order_ID = 0;
                listOption.Insert(0, data);


                data = new ApproverRoleModel();
                data.Position_ID = 999;
                data.Position_Name = "None";
                data.Order_ID = 99;
                listOption.Add(data);

                return listOption.OrderBy(o => o.Order_ID).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string CopyListAttachmentToAnotherList(int SourceItemId, int DestItemId, string WebUrl, string listName, string DestListName)
        {
            try
            {
                List<SPListModel> listAttach = new List<SPListModel>();
                string fileName = string.Empty;
                using (SPSite oSPsite = new SPSite(WebUrl))
                {
                    using (SPWeb oSPWeb = oSPsite.OpenWeb())
                    {
                        oSPWeb.AllowUnsafeUpdates = true;
                        SPList list = oSPWeb.Lists[listName];
                        SPListItem item = list.GetItemById(SourceItemId);

                        SPAttachmentCollection attachmentsColl = item.Attachments;

                        //Loop through each attachment
                        foreach (string attachment in attachmentsColl)
                        {
                            SPFile file = oSPWeb.GetFile(attachmentsColl.UrlPrefix + attachment);
                            fileName = file.Name;
                            byte[] binFile = file.OpenBinary();
                            SPListModel model = new SPListModel();
                            model.Item_ID = DestItemId;
                            model.File_Name = file.Name;
                            model.List_Name = DestListName;
                            model.Url_Site = WebUrl;
                            model.Content_Data = binFile;
                            listAttach.Add(model);
                        }

                    }
                };
                UploadFileInCustomList(listAttach, WebUrl);

                return fileName;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void UploadFileInCustomList(string List_Name, int Item_ID, string PathFile, string File_Name, string Url_Site)
        {
            FileStream fileStream = File.OpenRead(PathFile);
            var ContentData = new byte[fileStream.Length];
            using (SPSite Site = new SPSite(Url_Site))
            {
                using (SPWeb Web = Site.OpenWeb())
                {
                    Web.AllowUnsafeUpdates = true;
                    SPList List = Web.Lists[List_Name];
                    SPListItem item = List.GetItemById(Item_ID);
                    SPAttachmentCollection attchList = item.Attachments;
                    int countElem = attchList.Count;
                    if (countElem > 0)
                    {
                        for (int i = 0; i < countElem; i++)
                        {
                            string currAttch = attchList[i];
                            if (currAttch.ToUpper() == File_Name.ToUpper())
                            {
                                attchList.Delete(File_Name);
                                break;
                            }
                        }
                    }

                    item.Attachments.Add(File_Name, ContentData);
                    item.Update();
                    Web.AllowUnsafeUpdates = false;
                }
            }

        }

        public void SaveRptDocuments(string siteUrl, string customListName = null, string localPath = "", int listItemId = 0)
        {

            if (!siteUrl.EndsWith("/"))
                siteUrl += "/";

            string currentLogin = GetCurrentUserLogin(siteUrl);
            UploadFileInCustomList(customListName, listItemId, localPath, siteUrl);

        }

        public string CamlQuerySPList(SPWeb web, string ListName, string FilterBy, string FilterValue, string RetrieveColumn)
        {
            SPList list = web.Lists[ListName];
            var q = new SPQuery()
            {
                Query = @"<Where><Eq><FieldRef Name='" + FilterBy + "' /><Value Type='Text'>" + FilterValue + "</Value></Eq></Where>"
            };


            var r = list.GetItems(q);
            if(r.Count > 0)
            {
                var value = r[0][RetrieveColumn];
                return value?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        public List<OptionModel> GetMultipleValues(SPWeb web, int SPUserId, string AttributeName)
        {
            List<OptionModel> listValues = new List<OptionModel>();
            SPList list = web.Lists[new Guid("308cecf4-12c7-4930-b097-01faa963f27c")];
            var q = new SPQuery()
            {
                Query = @"<Where><Eq><FieldRef Name='PIC_x0020_Account' LookupId='TRUE'/><Value Type='User'>" + SPUserId.ToString() + "</Value></Eq></Where>"
            };


            var r = list.GetItems(q);

            foreach (SPListItem item in r)
            {
                SPFieldLookupValueCollection MultipleValues = item[AttributeName] as SPFieldLookupValueCollection;
                foreach (SPFieldLookupValue itemValue in MultipleValues)
                {
                    listValues.Add(new OptionModel
                    {
                        Code = itemValue.LookupId.ToString(),
                        Name = itemValue.LookupValue.ToString()
                    });
                }
            }
            return listValues;
        }

        public List<string> GetBranchFinance(SPWeb web, int SPUserId, string AttributeName)
        {
            string branch = "";
            List<string> listBranch = new List<string>();
            SPList list = web.Lists["Master Finance Team"]; //TESTING LOCAL
            var q = new SPQuery()
            {
                Query = @"<Where><Eq><FieldRef Name='PIC_x0020_Account' LookupId='TRUE'/><Value Type='User'>" + SPUserId.ToString() + "</Value></Eq></Where>"
            };


            var r = list.GetItems(q);

            foreach (SPListItem item in r)
            {
                try
                {
                    string itemAttributeValue = item[AttributeName].ToString();
                    branch = itemAttributeValue.Split('#')[1];
                    listBranch.Add(branch);
                }
                catch
                {

                }
            }
            return listBranch;
        }
        public bool IsUserExistsFinanceTeam(SPWeb web, string AttributeName, string CompareValue)
        {
            bool IsExists = false;
            SPList list = web.Lists[new Guid("308cecf4-12c7-4930-b097-01faa963f27c")];
            var q = new SPQuery()
            {
                Query = @"<Where><Contains><FieldRef Name='" + AttributeName + "'/><Value Type='Text'>" + CompareValue.ToString() + "</Value></Contains></Where>"
            };


            var r = list.GetItems(q);

            foreach (SPListItem item in r)
            {
                try
                {
                    string itemAttributeValue = item[AttributeName].ToString();
                    if (itemAttributeValue.ToLower() == CompareValue.ToLower())
                    {
                        IsExists = true;
                        return IsExists;
                    }
                }
                catch
                {

                }
            }
            return IsExists;
        }

        public List<string> GetListBranch(SPWeb web, string CurrentLogin)
        {
            SPList list = web.Lists[new Guid("308cecf4-12c7-4930-b097-01faa963f27c")];
            var q = new SPQuery()
            {
                Query = @"<Where><Contains><FieldRef Name='UserAccName' /><Value Type='Text'>" + CurrentLogin.ToLower() + "</Value></Contains></Where>"
            };


            var r = list.GetItems(q);
            List<string> listBranch = new List<string>();
            foreach (SPListItem item in r)
            {
                try
                {
                    string itemAttributeValue = item["UserAccName"].ToString();
                    if (itemAttributeValue.ToLower().Contains(CurrentLogin.ToLower()))
                    {
                        string itemBranchValue = item["Branch"].ToString();
                        string branch = itemBranchValue.Split('#')[1];
                        listBranch.Add(branch);
                    }
                }
                catch
                {

                }
            }
            return listBranch;
        }

        public void SendEmail(SPWeb web, string subject, string email_address, string body_email)
        {
            if (email_address.Length <= 0)
            {
                email_address = "kenny.cassandra@elistec.com";
            }
            Microsoft.SharePoint.Utilities.SPUtility.SendEmail(web, true, true, email_address, subject, body_email, false);

        }
        public void CreateSharePointGroup(SPSite site, string groupName, string groupDescription)
        {
            SPWeb root = site.RootWeb;
            SPGroup group = null;

            // Check if the group exists
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    root.AllowUnsafeUpdates = true;
                    group = root.SiteGroups[groupName];
                });
            }
            catch { }

            // If it doesn't, add it
            if (group == null)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    root.SiteGroups.Add(groupName, root.SiteUsers[root.Author.LoginName], root.Author, groupDescription);
                    group = root.SiteGroups[groupName];

                    // Add the group's permissions
                    SPRoleDefinition roleDefinition = root.RoleDefinitions.GetByType(SPRoleType.Contributor);
                    SPRoleAssignment roleAssignment = new SPRoleAssignment(group);
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                    root.RoleAssignments.Add(roleAssignment);
                    root.Update();
                    root.AllowUnsafeUpdates = false;
                });
            }
        }
        public bool CheckGroupExistsInSiteCollection(SPWeb web, string groupName)
        {
            return web.SiteGroups.OfType<SPGroup>().Count(g => g.Name.Equals(groupName, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }
        public void RemoveUserFromSPGroup(SPSite site, string userLoginName, string groupName)
        {
            //Executes this method with Full Control rights even if the user does not have Full Control
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPWeb spWeb = site.RootWeb)
                {
                    try
                    {
                        spWeb.AllowUnsafeUpdates = true;
                        SPUser spUser = spWeb.EnsureUser(userLoginName);
                        if (spUser != null && CheckGroupExistsInSiteCollection(spWeb, groupName))
                        {
                            SPGroup spGroup = spWeb.Groups[groupName];
                            if (spGroup != null)
                                spGroup.RemoveUser(spUser);
                        }
                    }
                    catch (Exception)
                    {
                        //exception handling
                    }
                    finally
                    {
                        spWeb.AllowUnsafeUpdates = false; //Even Exception occurs it set back to false
                    }
                }
            });
        }
        public string AddUserToGroupPermission(string siteURL, string userName, string groupName)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPSite site = new SPSite(siteURL))
                    {
                        using (SPWeb myWeb = site.OpenWeb())
                        {
                            myWeb.AllowUnsafeUpdates = true;
                            SPUser spUser = myWeb.EnsureUser(userName);
                            SPGroup spGroup = myWeb.SiteGroups[groupName];
                            int noofcounts = spGroup.Users.Count;
                            spGroup.AddUser(spUser);

                            spGroup.Update();
                            myWeb.AllowUnsafeUpdates = false;
                        }
                    }
                });
                return "OK";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }

        }

        public string GetAllAttachmentByListName(string listName, int SourceItemId, string WebUrl)
        {
            try
            {
                string attachUrl = string.Empty;
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {

                    using (SPSite oSPsite = new SPSite(WebUrl))
                    {
                        using (SPWeb oSPWeb = oSPsite.OpenWeb())
                        {
                            oSPWeb.AllowUnsafeUpdates = true;
                            SPList list = oSPWeb.Lists[listName];
                            SPListItem item = list.GetItemById(SourceItemId);

                            SPAttachmentCollection attachmentsColl = item.Attachments;

                            //Loop through each attachment
                            foreach (string attachment in attachmentsColl)
                            {
                                SPFile file = oSPWeb.GetFile(attachmentsColl.UrlPrefix + attachment);
                                attachUrl += ";" + attachmentsColl.UrlPrefix + attachment;
                            }

                        }
                    }
                });
                return attachUrl;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string RemovePatternUserWithDomain(string loginName)
        {
            return loginName.Contains("|") ?
                         loginName.Split('|')[1] : loginName;
        }
        public string GetCurrentUserLogin(string SPUrl, bool WithPattern = true)
        {
            //With Pattern = i:0#.w|eiu\kenny
            //Without Pattern = eiu\kenny
            SPSite site = new SPSite(SPUrl);
            SPWeb web = site.OpenWeb();
            string CurrentUser = string.Empty;
            if (!WithPattern)
                CurrentUser = web.CurrentUser.LoginName.Contains("|") ?
                         web.CurrentUser.LoginName.Split('|')[1] : web.CurrentUser.LoginName;
            else
                CurrentUser = web.CurrentUser.LoginName;
            return CurrentUser;
        }

        public int GetCurrentUserId(string SPUrl)
        {
            SPSite site = new SPSite(SPUrl);
            SPWeb web = site.OpenWeb();
            return web.CurrentUser.ID;
        }

        public string GetCurrentLoginFullName(string SPUrl)
        {
            SPSite site = new SPSite(SPUrl);
            SPWeb web = site.OpenWeb();

            string CurrentUser = web.CurrentUser.Name;
            return CurrentUser;
        }

        public string GetCurrentLoginEmail(string SPUrl)
        {
            SPSite site = new SPSite(SPUrl);
            SPWeb web = site.OpenWeb();

            string CurrentUser = web.CurrentUser.Email;
            return CurrentUser;
        }

        public string CreateFolderDocumentLibrary(string FolderName, string SiteUrl, string DocLibName)
        {
            string CombinePath = "";
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                SPSite site = new SPSite(SiteUrl);
                SPWeb web = site.OpenWeb();
                web.AllowUnsafeUpdates = true;
                SPDocumentLibrary DocLib = (SPDocumentLibrary)web.Lists[DocLibName];
                SPFolderCollection folders = web.Folders;
                CombinePath = SiteUrl + DocLibName + "/" + FolderName;
                folders.Add(CombinePath);
                DocLib.Update();
                web.AllowUnsafeUpdates = false;

            });
            return CombinePath;
        }

        public void ensureParentFolder(string destUrl, string Current_Site)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                // implementation details omitted
                using (SPSite site = new SPSite(Current_Site))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        int index = 0;
                        destUrl = web.GetFile(destUrl).Url;
                        index = destUrl.LastIndexOf("/");
                        string parentFolderUrl = string.Empty;
                        if (index > -1)
                        {
                            parentFolderUrl = destUrl.Substring(0, index);
                            SPFolder parentFolder = web.GetFolder(parentFolderUrl);

                            if (!parentFolder.Exists)
                            {
                                SPFolder currentFolder = web.RootFolder;
                                web.AllowUnsafeUpdates = true;
                                foreach (string folder in parentFolderUrl.Split('/'))
                                {
                                    // implementation details omitted
                                    currentFolder = currentFolder.SubFolders.Add(folder);
                                }
                                web.AllowUnsafeUpdates = false;

                            }
                        }
                    }
                }
            });
        }

        public void UploadFileInCustomList(string List_Name, int Item_ID, string PathFile, string Url_Site)
        {
            string File_Name = Path.GetFileName(PathFile);

            FileStream fs = new FileStream(PathFile, FileMode.Open, FileAccess.Read);

            Byte[] imgByte = new byte[fs.Length];
            fs.Read(imgByte, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite Site = new SPSite(Url_Site))
                {
                    using (SPWeb Web = Site.OpenWeb())
                    {
                        Web.AllowUnsafeUpdates = true;
                        SPList List = Web.Lists[List_Name];
                        SPListItem item = List.GetItemById(Item_ID);
                        SPAttachmentCollection attchList = item.Attachments;
                        int countElem = attchList.Count;
                        if (countElem > 0)
                        {
                            for (int i = 0; i < countElem; i++)
                            {
                                string currAttch = attchList[i];
                                if (currAttch.ToUpper() == File_Name.ToUpper())
                                {
                                    attchList.Delete(File_Name);
                                    break;
                                }
                            }
                        }

                        item.Attachments.Add(File_Name, imgByte);
                        string fileName = System.IO.Path.GetFileName(PathFile);
                        item["Link Attachment"] = $"/Lists/{List_Name}/Attachments/" + Item_ID.ToString() + "/" + fileName;
                        item.Update();
                        Web.AllowUnsafeUpdates = false;
                    }
                }
            });

        }

        public void UploadFileInCustomList(List<SPListModel> list, string Url_Site)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite Site = new SPSite(Url_Site))
                {
                    using (SPWeb Web = Site.OpenWeb())
                    {
                        Web.AllowUnsafeUpdates = true;
                        SPListItem item = null;
                        foreach (SPListModel s in list)
                        {
                            SPList List = Web.Lists[s.List_Name];
                            item = List.GetItemById(s.Item_ID);
                            SPAttachmentCollection attchList = item.Attachments;
                            int countElem = attchList.Count;
                            if (countElem > 0)
                            {
                                for (int i = 0; i < countElem; i++)
                                {
                                    string currAttch = attchList[i];
                                    if (currAttch.ToUpper() == s.File_Name.ToUpper())
                                    {
                                        attchList.Delete(s.File_Name);
                                        break;
                                    }
                                }
                            }
                            item.Attachments.Add(s.File_Name, s.Content_Data);
                            item.Update();
                        }
                        Web.AllowUnsafeUpdates = false;
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }
            });
        }

        public void UploadFileInCustomList(string List_Name, int Item_ID, byte[] ContentData, string Url_Site, string File_Name)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite Site = new SPSite(Url_Site))
                {
                    using (SPWeb Web = Site.OpenWeb())
                    {
                        Web.AllowUnsafeUpdates = true;
                        SPList List = Web.Lists[List_Name];
                        SPListItem item = List.GetItemById(Item_ID);
                        SPAttachmentCollection attchList = item.Attachments;
                        int countElem = attchList.Count;
                        if (countElem > 0)
                        {
                            for (int i = 0; i < countElem; i++)
                            {
                                string currAttch = attchList[i];
                                if (currAttch.ToUpper() == File_Name.ToUpper())
                                {
                                    attchList.Delete(File_Name);
                                    break;
                                }
                            }
                        }

                        item.Attachments.Add(File_Name, ContentData);
                        Web.AllowUnsafeUpdates = false;
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }
            });

        }

        //Dest Url with fileName
        //Url Site : http://hostname
        //Doc Lib : Document Library Name
        public void UploadDocLib2(byte[] arrFile, string UrlSite, string DocLib, string destUrl)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                using (SPSite oSite = new SPSite(UrlSite))
                {
                    using (SPWeb oWeb = oSite.OpenWeb())
                    {
                        oWeb.AllowUnsafeUpdates = true;
                        SPList list = oWeb.Lists[DocLib];
                        list.RootFolder.Files.Add(destUrl, arrFile, true);
                        oWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        public string UploadDocLib(byte[] arrFile, string UrlSite, string DocLibName, string DestUrl)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                using (SPSite oSite = new SPSite(UrlSite))
                {
                    using (SPWeb oWeb = oSite.OpenWeb())
                    {
                        string FileName = System.IO.Path.GetFileName(DestUrl);
                        string FolderUrl = DestUrl.Replace(FileName, string.Empty);
                        ensureParentFolder(FolderUrl, UrlSite);

                        oWeb.AllowUnsafeUpdates = true;
                        SPFolder myLibrary = oWeb.Folders[DocLibName];

                        Boolean replaceExistingFiles = true;
                        SPFile spfile = myLibrary.Files.Add(DestUrl, arrFile, replaceExistingFiles);

                        myLibrary.Update();
                        oWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
            return DestUrl;
        }

        public string UploadDocLib(string PathFile, string UrlSite, string DocLibName, string DestUrl)
        {
            string FileName = Path.GetFileName(PathFile);
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {

                using (SPSite oSite = new SPSite(UrlSite))
                {
                    using (SPWeb oWeb = oSite.OpenWeb())
                    {
                        oWeb.AllowUnsafeUpdates = true;
                        if (!System.IO.File.Exists(PathFile))
                            throw new FileNotFoundException("File not found.", PathFile);

                        SPFolder myLibrary = oWeb.Folders[DocLibName];
                        Boolean replaceExistingFiles = true;
                        FileStream fileStream = File.OpenRead(PathFile);
                        SPFile spfile = myLibrary.Files.Add(DestUrl, fileStream, replaceExistingFiles);
                        myLibrary.Update();
                        oWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
            return DestUrl;
        }

        public void DeleteFileInSharePointListItem(int ItemId, string File_Name, string List_Name, string Url_Site)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(Url_Site))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        web.AllowUnsafeUpdates = true;
                        SPListItem listItem = web.Lists[List_Name].GetItemById(ItemId);
                        if (listItem != null)
                        {
                            foreach (string fileName in listItem.Attachments)
                            {
                                if (fileName.ToUpper() == File_Name.ToUpper())
                                {
                                    listItem.Attachments.Delete(fileName);
                                    break;
                                }
                            }

                            listItem.Update();
                        }
                        web.AllowUnsafeUpdates = false;
                    }
                }
            });
        }


        public void StartWorkflowBySystemAccount(string SiteURL, int Item_ID, string WF_Name, string List_Name)
        {
            try
            {
                SPSite spSite = new SPSite(SiteURL);
                SPWeb Web = spSite.OpenWeb();
                var currentLogin = new Utility().GetConfigValue("SystemUser");
                SPUserToken token = Web.EnsureUser(currentLogin).UserToken;
                spSite = new SPSite(Web.Site.ID, token);
                Web = spSite.OpenWeb();
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    SPList List = Web.Lists.TryGetList(List_Name);
                    SPListItem item = List.GetItemById(Item_ID);
                    SPWorkflowAssociationCollection associationCollection = List.WorkflowAssociations;
                    foreach (SPWorkflowAssociation association in associationCollection)
                    {
                        if (association.Name.ToUpper() == WF_Name.ToUpper())
                        {
                            Web.AllowUnsafeUpdates = true;
                            association.AssociationData = string.Empty;
                            spSite.WorkflowManager.StartWorkflow(item, association, association.AssociationData);
                            Web.AllowUnsafeUpdates = false;
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> RetrieveAttachmentBase64(string SiteURL, string ListName, int ItemID, string FileName)
        {
            try
            {
                SPListItem item = new SPSite(SiteURL).OpenWeb().Lists[ListName].GetItemById(ItemID);
                SPAttachmentCollection attchList = item.Attachments;
                using (SPSite site = new SPSite(SiteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        if (attchList.Count == 0)
                        {
                            return new Dictionary<string, object>
                            {
                                {"Success", false }, {"Message", "There are no attachments" }, {"Base64", "" }
                            };
                        }
                        foreach (string currAttach in attchList)
                        {
                            if (currAttach.StartsWith(FileName, StringComparison.OrdinalIgnoreCase))
                            {
                                string fileUrl = attchList.UrlPrefix + currAttach;
                                string urlPrefix = attchList.UrlPrefix;
                                Console.WriteLine(urlPrefix);
                                SPFile file = web.GetFile(fileUrl);
                                using (Stream fileStream = file.OpenBinaryStream())
                                {
                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        fileStream.CopyTo(memoryStream);
                                        string base64 = Convert.ToBase64String(memoryStream.ToArray());
                                        return new Dictionary<string, object>
                                        {
                                            {"Success", true }, {"Message", "File retrieved successfully" }, {"Base64", base64 }
                                        };
                                    }
                                }
                            }
                        }
                    }
                    return new Dictionary<string, object>
                    {
                        {"Success", false }, {"Message", "File not found" }, {"Base64", "" }
                    };
                }

            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    {"Success", false }, {"Message", ex.Message }, {"Base64", "" }
                };
            }
        }

        public Dictionary<string, object> RetrieveAttachmentBase64(string FileName)
        {
            string filePath = $@"\\dbs1\Nintex\Subcon\PO_Data\PrintOut\DONE\{FileName}.pdf";
            try
            {
                if (!File.Exists(filePath))
                {
                    return new Dictionary<string, object>
                    {
                        {"Success", false },
                        {"Message", "File not found" },
                        {"Base64", "" }
                    };
                }
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string base64String = Convert.ToBase64String(fileBytes);
                return new Dictionary<string, object>
                {
                    {"Success", true },
                    {"Message", "File retrieved successfully" },
                    {"Base64", base64String }
                };
            }
            catch(Exception ex)
            {
                return new Dictionary<string, object>
                {
                    {"Success", false },
                    {"Message", ex.Message },
                    {"Base64", "" }
                };
            }
        }
    }

}
