namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    Task EnqueueAsync<T>(T job, CancellationToken ct = default) where T : class;
    Task ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken ct = default) where T : class;
    Task RecurringAsync<T>(string jobId, T job, string cronExpression) where T : class;
}
