using System;
using System.Collections.Generic;
using System.Text;

namespace CloudflareIP_Updater.Models
{
    class CFZone
    {
        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string[] name_servers { get; set; }
        public int development_mode { get; set; }
    }
}
