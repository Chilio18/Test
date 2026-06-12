using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Call : TenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public CallStatus Status { get; private set; } = CallStatus.Uploaded;
    public CallType Type { get; private set; }
    public DateTimeOffset MeetingDate { get; private set; }
    public int? DurationSeconds { get; private set; }
    public Guid? AccountId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string? Description { get; private set; }
    public string? ExternalMeetingId { get; private set; }
    public string? MeetingPlatform { get; private set; }
    public bool ConsentObtained { get; private set; }
    public string? ConsentMethod { get; private set; }
    public string? Language { get; private set; } = "nl";
    public string? ProcessingErrorMessage { get; private set; }
    public DateTimeOffset? ProcessingStartedAt { get; private set; }
    public DateTimeOffset? ProcessingCompletedAt { get; private set; }
    public string? MetadataJson { get; private set; }

    public Account? Account { get; private set; }
    public Opportunity? Opportunity { get; private set; }
    public User? Owner { get; private set; }

    private readonly List<Recording> _recordings = new();
    public IReadOnlyCollection<Recording> Recordings => _recordings.AsReadOnly();

    private readonly List<Transcript> _transcripts = new();
    public IReadOnlyCollection<Transcript> Transcripts => _transcripts.AsReadOnly();

    private readonly List<CallInsight> _insights = new();
    public IReadOnlyCollection<CallInsight> Insights => _insights.AsReadOnly();

    private readonly List<ActionItem> _actionItems = new();
    public IReadOnlyCollection<ActionItem> ActionItems => _actionItems.AsReadOnly();

    private readonly List<CallScorecard> _scorecards = new();
    public IReadOnlyCollection<CallScorecard> Scorecards => _scorecards.AsReadOnly();

    private Call() : base() { }

    public static Call Create(Guid tenantId, string title, DateTimeOffset meetingDate,
        Guid ownerId, CallType type = CallType.Other)
    {
        var call = new Call
        {
            TenantId = tenantId,
            Title = title,
            MeetingDate = meetingDate,
            OwnerId = ownerId,
            Type = type
        };
        call.AddDomainEvent(new CallCreatedEvent(call.Id, tenantId, ownerId));
        return call;
    }

    public void StartProcessing()
    {
        Status = CallStatus.Processing;
        ProcessingStartedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CallStatusChangedEvent(Id, TenantId, CallStatus.Uploaded, CallStatus.Processing));
    }

    public void StartTranscribing()
    {
        Status = CallStatus.Transcribing;
        AddDomainEvent(new CallStatusChangedEvent(Id, TenantId, CallStatus.Processing, CallStatus.Transcribing));
    }

    public void StartAnalyzing()
    {
        Status = CallStatus.Analyzing;
        AddDomainEvent(new CallStatusChangedEvent(Id, TenantId, CallStatus.Transcribing, CallStatus.Analyzing));
    }

    public void Complete(int durationSeconds)
    {
        Status = CallStatus.Completed;
        DurationSeconds = durationSeconds;
        ProcessingCompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CallCompletedEvent(Id, TenantId, OwnerId));
    }

    public void Fail(string errorMessage)
    {
        Status = CallStatus.Failed;
        ProcessingErrorMessage = errorMessage;
        ProcessingCompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CallFailedEvent(Id, TenantId, errorMessage));
    }

    public void SetConsent(bool obtained, string? method)
    {
        ConsentObtained = obtained;
        ConsentMethod = method;
    }

    public void LinkToOpportunity(Guid opportunityId)
    {
        OpportunityId = opportunityId;
    }
}
