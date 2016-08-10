using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GsmManager.Entities.GsmEntities
{
    public class SystemSetting
    {
        public bool WebApiEnable { get; set; }
        public int? WebApiPort { get; set; }
        public string ReceivedCallback { get; set; }
        public string SentCallback { get; set; }
        public string FailedCallback { get; set; }
        public string AdminCallback { get; set; }
    }
}
