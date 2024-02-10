using easyWslLib;

namespace easyWslCmd
{
    internal class PlatformHelpers:IPlatformHelpers
    {
        private void Copy(Stream inStream, FileInfo destination)
        {
            if (!destination.Directory.Exists)
            {
                destination.Directory.Create();
            }
            using var outStream = destination.OpenWrite();
            inStream.CopyTo(outStream);
        }
        public Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using var inStream = File.OpenRead(sourcePath);
            Copy(inStream, new FileInfo(destinationPath));
            return Task.CompletedTask;
        }

        public async Task DownloadFileAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> headers, FileInfo destinationPath)
        {
            var httpClient = new HttpClient();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            await using var inStream = await httpClient.GetStreamAsync(uri);
            Copy(inStream, destinationPath);
        }

        public string TarCommand => "tar.exe";
    }
}
