using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace easyWSL
{
    class DockerPull
    {

        public class TokenFromResponse
        {
            public string token { get; set; }
            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string issued_at { get; set; }
            
        }

        public static void PullImage(string image)
        {
            Random random = new Random();
            string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            string GetRequest(string url)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                request.Credentials = CredentialCache.DefaultCredentials;
                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);
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
                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);
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
                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);
                Stream receiveStream = response.GetResponseStream();
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;

                FileStream fileStream = File.Create(fileName);
                while ((bytesRead = receiveStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
                response.Close();
                fileStream.Close();
            }

            List<string> layers = new List<string>();

            string repo = "library";

            string[] tmp = image.Split(':');
            string img = tmp[0];
            string tag = tmp[1];

            string repository = $"{repo}/{img}";
            string registry = "registry-1.docker.io";

            string auth_url = "https://auth.docker.io/token";
            string reg_service = "registry.docker.io";
                     
                             
            /*string GetAuthHead(string type)
            {
               
                dynamic tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>(GetRequest($"{auth_url}?service={reg_service}&scope=repository:{repository}:pull"));
                //Console.WriteLine(tokenFromResponse.token);
                //Console.WriteLine(" ");
                string jsonString = $"{{\"Authorization\": \"Bearer {tokenFromResponse.token}\", \"Accept\": \"{type}\"}}";
                //string jsonString = "{'Authorization': 'Bearer " + tokenFromResponse.token + "', 'Accept': '" + type + "'}";
                return jsonString;
            }
            */

            dynamic tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>(GetRequest($"{auth_url}?service={reg_service}&scope=repository:{repository}:pull"));
            //string auth_head = GetAuthHead("application/vnd.docker.distribution.manifest.v2+json");
            //Console.WriteLine(auth_head);
            string resp = GetRequestWithHeader($"https://{registry}/v2/{repository}/manifests/{tag}", tokenFromResponse.token, "application/vnd.docker.distribution.manifest.v2+json");
            Console.WriteLine(resp);

            string pattern = "(/\\b\\w*sha256\\w*\b/i)";

            foreach (Match match in Regex.Matches(resp, pattern, RegexOptions.IgnoreCase))
                Console.WriteLine("{0} (duplicates '{1}') at position {2}",
                                  match.Value, match.Groups[1].Value, match.Index);

            layers.Add("sha256:da7391352a9bb76b292a568c066aa4c3cbae8d494e6a3c68e3c596d34f7c75f8");
            layers.Add("sha256:14428a6d4bcdba49a64127900a0691fb00a3f329aced25eb77e3b65646638f8d");
            layers.Add("sha256:2c2d948710f21ad82dce71743b1654b45acb5c059cf5c19da491582cef6f2601");

            foreach (string layer in layers)
            {
                tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>(GetRequest($"{auth_url}?service={reg_service}&scope=repository:{repository}:pull"));

                string layerName = layer.Remove(0, 7) + ".tar";
                Console.WriteLine(layerName);
                
                GetRequestWithHeaderToFile($"https://{registry}/v2/{repository}/blobs/{layer}", tokenFromResponse.token, "application/vnd.docker.distribution.manifest.v2+json", layerName);

            }

           






        }
    }
}
