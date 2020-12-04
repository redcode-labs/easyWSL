using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;




namespace easyWSL
{
    class Program
    {
        public string easyWSLHome = Environment.GetEnvironmentVariable("HOMEPATH") + "\\easyWSL";
        static void Main(string[] args)
        {
            Program main = new Program();

            // serializing settings
            dynamic appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appSettings.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // a function to sync JSON file after changing the properies of an appSettigns object
            void SyncJSON()
            {
                string json = JsonSerializer.Serialize(appSettings);
                File.WriteAllText("appSettings.json", json);
            }

            // creating necessary directories
            
            Directory.CreateDirectory(main.easyWSLHome);
            Directory.CreateDirectory(main.easyWSLHome+"\\Sources");
            


            // if the easyWSL app is started for the first time on this machine proceed the initial setup
            if (appSettings.IsFirstRun == true)
            {
                appSettings.IsFirstRun = false;
                SyncJSON();
                FirstRun();

            }
            else
            {
                NotAFirstRun();
            }

        }
        public static void FirstRun()
        {
            Program main = new Program();
            dynamic appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appSettings.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine("Welcome to");
            Console.WriteLine(" ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("                          _       _______ __ ");
            Console.WriteLine("    ___  ____ ________  _| |     / / ___// / ");
            Console.WriteLine("   / _ \\/ __ `/ ___/ / / / | /| / /\\__ \\/ /  ");
            Console.WriteLine("  /  __/ /_/ (__  ) /_/ /| |/ |/ /___/ / /___");
            Console.WriteLine("  \\___/\\__,_/____/\\__, / |__/|__//____/_____/");
            Console.WriteLine("                 /____/                      ");
            Console.WriteLine(" ");
            Console.ResetColor();
            Console.Write("Type the path where you want to store all your custom distros (default is "+main.easyWSLHome + "\\Distros" +"): ");
            string distrosDirectory = Console.ReadLine();
            if(distrosDirectory=="")
            {
                Directory.CreateDirectory(main.easyWSLHome+"\\Distros");
                distrosDirectory = main.easyWSLHome + "\\Distros";
                appSettings.DistrosPath = distrosDirectory;
                string json = JsonSerializer.Serialize(appSettings);
                File.WriteAllText("appSettings.json", json);

            }
            else
            {
                Directory.CreateDirectory(distrosDirectory);
            }

            CommandLine.Show();

        }

        public static void NotAFirstRun()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("                          _       _______ __ ");
            Console.WriteLine("    ___  ____ ________  _| |     / / ___// / ");
            Console.WriteLine("   / _ \\/ __ `/ ___/ / / / | /| / /\\__ \\/ /  ");
            Console.WriteLine("  /  __/ /_/ (__  ) /_/ /| |/ |/ /___/ / /___");
            Console.WriteLine("  \\___/\\__,_/____/\\__, / |__/|__//____/_____/");
            Console.WriteLine("                 /____/                      ");
            Console.WriteLine(" ");
            Console.ResetColor();
            CommandLine.Show();
            
        }




    }
}
