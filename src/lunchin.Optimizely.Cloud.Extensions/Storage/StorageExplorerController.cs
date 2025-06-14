using System.IO;
using System.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.Storage;

[Authorize(Policy = Constants.lunchinPolicy)]
public class StorageExplorerController : Controller
{
    private const string BaseRoute = "/api/storageexplorer/";
    private readonly IStorageService _storageService;
    private readonly IMimeTypeResolver _mimeTypeResolver;
    private readonly AntiforgeryOptions _antiforgeryOptions;

    public StorageExplorerController(IStorageService storageService,
        IMimeTypeResolver mimeTypeResolver,
        IOptions<AntiforgeryOptions> antiforgeryOptions)
    {
        _storageService = storageService;
        _mimeTypeResolver = mimeTypeResolver;
        _antiforgeryOptions = antiforgeryOptions.Value;
    }

    public async Task<IActionResult> Index()
    {
        return await Task.FromResult(View(new MasterLanguageSwitcherViewModel
        {
            AntiforgeryOptions = _antiforgeryOptions,
        }));
    }

    [HttpGet($"{BaseRoute}search")]
    public async Task<IActionResult> Search(string? container = null, string? path = null)
    {
        if (!_storageService.IsInitialized)
        {
            return View("Error");
        }

        var blobs = new List<AzureBlob>();
        if (container == null)
        {
            await foreach (var blob in _storageService.GetContainersAsync())
            {
                blobs.Add(blob.GetAzureBlob());
            }
            return Json(new ExplorerModel
            {
                Results = blobs,
                Container = container,
                Path = path,
                BreadCrumbs = GetBreadCrumbs(container, path)
            });
        }

        var containerClient = await _storageService.GetContainerAsync(container);
        await foreach (var blob in _storageService.GetBlobItemsAsync(container, path!))
        {
            if (containerClient == null || blob == null)
            {
                continue;
            }

            var azureBlob = blob.GetAzureBlob(containerClient);
            if (azureBlob == null)
            {
                continue;
            }

            blobs.Add(azureBlob);
        }

        return Json(new ExplorerModel
        {
            Results = blobs,
            Container = container,
            Path = path,
            BreadCrumbs = GetBreadCrumbs(container, path)
        });
    }

    [HttpDelete($"{BaseRoute}delete")]
    public async Task<IActionResult> Delete(string url)
    {
        await _storageService.DeleteAsync(url);
        return Ok();
    }

    [HttpPost($"{BaseRoute}rename")]
    public async Task<IActionResult> Rename(string url, string newName, string? container = null, string? path = null, int page = 1, int pageSize = 100)
    {
        var reference = await _storageService.GetCloudBlockBlobAsync(url);
        if (reference == null)
        {
            return new EmptyResult();
        }

        await _storageService.RenameAsync(reference, newName);
        return RedirectToAction("Index", new { path, page, container, pageSize });
    }

    [HttpGet($"{BaseRoute}download")]
    public async Task<IActionResult> Download(string url)
    {
        var reference = await _storageService.GetCloudBlockBlobAsync(url);
        if (reference == null)
        {
            return new EmptyResult();
        }

        var file = await reference.DownloadAsync();
        string filename = reference.Name[(reference.Name.LastIndexOf('/') == 0 ? 0 : reference.Name.LastIndexOf('/') + 1)..];
        var contentType = _mimeTypeResolver.GetMimeMapping(url);

        var cd = new System.Net.Mime.ContentDisposition
        {
            FileName = filename,
            Inline = true,
        };

        Response.Headers.Append("Content-Disposition", cd.ToString());

        return File(file.Value.Content, contentType);
    }

    [HttpPost($"{BaseRoute}upload")]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> UploadFiles(List<IFormFile> postedFiles, string container, string? path = null)
    {
        if (path == null)
        {
            path = string.Empty;
        }
        foreach (var postedFile in postedFiles)
        {
            if (postedFile != null)
            {
                string fileName = Path.GetFileName(postedFile.FileName);
                var bytes = new byte[postedFile.Length];
                postedFile.OpenReadStream().Read(bytes, 0, bytes.Length);
                var ms = new MemoryStream(bytes);
                await _storageService.AddAsync(container, HttpUtility.HtmlDecode(path) + fileName, ms, postedFile.Length);
                GC.Collect();
            }
        }

        return Json(new
        {
            container,
            path
        });
    }

    [HttpPost($"{BaseRoute}createcontainer")]
    [AutoValidateAntiforgeryToken]
    public async Task<ActionResult> CreateContainer(string container)
    {
        _ = await _storageService.GetContainerAsync(container.ToLower());
        return Ok();
    }

    private Dictionary<string, string> GetBreadCrumbs(string? container, string? path)
    {
        var breadCrumbs = new Dictionary<string, string>()
        {
            { "Root", "Root" }
        };

        if (string.IsNullOrEmpty(container) && string.IsNullOrEmpty(path))
        {
            return breadCrumbs;
        }

        breadCrumbs.Add(container ?? "", container ?? "");

        if (string.IsNullOrEmpty(path))
        {
            return breadCrumbs;
        }

        var links = HttpUtility.HtmlDecode(path).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < links.Length; i++)
        {
            var prefix = string.Empty;

            if (i == links.Length - 1)
            {
                breadCrumbs.Add(links[i], "");
                continue;
            }
            else if (i == 0)
            {
                prefix = links[i] + "/";
                breadCrumbs.Add(links[i], prefix ?? "");
                continue;
            }

            for (var j = 0; j < i; j++)
            {
                prefix += links[j] + "/";
            }
            prefix += links[i] + "/";

            breadCrumbs.Add(links[i], prefix ?? "");
        }
        return breadCrumbs;
    }
}
