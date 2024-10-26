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

        public Task<List<string>> GetDirectoryListing(string containerName, string prefix)
        {
            
            if (_container?.Name != containerName)
            {
                _container = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            return ListBlobsHierarchicalListing(_container, prefix, null);
        }

        private static async Task<List<string>> ListBlobsHierarchicalListing(BlobContainerClient container, string prefix, int? segmentSize)
        {
            try
            {
                List<string> list = new();
                // Call the listing operation and return pages of the specified size.
                var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default, segmentSize);

                // Enumerate the blobs returned for each page.
                await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    // A hierarchical listing may return both virtual directories and blobs.
                    foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                    {
                        if (blobhierarchyItem.IsPrefix)
                        {
                            // Write out the prefix of the virtual directory.
                            //Console.WriteLine("Virtual directory prefix: {0}", blobhierarchyItem.Prefix);
                            list.Add(blobhierarchyItem.Prefix);
                            // Call recursively with the prefix to traverse the virtual directory.
                            //await ListBlobsHierarchicalListing(container, blobhierarchyItem.Prefix, null);
                        }
                        else
                        {
                            // Write out the name of the blob.
                            //Console.WriteLine("Blob name: {0}", blobhierarchyItem.Blob.Name);
                            list.Add(blobhierarchyItem.Blob.Name);
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
