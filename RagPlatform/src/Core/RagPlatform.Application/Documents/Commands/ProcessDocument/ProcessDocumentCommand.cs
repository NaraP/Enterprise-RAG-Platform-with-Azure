using MediatR;

namespace RagPlatform.Application.Documents.Commands.ProcessDocument;

/// <summary>
/// Steps 03-05 of the pipeline, run by RagPlatform.Worker after consuming a Service Bus
/// message: extract -> chunk -> embed -> index. Idempotent by DocumentId so redelivery
/// (at-least-once) is safe.
/// </summary>
public record ProcessDocumentCommand(Guid DocumentId, int AttemptNumber) : IRequest<Unit>;
