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
            if (!_appSettings.LocalBlobPath.StartsWith("/") || !_appSettings.LocalBlobPath.StartsWith("\\"))
            {
                // Use the home directory instead
                _appSettings.LocalBlobPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _appSettings.LocalBlobPath);
            }
        }

        public string SaveBlob(string blobPath, string blobContent)
        {
            blobPath = blobPath.Replace("\\\\", "\\").Replace("\\", "/");
            var blobName = Path.GetFileName(blobPath);
            var localFilePath = Path.Combine(_appSettings.LocalBlobPath, blobName);
            File.WriteAllText(localFilePath, blobContent);
            return localFilePath;
        }
    }
}
