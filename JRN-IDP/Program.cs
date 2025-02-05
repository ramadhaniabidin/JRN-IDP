using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class Program
    {
        static void Main(string[] args)
        {
            SPOHandler spoHander = new SPOHandler();
            ProsnapHandler prosnap = new ProsnapHandler();
            APIHandler api = new APIHandler();
            NACHandler NAC = new NACHandler();
            string code = "TEST WORKFLOW NOTIFICATION PRODUCTION";
            if(code == "0")
            {
                spoHander.UploadFileToProsnap();
            }
            else if(code == "1")
            {
                spoHander.DecryptCreds();
            }
            else if (code.ToUpper() == "TEST WORKFLOW NOTIFICATION PRODUCTION")
            {
                NAC.CallNotificationWorkflow_Production("0");
            }
            else
            {
                Task.Run(async () =>
                {
                    await api.CreateInvoiceBulkAsync_New();
                }).Wait();
            }
        }
    }
}
