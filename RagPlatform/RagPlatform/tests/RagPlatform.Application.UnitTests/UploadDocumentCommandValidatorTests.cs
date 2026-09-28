using FluentAssertions;
using RagPlatform.Application.Documents.Commands.UploadDocument;
using Xunit;

namespace RagPlatform.Application.UnitTests;

public class UploadDocumentCommandValidatorTests
{
    private readonly UploadDocumentCommandValidator _validator = new();

    [Fact]
    public void Rejects_unsupported_file_extension()
    {
        var command = new UploadDocumentCommand(new MemoryStream(new byte[10]), "malware.exe", "application/octet-stream", 10, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_supported_pdf_within_size_limit()
    {
        var command = new UploadDocumentCommand(new MemoryStream(new byte[10]), "report.pdf", "application/pdf", 10, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Rejects_file_exceeding_size_limit()
    {
        var command = new UploadDocumentCommand(new MemoryStream(new byte[10]), "report.pdf", "application/pdf", 300L * 1024 * 1024, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
