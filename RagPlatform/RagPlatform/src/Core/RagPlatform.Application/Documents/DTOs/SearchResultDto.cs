namespace RagPlatform.Application.Documents.DTOs;

public record SearchResultDto(
    Guid DocumentId,
    string FileName,
    int SequenceNumber,
    string ContentSnippet,
    double Score);
