using MediatR;
using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Application.Documents.Queries.SearchDocuments;

/// <summary>
/// Step 06 of the pipeline: hybrid (vector + keyword + semantic) search against the
/// caller's tenant index, filtered to documents the caller is permitted to see.
/// </summary>
public record SearchDocumentsQuery(string QueryText, int Top = 10) : IRequest<IReadOnlyList<SearchResultDto>>;
