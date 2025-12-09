using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Daikin.API.Areas.Common.Models
{
    public class SAPBatchModel
    {
        public string SAPFolderID { get; set; }
        public int headerID { get; set; }
        public string fileName { get; set; }
    }
}