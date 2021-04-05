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

            dynamic sources = JsonSerializer.Deserialize<Sources>(File.ReadAllText("sources.json"));
        
            string distroImage = "", distroName = "", distroPath = "";
            int distroNumber = 0;

            bool isConversionSuccessful;

            string easyWSLDirectory = (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).Remove(0, 6);
            string easyWSLDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "easyWSL");
            
            string distrosDirectory = $"{easyWSLDataDirectory}\\distros";
            Directory.CreateDirectory(distrosDirectory);

            if (args.Length >= 2)
            {
                foreach (int argument in Enumerable.Range(0, args.Length))
                {
                    if ((args[argument] == "-i") ^ (args[argument] == "--image"))
                    {
                        distroImage = args[argument + 1];
                    }

                    else if ((args[argument] == "-n") ^ (args[argument] == "--name"))
                    {
                        distroName = args[argument + 1];
                    }

                    else if ((args[argument] == "-p") ^ (args[argument] == "--path"))
                    {
                        distroPath = args[argument + 1];
                    }
                }
            }

            if (distroImage == "")
            {
                int count = 1;
                foreach (var source in sources.sources)
                {
                    Console.WriteLine($"{count}. {source.name}");
                    count++;
                }

                // additional entry for a custom image option
                Console.WriteLine($"{count}. Specify a docker image");

                do
                {
                    Console.Write("A number of a distro you want to install: ");
                    
                    isConversionSuccessful = Int32.TryParse(Console.ReadLine(), out distroNumber);
                } while ((distroNumber > sources.sources.Count + 1) ^ (isConversionSuccessful == false));

                if(distroNumber == sources.sources.Count+1)
                {
                    Console.Write("Specify a docker container: ");
                    distroImage = Console.ReadLine();
                }
                else
                {
                    distroImage = sources.sources[distroNumber - 1].image;
                }
                

                Console.WriteLine(distroImage);

            }
            
            if(distroName == "")
            {
                if (distroNumber == sources.sources.Count + 1)
                    Console.Write("A name for your distro: ");
                else
                    Console.Write("A name for your distro (default " + sources.sources[distroNumber - 1].name + "): ");
                distroName = Console.ReadLine();

                if (distroName == "")
                {
                    if (distroNumber == sources.sources.Count + 1)
                    {
                        while(distroName == "")
                        {
                            Console.Write("A name for your distro: ");
                            distroName = Console.ReadLine();
                        }
                    }
                    else
                        distroName = sources.sources[distroNumber-1].name;
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

            DistroInstaller.InstallDistro(distroImage, distroName, distroPath, easyWSLDataDirectory, easyWSLDirectory);
        }
    }
}
