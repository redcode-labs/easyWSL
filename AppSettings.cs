using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace easyWSL
{
    public class AppSettings
    { 
        public bool IsFirstRun { get; set; }
        public string DistrosPath { get; set; }
        public Dictionary<string, DistrosProperties> installedDistros { get; set; }

        public class DistrosProperties
        {
            public string Distro { get; set; }
            public string Path { get; set; }
        }
    }
}
