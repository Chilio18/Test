using FluentAssertions;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Enums;
using Xunit;

namespace UnameIT.RevenueIntelligence.Domain.Tests.Entities;

public class CallTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeCorrectly()
    {
        var call = Call.Create(TenantId, "Discovery Call Q3", DateTimeOffset.UtcNow, OwnerId, CallType.Discovery);

        call.Should().NotBeNull();
        call.TenantId.Should().Be(TenantId);
        call.OwnerId.Should().Be(OwnerId);
        call.Status.Should().Be(CallStatus.Uploaded);
        call.DomainEvents.Should().ContainSingle(e => e is CallCreatedEvent);
    }

    [Fact]
    public void StartProcessing_ShouldChangeStatusToProcessing()
    {
        var call = Call.Create(TenantId, "Test Call", DateTimeOffset.UtcNow, OwnerId);
        call.StartProcessing();

        call.Status.Should().Be(CallStatus.Processing);
        call.ProcessingStartedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldSetDurationAndStatus()
    {
        var call = Call.Create(TenantId, "Test Call", DateTimeOffset.UtcNow, OwnerId);
        call.StartProcessing();
        call.StartTranscribing();
        call.StartAnalyzing();
        call.Complete(1800);

        call.Status.Should().Be(CallStatus.Completed);
        call.DurationSeconds.Should().Be(1800);
        call.ProcessingCompletedAt.Should().NotBeNull();
        call.DomainEvents.Should().Contain(e => e is CallCompletedEvent);
    }

    [Fact]
    public void Fail_ShouldSetFailedStatusWithMessage()
    {
        var call = Call.Create(TenantId, "Test Call", DateTimeOffset.UtcNow, OwnerId);
        call.StartProcessing();
        call.Fail("Transcription service unavailable");

        call.Status.Should().Be(CallStatus.Failed);
        call.ProcessingErrorMessage.Should().Be("Transcription service unavailable");
    }

    [Fact]
    public void SetConsent_ShouldRecordConsentInformation()
    {
        var call = Call.Create(TenantId, "Test Call", DateTimeOffset.UtcNow, OwnerId);
        call.SetConsent(true, "Verbal consent at start of call");

        call.ConsentObtained.Should().BeTrue();
        call.ConsentMethod.Should().Be("Verbal consent at start of call");
    }
}
