using FluentAssertions;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Enums;
using Xunit;

namespace RagPlatform.Domain.UnitTests;

public class DocumentTests
{
    private static Document CreateDocument() => new(
        fileName: "invoice.pdf",
        storagePath: "tenant/user/2026/09/abc-invoice.pdf",
        containerName: "documents",
        documentType: DocumentType.Pdf,
        sizeInBytes: 1024,
        contentHash: "deadbeef",
        tenantId: Guid.NewGuid(),
        ownerUserId: Guid.NewGuid());

    [Fact]
    public void New_document_starts_in_uploaded_status_with_owner_permission()
    {
        var document = CreateDocument();

        document.Status.Should().Be(ProcessingStatus.Uploaded);
        document.Permissions.Should().ContainSingle(p => p.UserId == document.OwnerUserId && p.AccessLevel == AccessLevel.Owner);
        document.History.Should().ContainSingle(h => h.Status == ProcessingStatus.Uploaded);
    }

    [Fact]
    public void CompleteIndexing_sets_status_completed_and_records_chunk_count()
    {
        var document = CreateDocument();

        document.CompleteIndexing("tenant-index", chunkCount: 42);

        document.Status.Should().Be(ProcessingStatus.Completed);
        document.ChunkCount.Should().Be(42);
        document.SearchIndexName.Should().Be("tenant-index");
    }

    [Fact]
    public void MarkFailed_increments_attempts_and_records_error()
    {
        var document = CreateDocument();

        document.MarkFailed("blob not found");

        document.Status.Should().Be(ProcessingStatus.Failed);
        document.ProcessingAttempts.Should().Be(1);
        document.LastError.Should().Be("blob not found");
    }

    [Fact]
    public void Owner_can_always_access_their_own_document()
    {
        var document = CreateDocument();

        document.CanBeAccessedBy(document.OwnerUserId, Array.Empty<string>()).Should().BeTrue();
    }

    [Fact]
    public void Unrelated_user_without_permission_cannot_access_document()
    {
        var document = CreateDocument();

        document.CanBeAccessedBy(Guid.NewGuid(), Array.Empty<string>()).Should().BeFalse();
    }
}
