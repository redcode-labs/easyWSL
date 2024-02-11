using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using easyWslLib;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Web.Http;

namespace easyWSL
{
    internal class PlatformHelpers: IPlatformHelpers
    {
        private readonly Action<HttpProgress> _httpProgressCallback;
        public PlatformHelpers(string tarCommand, Action<HttpProgress> httpProgressCallback)
        {
            TarCommand = tarCommand;
            this._httpProgressCallback = httpProgressCallback;
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

        public async Task DownloadFileAsync(Uri uri, IEnumerable<KeyValuePair<string,string>> headers, FileInfo destinationPath)
        {
            HttpRequestHeaders _headers;

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
            };
            foreach (var header in headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>(_httpProgressCallback);
            HttpResponseMessage response = await App.httpClient.SendRequestAsync(httpRequestMessage).AsTask(progressCallback);

            StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(destinationPath.DirectoryName);
            StorageFile downloadFile = await downloadFolder.CreateFileAsync(destinationPath.Name);

            IInputStream inputStream = await response.Content.ReadAsInputStreamAsync();
            IOutputStream outputStream = await downloadFile.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAndCloseAsync(inputStream, outputStream);


            inputStream.Dispose();
            outputStream.Dispose();
        }

        public string TarCommand { get; }
    }
}
