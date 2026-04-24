using Daikin.BusinessLogics.Apps.NonCommercials.Model;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.NonCommercials.SharePointService
{
    public class ContractSharePointService
    {
        private readonly string moduleCode = "M014";
        private readonly string listName = "Contract";
        private readonly string generatedStatus = "Generated";
        private readonly SharePointManager _spManager;

        public ContractSharePointService(SharePointManager spManager)
        {
            _spManager = spManager;
        }

        public int SaveSPListContract(string siteUrl, ContractHeader ch, int totalItems, string status)
        {
            if (ch == null) throw new ArgumentNullException(nameof(ch));
            int itemId = 0;
            int id;
            if (ch.Item_ID != null && int.TryParse(ch.Item_ID.ToString(), out id)) itemId = id;

            using (var site = new SPSite(siteUrl))
            using (var web = site.OpenWeb())
            {
                var list = web.Lists[listName];
                web.AllowUnsafeUpdates = true;
                try
                {
                    SPListItem item;
                    if (itemId == 0)
                    {
                        item = list.AddItem();
                        item["Title"] = ch.Form_No;
                        item["Contract Remarks"] = ch.Remarks;
                        item["Request Date"] = ch.Request_Date;
                        item["Requester Branch"] = ch.Branch;
                        item["Requester Department"] = ch.Requester_Department;
                        item["Requester Name"] = ch.Requester_Name;
                        item["Requester Email"] = ch.Requester_Email;
                        item["Requester Account"] = _spManager.GetCurrentUserLogin();
                        item["Contract Type"] = ch.Contract_Type_ID;
                        item["Module"] = "M014";
                        item["Workflow Status"] = generatedStatus;
                    }
                    else
                    {
                        item = list.GetItemById(itemId);
                        if (ch.ID > 0) item["Transaction ID"] = ch.ID;
                    }
                    item["Procurement Department"] = ch.Procurement_Department;
                    item["Grand Total"] = ch.Grand_Total;
                    item["Approval Status"] = generatedStatus;
                    item["Form Status"] = status;
                    item.Update();
                    return item.ID;
                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }
        }


    }
}
