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
            string code = "1";
            if(code == "0")
            {
                spoHander.UploadFileToProsnap();
            }
            else if(code == "1")
            {
                //spoHander.TestEncryption();
                //spoHander.UpdateCredentials();
                //spoHander.TestConnection_JRNAzure();
                //spoHander.EncryptCreds();
                spoHander.DecryptCreds();
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
