using Integrations.Storage.Inspector.Models;
using Microsoft.Extensions.Options;

namespace Integrations.Storage.Inspector.Services
{
    public class LocalBlobService
    {
        private static AppSettings _appSettings;
        public LocalBlobService(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
        }

        public string SaveBlob(string blobPath, string blobContent)
        {
            var blobName = Path.GetFileName(blobPath);
            var localFilePath = Path.Combine(_appSettings.LocalBlobPath, blobName);
            File.WriteAllText(localFilePath, blobContent);
            return localFilePath;
        }
    }
}
