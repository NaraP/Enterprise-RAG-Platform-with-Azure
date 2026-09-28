using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;
using UglyToad.PdfPig;

namespace RagPlatform.Infrastructure.TextExtraction;

public class PdfTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentType documentType) => documentType == DocumentType.Pdf;

    public Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        using var document = PdfDocument.Open(content);
        var text = string.Join("\n\n", document.GetPages().Select(p => p.Text));

        var props = new Dictionary<string, string>
        {
            ["pageCount"] = document.NumberOfPages.ToString(),
            ["title"] = document.Information.Title ?? string.Empty,
            ["author"] = document.Information.Author ?? string.Empty
        };

        return Task.FromResult(new ExtractedContent(text, props));
    }
}
