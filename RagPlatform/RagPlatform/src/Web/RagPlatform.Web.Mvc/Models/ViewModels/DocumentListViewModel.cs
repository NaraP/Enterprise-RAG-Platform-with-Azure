using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Web.Mvc.Models.ViewModels;

public class DocumentListViewModel
{
    public IReadOnlyList<DocumentDto> Documents { get; init; } = new List<DocumentDto>();
    public int Page { get; init; }
    public int TotalPages { get; init; }
}
