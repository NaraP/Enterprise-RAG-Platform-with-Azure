using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Documents.DTOs;

public record DocumentDto(
    Guid Id,
    string FileName,
    DocumentType DocumentType,
    long SizeInBytes,
    ProcessingStatus Status,
    int ChunkCount,
    DateTimeOffset CreatedAtUtc,
    string? LastError);
