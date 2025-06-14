#pragma warning disable CS8602 // Dereference of a possibly null reference.
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPiServer.Azure.Blobs;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.Storage;

public class StorageService : IStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly AzureBlobProviderOptions? _azureBlobProviderOptions;

    public StorageService(IOptions<AzureBlobProviderOptions> options)
    {
        if (options.Value != null && !string.IsNullOrEmpty(options.Value.ConnectionString))
        {
            _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
            _azureBlobProviderOptions = options.Value ?? new AzureBlobProviderOptions();
            IsInitialized = true;
        }
    }

    public string? BlobRootUrl { get; }

    public bool IsInitialized { get; }

    public async Task<BlobClient> AddAsync(string containerName, string filename, Stream stream, long length)
    {
        var blob = await GetCloudBlockBlobAsync(containerName, filename);
        var shouldUpload = true;
        if (blob.Exists())
        {
            var properties = blob.GetProperties();
            if (properties.Value?.ContentLength == length)
            {
                shouldUpload = false;
            }
        }

        if (shouldUpload)
        {
            blob.Upload(stream);
        }

        return blob;
    }

    public async IAsyncEnumerable<BlobHierarchyItem> GetBlobItemsAsync(string containerName, string path)
    {
        var container = await GetContainerAsync(containerName);
        await foreach (var item in container.GetBlobsByHierarchyAsync(delimiter: "/", prefix: path))
        {
            yield return item;
        }
    }

    public async Task<BlobClient> GetCloudBlockBlobAsync(string containerName, string path)
    {
        var container = await GetContainerAsync(containerName);
        return container.GetBlobClient(path);
    }

    public async Task<bool> ExistsAsync(string containerName, string path)
    {
        var reference = await GetCloudBlockBlobAsync(containerName, path);
        return reference.Exists();
    }

    public async Task DeleteAsync(string containerName, string path)
    {
        var reference = await GetCloudBlockBlobAsync(containerName, path);
        reference.DeleteIfExists();
    }

    public async Task DeleteAsync(string url)
    {
        var isUrl = Uri.TryCreate(url, new UriCreationOptions(), out _);
        if (!isUrl)
        {

            var containerClient = await GetContainerAsync(url);
            if (containerClient != null)
            {
                await containerClient.DeleteAsync();
                return;
            }
        }

        var blobClient = new BlobClient(new Uri(url));
        if (blobClient == null)
        {
            return;
        }

        var blobContainerClient = await GetContainerAsync(blobClient.BlobContainerName);
        if (blobContainerClient == null)
        {
            return;
        }

        var deleted = await blobContainerClient.DeleteBlobIfExistsAsync(blobClient.Name);
        if (!deleted?.Value ?? true)
        {
            await foreach (var blobItem in (Azure.AsyncPageable<BlobItem>)blobContainerClient.GetBlobsAsync(prefix: blobClient.Name))
            {
                var newBlobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                await newBlobClient.DeleteIfExistsAsync();
            }
        }
    }

    public async IAsyncEnumerable<BlobHierarchyItem> GetBlobDirectoriesAsync(string containerName, string? path = null)
    {
        var container = await GetContainerAsync(containerName);
        await foreach (var dir in container.GetBlobsByHierarchyAsync(delimiter: "/", prefix: path))
        {
            if (dir.IsPrefix)
            {
                yield return dir;
            }
        }
    }

    public async Task<BlobHierarchyItem?> GetBlobDirectoryAsync(string containerName, string path)
    {
        var container = await GetContainerAsync(containerName);
        BlobHierarchyItem returnItem;
        await foreach (var item in container.GetBlobsByHierarchyAsync(delimiter: "/", prefix: path))
        {
            if (item.IsPrefix)
            {
                returnItem = item;
            }
            break;
        }

        return null;
    }

    public async Task<BlobContainerClient?> GetContainerAsync(string containerName,
        PublicAccessType blobContainerPublicAccessType = PublicAccessType.None)
    {
        if (!IsInitialized)
        {
            return null;
        }

        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.CreateIfNotExistsAsync(blobContainerPublicAccessType);
        return blobContainerClient;
    }

    public async Task RenameAsync(BlobClient blob, string newName)
    {
        if (blob == null || !IsInitialized)
        {
            return;
        }

        var blobCopy = new BlobClient(_azureBlobProviderOptions?.ConnectionString ?? "", blob.BlobContainerName, newName);
        if (blobCopy == null || await blobCopy.ExistsAsync() || !await blob.ExistsAsync())
        {
            return;
        }

        await blobCopy.StartCopyFromUriAsync(blob.Uri);
        await blob.DeleteIfExistsAsync();
    }

    public async IAsyncEnumerable<BlobContainerClient> GetContainersAsync()
    {
        await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
        {
            yield return _blobServiceClient.GetBlobContainerClient(container.Name);
        }
    }

    public async Task<BlobClient> GetCloudBlockBlobAsync(string url)
    {
        var blobClient = new BlobClient(new Uri(url));
        var containerName = blobClient.BlobContainerName;
        var blobName = blobClient.Name;
        return await Task.FromResult(new BlobClient(_azureBlobProviderOptions?.ConnectionString, containerName, blobName));
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
