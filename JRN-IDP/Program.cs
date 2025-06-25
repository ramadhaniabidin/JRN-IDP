using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public static class Program
    {
        static void Main(string[] args)
        {
            SPOHandler spoHander = new SPOHandler();
            APIHandler api = new APIHandler();
            NACHandler NAC = new NACHandler();
            string code = "4";
            if(code == "0")
            {
                spoHander.UploadFileToProsnap();
            }
            else if(code == "NAC TASK")
            {
                string token = NintexTaskHandler.GetToken_DaikinNAC();
                Console.WriteLine(token);
                string Form_No = "8300214476";
                var getTaskResponse = NintexTaskHandler.GetTask_ByInstanceID_Async("c5b295e7-e4ec-48d3-9bf9-f941d1964e2d_0_4");
                var tasks = getTaskResponse.Result;
                var task = tasks.Tasks.FirstOrDefault(t => t.Name.Contains(Form_No));
                Console.WriteLine($"Task count: {tasks.Tasks.Count}");
                foreach(var t in tasks.Tasks)
                {
                    Console.WriteLine($"Task Name: {t.Name}");
                }

                if(task == null)
                {
                    Console.WriteLine("No task found");
                    return;
                }
                Console.WriteLine(task.Name);
                foreach(var assignment in task.TaskAssignments)
                {
                    Console.WriteLine($"\nAssignment ID: {assignment.Id}");
                    Console.WriteLine($"Assignee: {assignment.Assignee}");
                }
            }
            else if(code == "1")
            {
                spoHander.DecryptCreds();
            }
            else if (code.ToUpperInvariant() == "TEST WORKFLOW NOTIFICATION PRODUCTION")
            {
                NAC.CallNotificationWorkflow_Production("0");
            }
            else if(code == "3")
            {
                spoHander.TestGetFile(4);
            }
            else if(code == "4")
            {
                Task.Run(async () =>
                {
                    await api.GetOracleMasterSuppliers();
                }).Wait();
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
