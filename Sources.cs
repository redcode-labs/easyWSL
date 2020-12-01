using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyWSL
{
    class Sources
    {
        public dynamic sources = new Dictionary<int, DistroProperties>(){
            { 1, new DistroProperties { name="Ubuntu", path="https://cloud-images.ubuntu.com/focal/current/focal-server-cloudimg-amd64-wsl.rootfs.tar.gz"} },
            { 2, new DistroProperties { name="Dina", path="Salimzianova"} },
            { 3, new DistroProperties { name="Andy", path="Ruth"} }
        };

        class DistroProperties
        {
            public string name { get; set; }
            public string path { get; set; }
        }

        public static void ShowDistros()
        {
            Sources sourcesObj = new Sources();
            foreach (var index in Enumerable.Range(1, sourcesObj.sources.Count))
            {
                Console.WriteLine($"{index}. {sourcesObj.sources[index].name}");
            }
        }

        public static string ReturnDistroName(int index)
        {
            Sources sourcesObj = new Sources();
            string name = sourcesObj.sources[index].name;
            return name;
        }

        public static string ReturnDistroPath(int index)
        {
            Sources sourcesObj = new Sources();
            string path = sourcesObj.sources[index].path;
            return path;
        }

        public static int ReturnSourcesDictLenght()
        {
            Sources sourcesObj = new Sources();
            return sourcesObj.sources.Count;
        }
    }

    
}   
