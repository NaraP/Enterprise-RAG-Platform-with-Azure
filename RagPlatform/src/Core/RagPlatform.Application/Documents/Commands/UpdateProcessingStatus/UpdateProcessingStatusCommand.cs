using MediatR;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Documents.Commands.UpdateProcessingStatus;

/// <summary>Handles messages from the "document-status" queue/topic so the Web app's
/// dashboard can reflect Worker progress without polling the Worker directly.</summary>
public record UpdateProcessingStatusCommand(Guid DocumentId, ProcessingStatus NewStatus, string? Message) : IRequest<Unit>;
