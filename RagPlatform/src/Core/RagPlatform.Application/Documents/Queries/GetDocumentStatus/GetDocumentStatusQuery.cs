using MediatR;
using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Application.Documents.Queries.GetDocumentStatus;

public record GetDocumentStatusQuery(Guid DocumentId) : IRequest<DocumentStatusDto>;
