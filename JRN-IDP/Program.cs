using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string connString = ConfigurationManager.AppSettings["connString"];
                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"Connection String: {connString}");
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
