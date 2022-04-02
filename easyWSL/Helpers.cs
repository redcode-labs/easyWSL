using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace easyWSL
{
    class Helpers
    {
        public string GetRequest(string url)
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
        public string GetRequestWithHeader(string url, string token, string type)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Accept = type;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                throw ex;
            }
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string responseStream = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
            return responseStream;
        }

        public string ComputeSha256Hash(byte[] rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(rawData);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            string destinationFolderPath = sourcePath.Substring(0, sourcePath.LastIndexOf(@"\"));
            string destinationFileName = Path.GetFileName(destinationPath);

            StorageFile sourceFile = await StorageFile.GetFileFromPathAsync(sourcePath);
            StorageFolder destinationFolder = await StorageFolder.GetFolderFromPathAsync(destinationFolderPath);
            StorageFile destinationFile = await destinationFolder.CreateFileAsync(destinationFileName);

            IInputStream inputStream = await sourceFile.OpenAsync(FileAccessMode.Read);
            IOutputStream outputStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite);

            await RandomAccessStream.CopyAndCloseAsync(inputStream, outputStream); 
            inputStream.Dispose();
            outputStream.Dispose();
        }
        public async Task ExecuteProcessAsynch(string exe, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = exe;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = arguments;
            proc.Start();
            await proc.WaitForExitAsync().ConfigureAwait(false);
        }

        public async Task StartWSLDistroAsync(string distroName)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "wsl.exe";
            proc.StartInfo.Arguments = $"-d {distroName}";
            proc.Start();
        }
        public async Task ExecuteCommandInWSLAsync(string distroName, string command)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "wsl.exe";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = $"-d {distroName} -- {command}";
            proc.Start();
            await proc.WaitForExitAsync().ConfigureAwait(false);
        }

        public string ExecuteProcessAndGetOutputAsynch(string exe, string arguments)
        {
            using Process process = Process.Start(new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = exe,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            StringBuilder outputBuilder = new();
            process.OutputDataReceived += (_, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    outputBuilder.Append(eventArgs.Data);
            };
            process.BeginOutputReadLine();
            process.WaitForExit();
            return outputBuilder.ToString();
        }
        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;

            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

    }
}
