﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

namespace easyWSL
{
    class DistroInstaller
    {
        public class autorizationResponse
        {
            public string token { get; set; }
            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string issued_at { get; set; }

        }


        public static void InstallDistro(string distroImage, string distroName, string distroPath, string easyWSLDataDirectory, string easyWSLDirectory, bool isCustomImageSpecified)
        {

            void StartProcessSilently(string processName, string processArguments)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = processName;
                psi.UseShellExecute = false;
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

            string GetRequestWithHeader(string url, string token, string type)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("Authorization", "Bearer " + token);
                request.Accept = type;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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
                while ((bytesRead = receiveStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }

                response.Close();
                fileStream.Close();
            }


            dynamic sources = JsonSerializer.Deserialize<Sources>(File.ReadAllText("sources.json"));
            string repository = "", tag = "", registry = "registry-1.docker.io", authorizationUrl = "https://auth.docker.io/token", registryUrl = "registry.docker.io";


            if (distroImage.Contains('/'))
            {
                string[] imageArray = distroImage.Split('/');
                tag = "latest";
                repository = distroImage;
            }

            else
            {
                string[] imageArray = distroImage.Split(':');
                string imgage = imageArray[0];
                tag = imageArray[1];
                repository = $"library/{imgage}";
            }

            dynamic autorizationResponse = JsonSerializer.Deserialize<autorizationResponse>(GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));

            string layersResponse = GetRequestWithHeader($"https://{registry}/v2/{repository}/manifests/{tag}", autorizationResponse.token, "application/vnd.docker.distribution.manifest.v2+json");

            MatchCollection layersRegex = Regex.Matches(layersResponse, @"sha256:\w{64}");
            var layersList = layersRegex.Cast<Match>().Select(match => match.Value).ToList();
            layersList.RemoveAt(0);

            string layersDirectory = $"{easyWSLDataDirectory}\\layers";
            Directory.CreateDirectory(layersDirectory);

            string concatTarCommand = $" cf {layersDirectory}\\{distroName}-install.tar";

            int count = 0;
            foreach (string layer in layersList)
            {
                count++;
                Console.WriteLine($"Downloading {count}. layer ...");

                autorizationResponse = JsonSerializer.Deserialize<autorizationResponse>(GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));

                string layerName = $"{distroName}-layer{count}.tar.bz";
                string layerPath = $"{layersDirectory}\\{layerName}";

                GetRequestWithHeaderToFile($"https://{registry}/v2/{repository}/blobs/{layer}", autorizationResponse.token, "application/vnd.docker.distribution.manifest.v2+json", layerPath);
                concatTarCommand += $" @{layerPath} ";
            }
            

            Console.WriteLine("Creating install.tar file ...");
            if (layersList.Count == 1)
            {
                File.Move($"{layersDirectory}\\{distroName}-layer1.tar.bz", $"{layersDirectory}\\{distroName}-install.tar.bz");

                Console.WriteLine("Registering the distro ...");
                StartProcessSilently("wsl.exe", $"--import {distroName} {distroPath} {easyWSLDataDirectory}\\layers\\{distroName}-install.tar.bz");
            }
            else
            {
                StartProcessSilently($"{easyWSLDirectory}\\dep\\bsdtar.exe", concatTarCommand);

                Console.WriteLine("Registering the distro ...");
                StartProcessSilently("wsl.exe", $"--import {distroName} {distroPath} {easyWSLDataDirectory}\\layers\\{distroName}-install.tar");
            }


            Console.WriteLine("Cleaning up ...");
            Directory.Delete(layersDirectory, true);

            if (isCustomImageSpecified == false)
            {
                string postInstallPathWindows = $"{easyWSLDirectory}\\post-install.sh";
                string postInstallPathLinux = postInstallPathWindows.Replace(@"\", "/");
                char windowsDriveLetter = Char.ToLower(postInstallPathLinux[0]);
                postInstallPathLinux = postInstallPathLinux.Remove(0, 2);
                postInstallPathLinux = $"/mnt/{windowsDriveLetter}{postInstallPathLinux}";


                StartProcessSilently("wsl.exe", $"-d {distroName} \"{postInstallPathLinux}\"");
                StartProcessSilently("wsl.exe", $"-t {distroName}");
                StartProcessSilently("wsl.exe", $"-d {distroName}");
            }

            else
                StartProcessSilently("wsl.exe", $"-d {distroName}");
        }
    }
}
