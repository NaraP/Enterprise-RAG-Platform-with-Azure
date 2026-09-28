using MediatR;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Common.Models;
using RagPlatform.Application.Documents.DTOs;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Application.Documents.Queries.GetUserDocuments;

public class GetUserDocumentsQueryHandler : IRequestHandler<GetUserDocumentsQuery, PaginatedList<DocumentDto>>
{
    private readonly IDocumentRepository _documents;
    private readonly ICurrentUserService _currentUser;

    public GetUserDocumentsQueryHandler(IDocumentRepository documents, ICurrentUserService currentUser)
    {
        _documents = documents;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<DocumentDto>> Handle(GetUserDocumentsQuery request, CancellationToken ct)
    {
        var items = await _documents.GetByOwnerAsync(_currentUser.UserId, request.Page, request.PageSize, ct);

        var dtos = items.Select(d => new DocumentDto(
            d.Id, d.FileName, d.DocumentType, d.SizeInBytes, d.Status, d.ChunkCount, d.CreatedAtUtc, d.LastError)).ToList();

        // NOTE: total count for paging should come from a dedicated repository count method in a
        // production implementation; simplified here to items.Count for scaffold purposes.
        return new PaginatedList<DocumentDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
