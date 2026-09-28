using MediatR;
using RagPlatform.Application.Common.Exceptions;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Documents.DTOs;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Application.Documents.Queries.GetDocumentStatus;

public class GetDocumentStatusQueryHandler : IRequestHandler<GetDocumentStatusQuery, DocumentStatusDto>
{
    private readonly IDocumentRepository _documents;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentStatusQueryHandler(IDocumentRepository documents, ICurrentUserService currentUser)
    {
        _documents = documents;
        _currentUser = currentUser;
    }

    public async Task<DocumentStatusDto> Handle(GetDocumentStatusQuery request, CancellationToken ct)
    {
        var document = await _documents.GetByIdAsync(request.DocumentId, ct)
            ?? throw new NotFoundException("Document", request.DocumentId);

        if (!document.CanBeAccessedBy(_currentUser.UserId, _currentUser.Roles))
            throw new ForbiddenAccessException();

        return new DocumentStatusDto(
            document.Id,
            document.FileName,
            document.Status,
            document.ProcessingAttempts,
            document.LastError,
            document.History
                .OrderBy(h => h.CreatedAtUtc)
                .Select(h => new ProcessingHistoryDto(h.Status, h.Message, h.CreatedAtUtc))
                .ToList());
    }
}
