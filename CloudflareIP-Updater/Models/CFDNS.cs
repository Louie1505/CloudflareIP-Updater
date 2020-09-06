using System;
using System.Collections.Generic;
using System.Text;

namespace CloudflareIP_Updater.Models
{
    class CFDNS
    {
        public string id { get; set; }
        public string zone_id { get; set; }
        public string zone_name { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public bool locked { get; set; }
    }
}
