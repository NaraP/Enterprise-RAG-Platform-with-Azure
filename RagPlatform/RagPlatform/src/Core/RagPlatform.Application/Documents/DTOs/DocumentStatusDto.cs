using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Documents.DTOs;

public record ProcessingHistoryDto(ProcessingStatus Status, string? Message, DateTimeOffset TimestampUtc);

public record DocumentStatusDto(
    Guid DocumentId,
    string FileName,
    ProcessingStatus Status,
    int ProcessingAttempts,
    string? LastError,
    IReadOnlyList<ProcessingHistoryDto> History);
