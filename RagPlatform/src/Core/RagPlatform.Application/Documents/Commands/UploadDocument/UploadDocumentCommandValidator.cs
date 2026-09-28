using FluentValidation;

namespace RagPlatform.Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".md"
    };

    private const long MaxSizeBytes = 200L * 1024 * 1024; // 200 MB

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(name => AllowedExtensions.Contains(Path.GetExtension(name)))
            .WithMessage("Unsupported file type.");

        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxSizeBytes)
            .WithMessage($"File exceeds the {MaxSizeBytes / (1024 * 1024)} MB limit.");

        RuleFor(x => x.FileContent).NotNull();
    }
}
