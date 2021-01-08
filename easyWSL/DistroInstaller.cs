using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace easyWSL
{
    class DistroInstaller
    {
        public class TokenFromResponse
        {
            public string token { get; set; }
            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string issued_at { get; set; }

        }


        public static void InstallDistro(string distroID, string distroName, string distroPath, string easyWSLDataDirectory, string easyWSLDirectory)
        {

            void StartProcessSilently(string processName, string processArguments)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = processName;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.Arguments = processArguments;

                Process proc = Process.Start(psi);
                proc.WaitForExit();
            }

            string GetRequest(string url)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                request.Credentials = CredentialCache.DefaultCredentials;
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string responseStream = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
                return responseStream;
            }

            void GetRequestWithHeaderToFile(string url, string token, string type, string fileName)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("Authorization", "Bearer " + token);
                request.Accept = type;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                int bufferSize = 1024, bytesRead = 0;
                byte[] buffer = new byte[bufferSize];

                FileStream fileStream = File.Create(fileName);
                while((bytesRead = receiveStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
                
                response.Close();
                fileStream.Close();
            }


            SortedDictionary<string, Sources> sources = JsonSerializer.Deserialize<SortedDictionary<string, Sources>>(File.ReadAllText("sources.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            string repository = "", tag = "", registry = "registry-1.docker.io", authorizationUrl = "https://auth.docker.io/token", registryUrl = "registry.docker.io";

            if (sources[distroID].Image.Contains('/'))
            {
                string[] imageArray = sources[distroID].Image.Split('/');
                tag = "latest";
                repository = sources[distroID].Image;
            }

            else
            {
                string[] imageArray = sources[distroID].Image.Split(':');
                string imgage = imageArray[0];
                tag = imageArray[1];
                repository = $"library/{imgage}";
            }


            dynamic tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>(GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));

            string layersDirectory = $"{easyWSLDataDirectory}\\layers";
            Directory.CreateDirectory(layersDirectory);

            string concatTarCommand = $" cf {layersDirectory}\\install.tar";

            int count = 0;
            foreach (string layer in sources[distroID].Layers)
            {
                count++;
                Console.WriteLine($"Downloading {count}. layer ...");

                tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>(GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));

                string layerName = $"layer{count}.tar.bz";
                string layerPath = $"{layersDirectory}\\{layerName}";

                GetRequestWithHeaderToFile($"https://{registry}/v2/{repository}/blobs/{layer}", tokenFromResponse.token, "application/vnd.docker.distribution.manifest.v2+json", layerPath);
                concatTarCommand += $" @{layerPath} ";
            }


            Console.WriteLine("Creating install.tar file ...");
            if(sources[distroID].Layers.Count == 1)
            {
                File.Move($"{layersDirectory}\\layer1.tar.bz", $"{layersDirectory}\\install.tar.bz");

                Console.WriteLine("Registering the distro ...");
                StartProcessSilently("wsl.exe", $"--import {distroName} {distroPath} {easyWSLDataDirectory}\\layers\\install.tar.bz");
            }
            else
            {
                StartProcessSilently($"{easyWSLDirectory}\\dep\\bsdtar.exe", concatTarCommand);

                Console.WriteLine("Registering the distro ...");
                StartProcessSilently("wsl.exe", $"--import {distroName} {distroPath} {easyWSLDataDirectory}\\layers\\install.tar");
            }
            

            Console.WriteLine("Cleaning up ...");
            Directory.Delete(layersDirectory, true);

            Console.Write($"Do you want to start {distroName} distribution? [Y/n]:");
            ConsoleKeyInfo chooseToStart = Console.ReadKey();
            do
            {
                if ((chooseToStart.Key == ConsoleKey.Y) ^ (chooseToStart.Key == ConsoleKey.Enter))
                    Process.Start("wsl.exe", $"-d {distroName}");


                else if (chooseToStart.Key == ConsoleKey.N)
                    Environment.Exit(0);
            } while ((chooseToStart.Key != ConsoleKey.Y) & (chooseToStart.Key != ConsoleKey.Enter) & (chooseToStart.Key != ConsoleKey.N));


        }
    }
}
