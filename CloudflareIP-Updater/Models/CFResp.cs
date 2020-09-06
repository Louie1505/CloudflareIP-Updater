using System;
using System.Collections.Generic;
using System.Text;

namespace CloudflareIP_Updater.Models
{
    class CFResp
    {
        public object result { get; set; }
        public bool success { get; set; }
        public string[] errors { get; set; }
        public string[] messges { get; set; }
    }
}
