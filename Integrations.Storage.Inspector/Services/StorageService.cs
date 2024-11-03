using Integrations.Storage.Inspector.Models;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;

namespace Integrations.Storage.Inspector.Services
{
    public class StorageService
    {
        private static BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _container = null;

        public StorageService(IOptions<AppSettings> options)
        {
        }

        public void InitializeConnection(Connection connection)
        {
            _blobServiceClient = new(connection.options.BlobStorageConnectionString);
        }

        public List<string> GetContainers()
        {
            List<string> list = new();
            var containers = _blobServiceClient.GetBlobContainers();
            foreach (var container in containers)
            {
                list.Add(container.Name);
            }
            return list;
        }

        public async Task<List<string>> GetBlobList(string containerName, string prefix)
        {
            List<string> list = new();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobItems = containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix:  prefix);
            await foreach (var blob in blobItems)
            {
                list.Add(blob.Name);
            }
            return list;

        }

        public async Task<string> GetBlobContent(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var blobDownLoadInfo = await blobClient.DownloadContentAsync();
            return blobDownLoadInfo.Value.Content.ToString();
        }

        public void SetContainerClient(string containerName)
        {
            _container = _blobServiceClient.GetBlobContainerClient(containerName);
        }

        public Task<List<StorageInfo>> GetDirectoryListing(string containerName, string prefix)
        {
            if (_container?.Name != containerName)
            {
                _container = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            return ListBlobsHierarchicalListing(_container, prefix, null);
        }

        private static async Task<List<StorageInfo>> ListBlobsHierarchicalListing(BlobContainerClient container, string prefix, int? segmentSize)
        {
            try
            {
                List<StorageInfo> list = [];
                var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default, segmentSize);
                await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    // A hierarchical listing may return both virtual directories and blobs.
                    foreach (BlobHierarchyItem hierarchyItem in blobPage.Values)
                    {
                        if (hierarchyItem.IsPrefix)
                        {
                            var parts = hierarchyItem.Prefix.Split('/');
                            list.Add(new StorageInfo
                            {
                                IsDirectory = true,
                                FullPath = hierarchyItem.Prefix,
                                Name = parts[^2]
                            });
                            // Call recursively with the prefix to traverse the virtual directory.
                            //await ListBlobsHierarchicalListing(container, hierarchyItem.Prefix, null);
                        }
                        else
                        {
                            var parts = hierarchyItem.Blob.Name.Split('/');
                            list.Add(new StorageInfo
                            {
                                IsDirectory = false,
                                FullPath = hierarchyItem.Blob.Name,
                                Name = parts[^1]
                            });
                        }

                    }
                }
                return list;
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
        }
    }
}
