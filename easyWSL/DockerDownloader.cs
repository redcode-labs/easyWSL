using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace easyWSL
{

    internal class DockerDownloader
    {
        private Helpers helpers = new();
        
        private List<string> layersPaths = new();

        private static string tmpDirectory = App.tmpDirectory.Path;


        public class autorizationResponse
        {
            public string token { get; set; }
            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string issued_at { get; set; }

        }

        public class DockerException : Exception
        {
            public DockerException()
            {
            }
        }

            public async Task DownloadImage(string distroImage, Action<HttpProgress> httpProgressCallback)
            {

            DirectoryInfo tmpDirectoryInfo = new DirectoryInfo(tmpDirectory);
            foreach (FileInfo file in tmpDirectoryInfo.EnumerateFiles())
            {
                file.Delete();
            }


            string repository = "";
            string tag = "";
            string registry = "registry-1.docker.io";
            string authorizationUrl = "https://auth.docker.io/token";
            string registryUrl = "registry.docker.io";

            if (distroImage.Contains('/'))
            {
                string[] imageArray = distroImage.Split('/');
                if (imageArray.Length < 2)
                {
                    throw (new DockerException());
                }

                if (imageArray[1].Contains(':'))
                {
                    tag = imageArray[1].Split(':')[1];
                    repository = distroImage.Split(':')[0];
                }
                else
                {
                    tag = "latest";
                    repository = distroImage;
                }
            }
            else
            {
                string[] imageArray = distroImage.Split(':');
                if (imageArray.Length < 2)
                {
                    throw (new DockerException());
                }
                string imgage = imageArray[0]; 
                tag = imageArray[1];
                repository = $"library/{imgage}";
            }

            dynamic autorizationResponse = JsonSerializer.Deserialize<autorizationResponse>(helpers.GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));
            string layersResponse;
            try
            {
                layersResponse = helpers.GetRequestWithHeader($"https://{registry}/v2/{repository}/manifests/{tag}", autorizationResponse.token, "application/vnd.docker.distribution.manifest.v2+json");
            }
            catch (WebException ex)
            {
                throw (new DockerException());
            }

            MatchCollection layersRegex = Regex.Matches(layersResponse, @"sha256:\w{64}");
            var layersList = layersRegex.Cast<Match>().Select(match => match.Value).ToList();
            layersList.RemoveAt(0);

            MatchCollection layersSizeRegex = Regex.Matches(layersResponse, @"""size"": \d*");
            var layersSizeList = layersSizeRegex.Cast<Match>().Select(match => Convert.ToInt32(match.Value.Remove(0, 8))).ToList();

            
            Trace.WriteLine(tmpDirectory);
            Directory.CreateDirectory(tmpDirectory);

            int layersCount = 0;
            foreach (string layer in layersList)
            {
                layersCount++;

                autorizationResponse = JsonSerializer.Deserialize<autorizationResponse>(helpers.GetRequest($"{authorizationUrl}?service={registryUrl}&scope=repository:{repository}:pull"));

                string layerName = $"layer{layersCount}.tar.bz";
                string layerPath = $"{tmpDirectory}\\{layerName}";

                layersPaths.Add(layerPath);

                Progress<HttpProgress> progressCallback = new Progress<HttpProgress>(httpProgressCallback);
                var tokenSource = new CancellationTokenSource();
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{registry}/v2/{repository}/blobs/{layer}"),
                    Headers = {
                        { HttpRequestHeader.Authorization.ToString(), $"Bearer {autorizationResponse.token}" },
                        { HttpRequestHeader.Accept.ToString(), "application/vnd.docker.distribution.manifest.v2+json" },
                    },
                };

                HttpResponseMessage response = await App.httpClient.SendRequestAsync(httpRequestMessage).AsTask(tokenSource.Token, progressCallback);

                StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(tmpDirectory);
                StorageFile downloadFile = await downloadFolder.CreateFileAsync(layerName);

                IInputStream inputStream = await response.Content.ReadAsInputStreamAsync();
                IOutputStream outputStream = await downloadFile.OpenAsync(FileAccessMode.ReadWrite);
                await RandomAccessStream.CopyAndCloseAsync(inputStream, outputStream);

                
                inputStream.Dispose();
                outputStream.Dispose();
            }
        }

        public async Task CombineLayers()
        {
            var installTarPath = Path.Combine(App.tmpDirectory.Path, "install.tar.bz");

            if (layersPaths.Count == 1)
            {
                await helpers.CopyFileAsync(layersPaths[0], installTarPath);
            }
            else
            {
                string concatTarCommand = $" cf {installTarPath}";
                foreach (var layerPath in layersPaths)
                {
                    concatTarCommand += $" @{layerPath}";
                }
                Trace.WriteLine(concatTarCommand);
                await helpers.ExecuteProcessAsynch(Path.Combine(App.executableLocation, @"dep\bsdtar.exe"), concatTarCommand);
                Trace.WriteLine("combining completed");
            }
        }
    }
}
