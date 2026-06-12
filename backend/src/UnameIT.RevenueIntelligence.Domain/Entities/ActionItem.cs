using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class ActionItem : TenantEntity
{
    public Guid CallId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public ActionItemStatus Status { get; private set; } = ActionItemStatus.Open;
    public int Priority { get; private set; } = 2;
    public bool IsCrmSynced { get; private set; }
    public string? CrmTaskId { get; private set; }
    public DateTimeOffset? CrmSyncedAt { get; private set; }
    public string? Source { get; private set; } = "AI";

    public Call? Call { get; private set; }
    public User? Assignee { get; private set; }

    private ActionItem() : base() { }

    public static ActionItem Create(Guid tenantId, Guid callId, string title,
        Guid? assigneeId = null, DateTimeOffset? dueDate = null)
    {
        return new ActionItem
        {
            TenantId = tenantId,
            CallId = callId,
            Title = title,
            AssigneeId = assigneeId,
            DueDate = dueDate
        };
    }

    public void Complete() => Status = ActionItemStatus.Completed;
    public void Dismiss() => Status = ActionItemStatus.Dismissed;
    public void StartProgress() => Status = ActionItemStatus.InProgress;

    public void MarkCrmSynced(string crmTaskId)
    {
        IsCrmSynced = true;
        CrmTaskId = crmTaskId;
        CrmSyncedAt = DateTimeOffset.UtcNow;
    }
}
