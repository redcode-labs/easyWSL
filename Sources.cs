using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyWSL
{
    class Sources
    {
        public List<SourceProperties> sources { get; set; }
    }

    public class SourceProperties
    {
        public string image { get; set; }
        public string name { get; set; }
    }
}
