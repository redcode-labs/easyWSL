using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;


namespace easyWSL
{
    class DockerPull
    {

        public class TokenFromResponse
        {
            public string token { get; set; }
        }

        public class AuthHead
        {
            public string Authorization { get; set; }
            public string Accept { get; set; }
        }

        public static void PullImage(string image)
        {
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

            string repo = "library";

            string[] tmp = image.Split(':');
            string img = tmp[0];
            string tag = tmp[1];

            string repository = $"{repo}/{img}";
            string registry = "registry-1.docker.io";

            string auth_url = "https://auth.docker.io/token";
            string reg_service = "registry.docker.io";

           
            

            


            string GetAuthHead(string type)
            {
                AuthHead authHead = new AuthHead();
                dynamic tokenFromResponse = JsonSerializer.Deserialize<TokenFromResponse>((GetRequest($"{auth_url}?service={reg_service}&scope=repository:{repository}:pull")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                authHead.Authorization = $"Bearer {tokenFromResponse.token}";
                authHead.Accept = type;

                string jsonString = JsonSerializer.Serialize(authHead);

                return jsonString;
            }

            string auth_head = GetAuthHead("application/vnd.docker.distribution.manifest.v2+json");
            Console.WriteLine(GetAuthHead(auth_head));





        }
    }
}
