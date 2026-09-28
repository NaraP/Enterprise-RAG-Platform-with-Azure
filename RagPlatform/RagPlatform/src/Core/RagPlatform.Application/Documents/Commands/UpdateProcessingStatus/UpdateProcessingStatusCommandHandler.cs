using MediatR;
using RagPlatform.Application.Common.Exceptions;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Application.Documents.Commands.UpdateProcessingStatus;

public class UpdateProcessingStatusCommandHandler : IRequestHandler<UpdateProcessingStatusCommand, Unit>
{
    private readonly IDocumentRepository _documents;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProcessingStatusCommandHandler(IDocumentRepository documents, IUnitOfWork unitOfWork)
    {
        _documents = documents;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateProcessingStatusCommand request, CancellationToken ct)
    {
        var document = await _documents.GetByIdAsync(request.DocumentId, ct)
            ?? throw new NotFoundException("Document", request.DocumentId);

        document.TransitionTo(request.NewStatus, request.Message);
        await _unitOfWork.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
