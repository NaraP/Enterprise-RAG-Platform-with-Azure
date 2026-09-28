using ClosedXML.Excel;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Infrastructure.TextExtraction;

public class ExcelTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(DocumentType documentType) => documentType == DocumentType.Excel;

    public Task<ExtractedContent> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook(content);
        var sb = new System.Text.StringBuilder();

        foreach (var ws in workbook.Worksheets)
        {
            sb.AppendLine($"# Sheet: {ws.Name}");
            foreach (var row in ws.RowsUsed())
                sb.AppendLine(string.Join(" | ", row.CellsUsed().Select(c => c.GetString())));
        }

        var props = new Dictionary<string, string> { ["sheetCount"] = workbook.Worksheets.Count.ToString() };
        return Task.FromResult(new ExtractedContent(sb.ToString(), props));
    }
}
