using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace easyWslLib
{
    public interface IPlatformHelpers
    {
        public Task CopyFileAsync(string sourcePath, string destinationPath);

        public Task DownloadFileAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> headers,
            FileInfo destinationPath);

        public string TarCommand { get; }
    }
}
