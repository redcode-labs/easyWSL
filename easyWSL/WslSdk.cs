using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace easyWSL
{
    class WslSdk
    {
        private Helpers helpers = new();

        public static Dictionary<string, InstalledDistrosProperties> InstalledDistros = new Dictionary<string, InstalledDistrosProperties> { };
        public class InstalledDistrosProperties
        {
            public string name { get; set; }
            public string path { get; set; }
            public string state { get; set; }
            public string version { get; set; }
            public string regkey { get; set; }
            public string size { get; set; }
        }

        public static bool CheckIfWSLInstalled()
        {
            var currentUserReg = Registry.CurrentUser;
            var lxssPathReg = currentUserReg.OpenSubKey(Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss"), false);
            if (lxssPathReg == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static async Task GetInstalledDistributions()
        {
            InstalledDistros.Clear();

            var currentUserReg = Registry.CurrentUser;
            var lxssPathReg = currentUserReg.OpenSubKey(Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss"), false);
  
            String[] distrosRegkeyNames = lxssPathReg.GetSubKeyNames();
            foreach (string distroRegkeyName in distrosRegkeyNames)
            {
                var distroRegkey = lxssPathReg.OpenSubKey(distroRegkeyName, false);
                var distroName = distroRegkey.GetValue("DistributionName");
                var distroPath = distroRegkey.GetValue("BasePath");
                var distroVersion = distroRegkey.GetValue("Version");
                string distroState = "Stopped";

                
                if (distroName != null && distroPath != null && distroState != null && distroVersion != null)
                {
                    string sizeString = "0.0 GB";
                    if (Directory.Exists(distroPath.ToString()))
                    {
                        DirectoryInfo distroDir = new DirectoryInfo(distroPath.ToString());
                        long sizeBytes = Helpers.DirSize(distroDir);
                        double sizeGigaBytes = (double)sizeBytes / 1024 / 1024 / 1024;
                        sizeString = $"{String.Format("{0:F2}", sizeGigaBytes)} GB";
                    }


                    InstalledDistros.Add(distroName.ToString(), new InstalledDistrosProperties() { name = distroName.ToString(), 
                                                                                                   path = distroPath.ToString(), 
                                                                                                   state = distroState.ToString(), 
                                                                                                   version = distroVersion.ToString(), 
                                                                                                   regkey = distroRegkeyName,
                                                                                                   size = sizeString
                    });
                }
                distroRegkey.Close();
            }
            lxssPathReg.Close();
        }
    }
}
