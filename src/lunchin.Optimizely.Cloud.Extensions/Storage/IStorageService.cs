using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace lunchin.Optimizely.Cloud.Extensions.Storage;

public interface IStorageService
{
    string? BlobRootUrl { get; }
    bool IsInitialized { get; }
    IAsyncEnumerable<BlobHierarchyItem> GetBlobItemsAsync(string containerName, string path);
    IAsyncEnumerable<BlobHierarchyItem> GetBlobDirectoriesAsync(string containerName, string? path = null);
    Task<BlobHierarchyItem?> GetBlobDirectoryAsync(string containerName, string path);
    Task<BlobContainerClient?> GetContainerAsync(string containerName,
        PublicAccessType blobContainerPublicAccessType = PublicAccessType.None);
    IAsyncEnumerable<BlobContainerClient> GetContainersAsync();
    Task<BlobClient> GetCloudBlockBlobAsync(string containerName, string path);
    Task<BlobClient> GetCloudBlockBlobAsync(string url);
    Task<BlobClient> AddAsync(string containerName, string filename, Stream stream, long length);
    Task DeleteAsync(string containerName, string path);
    Task DeleteAsync(string url);
    Task RenameAsync(BlobClient blob, string newName);
    Task<bool> ExistsAsync(string containerName, string path);
}
