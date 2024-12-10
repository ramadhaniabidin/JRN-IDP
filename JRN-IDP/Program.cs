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
            //prosnap.TestParsingJSON();
            string code = "0";
            if(code == "0")
            {
                spoHander.UploadFileToProsnap();
                System.Threading.Thread.Sleep(10000);
            }
            else if(code == "1")
            {
                //spoHander.TestEncryption();
                //spoHander.UpdateCredentials();
                spoHander.TestConnection_JRNAzure();
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
