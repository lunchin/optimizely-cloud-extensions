using System.Data;
using EPiServer.Cms.Shell.UI.ContentTree.Internal;
using EPiServer.Shell.Web.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.MasterLanguage;

[Authorize(Policy = Constants.lunchinPolicy)]
public class MasterLanguageSwitcherController(IContentCacheRemover contentCacheRemover,
    IOptions<AntiforgeryOptions> antiforgeryOptions,
    ILanguageBranchRepository languageBranchRepository,
    ContentTreeLoader contentTreeLoader) : Controller
{
    private const string _spGetContentHierarchy = "lunchin_GetContentHierarchy";
    private const string _spGetContentBlocks = "lunchin_GetContentBlocks";
    private const string _spChangePageBranchMasterLanguage = "lunchin_ChangePageBranchMasterLanguage";
    private const string _baseRoute = "/api/masterlanguage/";

    private readonly IContentCacheRemover _contentCacheRemover = contentCacheRemover;
    private readonly AntiforgeryOptions _antiforgeryOptions = antiforgeryOptions.Value;
    private readonly ILanguageBranchRepository _languageBranchRepository = languageBranchRepository;
    private readonly ContentTreeLoader _contentTreeLoader = contentTreeLoader;

    public async Task<IActionResult> Index()
    {
        return await Task.FromResult(View(new MasterLanguageSwitcherViewModel
        {
            AntiforgeryOptions = _antiforgeryOptions,
        }));
    }

    [HttpGet($"{_baseRoute}gettree")]
    public ActionResult GetTreeNode(int parentId = 0)
    {
        var node = parentId == 0 ? ContentReference.RootPage : new ContentReference(parentId);
        return new JsonDataResult(_contentTreeLoader.Load<IContent>(node, [], node).Children.Select
        (xv=> new {
            key= xv.ContentLink,
            title =  xv.Name
        }));
    }

    [HttpGet($"{_baseRoute}languages")]
    public ActionResult GetLanguages()
    {
        return new JsonDataResult(_languageBranchRepository.ListEnabled());
    }

    [HttpPost($"{_baseRoute}switchmasterlanguage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchMasterLanguage([FromBody] SwitchLanguageModel model)
    {
        if (model == null || model.ContentId <= 0)
        {
            return Problem(detail: "No target page was selected", statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrEmpty(model.TargetLanguage))
        {
            return Problem(detail: "No language branch was selected", statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            await RunSwitchLanguage(model.ContentId, model.TargetLanguage, model.ProcessChildren, model.SwitchOnly);
            await RunSwitchBlockLanguage(model.ContentId, model.TargetLanguage, model.ProcessChildren, model.SwitchOnly);
            _contentCacheRemover.Clear();
            return Ok("Page language changed.");
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task RunSwitchBlockLanguage(int contentId, string langBranch, bool recursive, bool switchOnly)
    {
        var ids = new List<int>();

        foreach (var pageId in await GetContentHierarchy(contentId))
        {
            ids.AddRange(await GetContentBlocks(pageId));
        }

        foreach (var id in ids)
        {
            await RunSwitchLanguage(id, langBranch, recursive, switchOnly);
        }
    }

    private static async Task<bool> RunSwitchLanguage(int contentId, string langBranch, bool recursive, bool switchOnly)
    {
        return await DatabaseUtilities.ExecuteNonQueryAsync("EPiServerDB",
            _spChangePageBranchMasterLanguage,
            CommandType.StoredProcedure,
            [new SqlParameter("@page_id", contentId),
             new SqlParameter("@language_branch", langBranch),
             new SqlParameter("@recursive", recursive),
             new SqlParameter("@switch_only", switchOnly)]);
    }

    private static async Task<IEnumerable<int>> GetContentHierarchy(int pageId)
    {
        return await DatabaseUtilities.ExecuteReaderAsync("EPiServerDB",
            _spGetContentHierarchy,
            (reader) => int.Parse(reader["pkID"]?.ToString() ?? "0"),
            CommandType.StoredProcedure,
            [new SqlParameter("@page_id", pageId)]) ?? [];
    }

    private static async Task<IEnumerable<int>> GetContentBlocks(int pageId)
    {
        return await DatabaseUtilities.ExecuteReaderAsync("EPiServerDB",
            _spGetContentBlocks,
            (reader) => int.Parse(reader["pkID"]?.ToString() ?? "0"),
            CommandType.StoredProcedure,
            [new SqlParameter("@page_id", pageId)]) ?? [];
    }
}
