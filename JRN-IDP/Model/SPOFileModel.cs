using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class SPOFileModel
    {
        public int Item_ID { get; set; }
        public string Document_Name { get; set; }
        public string List_Name { get; set; }
        public string List_ID { get; set; }
        public int Prosnap_Status { get; set; }
        public string Created_By { get; set; }
        public string Created_DateTime { get; set; }
        public string Document_Url { get; set; }
    }
}
