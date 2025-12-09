using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common.Model
{
    public class ProgramModel
    {
    }

    public class Options
    {
        public string callbackUrl { get; set; }
        public string instanceToken { get; set; }
    }

    public class GetAttachmentFromSFParam
    {
        public ParamStartData startData { get; set; }
    }

    public class ParamStartData
    {
        public string se_ponumber { get; set; }
    }

    public class GetAttachmentFromSF
    {
        public string url { get; set; }
        public GetAttachmentFromSFParam param { get; set; }
    }
}