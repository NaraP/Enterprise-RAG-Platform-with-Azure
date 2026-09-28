using DocumentFormat.OpenXml.Packaging;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Infrastructure.TextExtraction;

public class PowerPointTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentType documentType) => documentType == DocumentType.PowerPoint;

    public Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        using var presentation = PresentationDocument.Open(content, false);
        var sb = new System.Text.StringBuilder();
        var slideParts = presentation.PresentationPart?.SlideParts ?? Enumerable.Empty<SlidePart>();

        var slideNumber = 1;
        foreach (var slidePart in slideParts)
        {
            sb.AppendLine($"# Slide {slideNumber++}");
            var texts = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>().Select(t => t.Text);
            sb.AppendLine(string.Join(" ", texts));
        }

        var props = new Dictionary<string, string> { ["slideCount"] = (slideNumber - 1).ToString() };
        return Task.FromResult(new ExtractedContent(sb.ToString(), props));
    }
}
