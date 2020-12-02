using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using IronPython.Hosting;

namespace easyWSL
{
    class Commands
    {
        public static void CallWSL(string arguments)
        {
            Process wsl = new Process();
            wsl.StartInfo.FileName = "wsl.exe";
            wsl.StartInfo.Arguments = arguments;
            wsl.Start();
        }

        public static void DownLoadFile(string address)
        {
            WebClient client = new WebClient();
            Uri uri = new Uri(address);

            
            client.DownloadFileAsync(uri, "install.tar.gz");
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress.
            Console.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
                (string)e.UserState,
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }



        public static void Install(string[] obj)
        {
            Sources sourcesObj = new Sources();

            // serializing settings
            dynamic appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appSettings.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // a function to sync JSON file after changing the properies of an appSettigns object
            void SyncJSON()
            {
                string json = JsonSerializer.Serialize(appSettings);
                File.WriteAllText("appSettings.json", json);
            }

            string distrosDirectory = appSettings.DistrosPath;

            Console.Clear();

            Sources.ShowDistros();

            //Console.WriteLine(appSettings.installedDistros["arch"].Name);
            Console.Write("Choose the number of a distro you want to install: ");

            // converting the user input to int
            string input = Console.ReadLine();
            int choosenDistroID;
            Int32.TryParse(input, out choosenDistroID);

            // protection from the wrong input
            if ((!Int32.TryParse(input, out choosenDistroID))^(choosenDistroID>Sources.ReturnSourcesDictLenght()))
            {
                Console.WriteLine ("Wrong value!");

                Console.Write("Choose the number of a distro you want to install: ");

                input = Console.ReadLine();
                Int32.TryParse(input, out choosenDistroID);
            }

            // choosing distro name from the dictonary
            string choosenDistroName = Sources.ReturnDistroName(choosenDistroID);
            // choosing a path on our vps to the specified distro from the dictonary
            string choosenDistroSource = Sources.ReturnDistroPath(choosenDistroID);


            Console.Write("Choose the name for your distro installation (default " + choosenDistroName + "): ");
            string customDistroName = Console.ReadLine();

            if (customDistroName == "")
            {
                customDistroName = choosenDistroName;
            }

            

            Console.Write("Type the path to directory where you want to store your " + customDistroName + " distro (default is " + distrosDirectory + "\\" + choosenDistroName + "): ");
            string customDistroPath = Console.ReadLine();

            if(customDistroPath == "")
            {
                customDistroPath = distrosDirectory+"\\"+ choosenDistroName;
            }
            Console.WriteLine("Creating the directory for your custom distro ...");
            Directory.CreateDirectory(customDistroPath);
            //Directory.CreateDirectory(easyWSLHome+ "\\Sources\\"+ choosenDistro);

            /*Console.WriteLine("Downloading distro image ...");
            
            using (var progress = new ProgressBar())
            {
                DownLoadFile(choosenDistroSource);
                for (int i = 0; i <= 100; i++)
                {
                    progress.Report((double)i / 100);
                    Thread.Sleep(20);
                }
            }
            Console.WriteLine("Done.");*/

            DockerPull.PullImage("ubuntu:latest");

            Console.WriteLine("Copying image ...");
            File.Copy("install.tar.gz", customDistroPath + "\\install.tar.gz", true);
            File.Delete("install.tar.gz");

            Console.WriteLine("Registering new WSL distro ...");
            Process.Start("wsl.exe", "--import" + " " + customDistroName + " " + customDistroPath + " " + customDistroPath + "\\install.tar.gz");

            // chuj z tym, nie wiem jak dodać zainstalowane distro do słownika installedDistros
            //appSettings.installedDistros.Add("txt", ["notepad.exe", "fsdfsdsd"]);
            //SyncJSON();

        }
        public static void Exit(string[] obj)
        {
            System.Environment.Exit(1);
        }

        public static void Help(string[] args)
        {
            Console.WriteLine("dupa");
        }


    }
}
