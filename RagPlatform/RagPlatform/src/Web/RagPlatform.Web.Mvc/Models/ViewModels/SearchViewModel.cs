using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Web.Mvc.Models.ViewModels;

public class SearchViewModel
{
    public string? Query { get; set; }
    public IReadOnlyList<SearchResultDto> Results { get; set; } = new List<SearchResultDto>();
}
