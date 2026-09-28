using System.Text.RegularExpressions;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.TextExtraction;

/// <summary>
/// Splits extracted text into overlapping character-based chunks while trying to break on
/// paragraph/sentence boundaries and preserving any "# Heading" markers produced by the
/// extractors above, so retrieved chunks stay coherent and traceable to their section.
/// </summary>
public class TextChunkingService : ITextChunkingService
{
    private static readonly Regex HeadingRegex = new(@"^#\s*(.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

    public IReadOnlyList<TextChunk> Chunk(string fullText, int chunkSize = 1000, int chunkOverlap = 150)
    {
        if (string.IsNullOrWhiteSpace(fullText)) return Array.Empty<TextChunk>();

        var chunks = new List<TextChunk>();
        var currentHeading = (string?)null;
        var position = 0;
        var sequence = 0;

        while (position < fullText.Length)
        {
            var length = Math.Min(chunkSize, fullText.Length - position);
            var window = fullText.Substring(position, length);

            // prefer to end on a paragraph or sentence boundary rather than mid-word
            if (position + length < fullText.Length)
            {
                var lastBreak = window.LastIndexOfAny(new[] { '\n', '.', '!', '?' });
                if (lastBreak > chunkSize / 2) length = lastBreak + 1;
            }

            var chunkText = fullText.Substring(position, length).Trim();
            var headingMatch = HeadingRegex.Match(chunkText);
            if (headingMatch.Success) currentHeading = headingMatch.Groups[1].Value.Trim();

            if (chunkText.Length > 0)
                chunks.Add(new TextChunk(sequence++, chunkText, currentHeading));

            position += Math.Max(1, length - chunkOverlap);
        }

        return chunks;
    }
}
