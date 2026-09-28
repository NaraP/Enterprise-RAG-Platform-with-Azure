using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Infrastructure.TextExtraction;

public class PlainTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentType documentType) =>
        documentType is DocumentType.PlainText or DocumentType.Markdown or DocumentType.Other;

    public async Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        using var reader = new StreamReader(content);
        var text = await reader.ReadToEndAsync(ct);
        return new ExtractedContent(text, new Dictionary<string, string>());
    }
}
