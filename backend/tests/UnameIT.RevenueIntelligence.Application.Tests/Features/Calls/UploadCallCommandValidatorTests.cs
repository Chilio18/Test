using FluentAssertions;
using UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;
using UnameIT.RevenueIntelligence.Domain.Enums;
using Xunit;

namespace UnameIT.RevenueIntelligence.Application.Tests.Features.Calls;

public class UploadCallCommandValidatorTests
{
    private readonly UploadCallCommandValidator _validator = new();

    private static UploadCallCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(),
        "Discovery Call",
        DateTimeOffset.UtcNow.AddHours(-1),
        CallType.Discovery,
        "recording.mp4", "video/mp4",
        1024 * 1024 * 100, true, true, "Verbal",
        "nl", null, null, null);

    [Fact]
    public async Task ValidCommand_ShouldPassValidation()
    {
        var result = await _validator.ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyTitle_ShouldFailValidation()
    {
        var cmd = ValidCommand() with { Title = "" };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task UnsupportedContentType_ShouldFailValidation()
    {
        var cmd = ValidCommand() with { ContentType = "application/pdf" };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task FutureMeetingDate_ShouldFailValidation()
    {
        var cmd = ValidCommand() with { MeetingDate = DateTimeOffset.UtcNow.AddDays(1) };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }
}
