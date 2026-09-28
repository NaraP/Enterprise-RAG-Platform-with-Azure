using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Infrastructure.TextExtraction;

public class TextExtractorFactory : ITextExtractorFactory
{
    private readonly IEnumerable<IDocumentTextExtractor> _extractors;

    public TextExtractorFactory(IEnumerable<IDocumentTextExtractor> extractors) => _extractors = extractors;

    public IDocumentTextExtractor GetExtractor(DocumentType documentType) =>
        _extractors.FirstOrDefault(e => e.CanHandle(documentType))
        ?? throw new NotSupportedException($"No text extractor registered for {documentType}");
}
