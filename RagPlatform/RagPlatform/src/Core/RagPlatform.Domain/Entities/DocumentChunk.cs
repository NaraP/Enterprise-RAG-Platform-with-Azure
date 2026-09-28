using RagPlatform.Domain.Common;

namespace RagPlatform.Domain.Entities;

/// <summary>
/// One chunk produced by the text-chunking stage. Embeddings themselves live in
/// Azure AI Search; this row is the source-of-truth metadata used for re-indexing/audit.
/// </summary>
public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public int SequenceNumber { get; private set; }
    public string Content { get; private set; } = default!;
    public int CharacterCount { get; private set; }
    public string? SectionHeading { get; private set; }
    public bool IsIndexed { get; private set; }
    public string SearchDocumentKey { get; private set; } = default!; // key used in the AI Search index

    private DocumentChunk() { }

    public DocumentChunk(Guid documentId, int sequenceNumber, string content, string? sectionHeading = null)
    {
        DocumentId = documentId;
        SequenceNumber = sequenceNumber;
        Content = content;
        CharacterCount = content.Length;
        SectionHeading = sectionHeading;
        SearchDocumentKey = $"{documentId:N}-{sequenceNumber:D5}";
    }

    public void MarkIndexed() => IsIndexed = true;
}
