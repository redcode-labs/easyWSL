using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace easyWSL
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
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
            }

            SortedDictionary<string, Sources> sources = JsonSerializer.Deserialize<SortedDictionary<string, Sources>>(File.ReadAllText("sources.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            string distroID = "", distroName = "", distroPath = "";
            int distroNumber;

            bool isConversionSuccessful;

            string easyWSLDirectory = (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).Remove(0, 6);
            string easyWSLDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "easyWSL");
            
            string distrosDirectory = $"{easyWSLDataDirectory}\\distros";
            Directory.CreateDirectory(distrosDirectory);

            if (args.Length >= 2)
            {
                foreach (int argument in Enumerable.Range(0, args.Length))
                {
                    if ((args[argument] == "-d") ^ (args[argument] == "--distro"))
                    {
                        distroID = args[argument + 1];
                    }

                    else if ((args[argument] == "-n") ^ (args[argument] == "--name"))
                    {
                        distroName = args[argument + 1];
                    }

                    else if ((args[argument] == "-p") ^ (args[argument] == "--path"))
                    {
                        distroID = args[argument + 1];
                    }
                }
            }

            if (distroID == "")
            {
                int count = 1;
                foreach (KeyValuePair<string, Sources> kvp in sources)
                {
                    Console.WriteLine($"{count}. {sources[kvp.Key].Name}");
                    count++;
                }

                do
                {
                    Console.Write("A number of a distro you want to install: ");
                    
                    isConversionSuccessful = Int32.TryParse(Console.ReadLine(), out distroNumber);
                } while ((distroNumber > sources.Count) ^ (isConversionSuccessful == false));
                
                

                count = 1;
                foreach (KeyValuePair<string, Sources> kvp in sources)
                {
                    distroID = kvp.Key;
                    count++;
                    if (count > distroNumber)
                        break;
                }
            }
           
            if(distroName == "")
            {
                Console.Write("A name for your distro (default " + sources[distroID].Name + "): ");
                distroName = Console.ReadLine();

                if (distroName == "")
                {
                    distroName = sources[distroID].Name;
                }

                distroName = Regex.Replace(distroName, @"\s+", "");
            }

            if( Directory.Exists(distroPath) == false)
            {

                do
                {
                    Console.Write("A path to a directory where you want to store your distro (default is " + distrosDirectory + "\\" + distroName + "): ");
                    distroPath = Console.ReadLine();
                } while((Directory.Exists(distroPath) == false) ^ (distroPath == ""));
                

                if(distroPath == "")
                {
                    distroPath = distrosDirectory + "\\" + distroName;
                }
            }


            Directory.CreateDirectory(distroPath);

            DistroInstaller.InstallDistro(distroID, distroName, distroPath, easyWSLDataDirectory, easyWSLDirectory);
        }
    }
}
