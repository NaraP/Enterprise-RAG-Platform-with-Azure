using MediatR;
using RagPlatform.Application.Common.Models;
using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Application.Documents.Queries.GetUserDocuments;

/// <summary>Backs the dashboard grid: the caller's own documents with paging.</summary>
public record GetUserDocumentsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedList<DocumentDto>>;
