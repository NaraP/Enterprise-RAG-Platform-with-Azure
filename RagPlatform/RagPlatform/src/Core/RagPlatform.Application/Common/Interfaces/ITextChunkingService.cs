namespace RagPlatform.Application.Common.Interfaces;

public record TextChunk(int SequenceNumber, string Content, string? SectionHeading);

public interface ITextChunkingService
{
    /// <summary>Splits text into overlapping chunks, preserving section headings where detectable.</summary>
    IReadOnlyList<TextChunk> Chunk(string fullText, int chunkSize = 1000, int chunkOverlap = 150);
}
