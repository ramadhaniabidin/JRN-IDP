using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class AttachmentModel
    {
        public string module_code { get; set; }
        public string transaction_code { get; set; }
        public string list_name { get; set; }
        public int list_item_id { get; set; }
        public string file_name { get; set; }
        public string file_url { get; set; }
        public int is_deleted { get; set; }
        public string uploaded_by { get; set; }
    }
    public class AttachmentBRModel
    {
        public string Attachment_Selfie { get; set; }
        public int No { get; set; }
    }
    public class ReportModel
    {
        public string Report_Name { get; set; }
        public string Report_ID { get; set; }
        public string Extension { get; set; }
    }
}
