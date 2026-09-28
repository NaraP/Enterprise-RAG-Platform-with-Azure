using MediatR;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Step 01/02 of the pipeline: stores the file in Blob Storage, persists Document
/// metadata, then publishes an ingestion message to Service Bus.
/// </summary>
public record UploadDocumentCommand(
    Stream FileContent,
    string FileName,
    string ContentType,
    long SizeInBytes,
    Dictionary<string, string>? Metadata) : IRequest<Guid>;
