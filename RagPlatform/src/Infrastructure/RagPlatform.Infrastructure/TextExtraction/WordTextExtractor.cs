using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;
using DocumentType = RagPlatform.Domain.Enums.DocumentType;

namespace RagPlatform.Infrastructure.TextExtraction;

public class WordTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentType documentType) => documentType == DocumentType.Word;


    public Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        using var wordDoc = WordprocessingDocument.Open(content, false);
        var body = wordDoc.MainDocumentPart?.Document.Body;
        var text = body is null ? string.Empty : string.Join("\n", body.Descendants<Paragraph>().Select(p => p.InnerText));

        var props = new Dictionary<string, string>
        {
            ["title"] = wordDoc.PackageProperties.Title ?? string.Empty,
            ["author"] = wordDoc.PackageProperties.Creator ?? string.Empty
        };

        return Task.FromResult(new ExtractedContent(text, props));
    }
}
