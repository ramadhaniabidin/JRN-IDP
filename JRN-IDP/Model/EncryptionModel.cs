using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP.Model
{
    public class EncryptionModel
    {
        public string key { get; set; }
        public string iv { get; set; }
    }

    public class DecryptionModel
    {
        public string username_ { get; set; }
        public string password_ { get; set; }
    }
}
