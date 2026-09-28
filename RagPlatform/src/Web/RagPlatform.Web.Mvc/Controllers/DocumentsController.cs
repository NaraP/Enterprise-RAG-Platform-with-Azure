using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPlatform.Application.Documents.Commands.UploadDocument;
using RagPlatform.Application.Documents.Queries.GetDocumentStatus;
using RagPlatform.Application.Documents.Queries.GetUserDocuments;
using RagPlatform.Infrastructure.Security;
using RagPlatform.Web.Mvc.Models.ViewModels;

namespace RagPlatform.Web.Mvc.Controllers;

/// <summary>Upload UI (step 01/02) and the document/status dashboard (step 06/07).</summary>
[Authorize]
public class DocumentsController : Controller
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _mediator.Send(new GetUserDocumentsQuery(page, PageSize: 20));
        return View(new DocumentListViewModel
        {
            Documents = result.Items,
            Page = result.Page,
            TotalPages = result.TotalPages
        });
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanUploadDocuments)]
    public IActionResult Upload() => View(new UploadDocumentViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.CanUploadDocuments)]
    [RequestSizeLimit(200_000_000)] // 200 MB, matches UploadDocumentCommandValidator
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid || model.File is null) return View(model);

        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(model.Tags)) metadata["tags"] = model.Tags;
        if (!string.IsNullOrWhiteSpace(model.Description)) metadata["description"] = model.Description;

        await using var stream = model.File.OpenReadStream();
        var documentId = await _mediator.Send(new UploadDocumentCommand(
            stream, model.File.FileName, model.File.ContentType, model.File.Length, metadata));

        TempData["Message"] = $"'{model.File.FileName}' uploaded and queued for processing.";
        return RedirectToAction(nameof(Status), new { id = documentId });
    }

    [HttpGet]
    public async Task<IActionResult> Status(Guid id)
    {
        var status = await _mediator.Send(new GetDocumentStatusQuery(id));
        return View(new DocumentStatusViewModel { Status = status });
    }
}
