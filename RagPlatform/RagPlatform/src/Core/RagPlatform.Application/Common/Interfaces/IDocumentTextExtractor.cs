using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Common.Interfaces;

public record ExtractedContent(string FullText, Dictionary<string, string> Properties);

public interface IDocumentTextExtractor
{
    bool CanHandle(DocumentType documentType);
    Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default);
}

public interface ITextExtractorFactory
{
    IDocumentTextExtractor GetExtractor(DocumentType documentType);
}
