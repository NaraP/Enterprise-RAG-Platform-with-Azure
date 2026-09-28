using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPlatform.Application.Documents.Queries.SearchDocuments;
using RagPlatform.Web.Mvc.Models.ViewModels;

namespace RagPlatform.Web.Mvc.Controllers;

/// <summary>Step 06: hybrid search UI over the tenant's Azure AI Search index.</summary>
[Authorize]
public class SearchController : Controller
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var vm = new SearchViewModel { Query = q };

        if (!string.IsNullOrWhiteSpace(q))
            vm.Results = await _mediator.Send(new SearchDocumentsQuery(q));

        return View(vm);
    }
}
