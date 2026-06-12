using FluentValidation;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public class UploadCallCommandValidator : AbstractValidator<UploadCallCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "audio/mpeg", "audio/wav", "audio/mp4", "audio/ogg", "audio/webm",
        "video/mp4", "video/webm", "video/mpeg", "video/quicktime"
    ];

    public UploadCallCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.MeetingDate).NotEmpty().LessThanOrEqualTo(DateTimeOffset.UtcNow.AddHours(1));
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Unsupported file type.");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0).LessThanOrEqualTo(5L * 1024 * 1024 * 1024);
    }
}
