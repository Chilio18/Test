using MediatR;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public record ProcessCallCommand(Guid CallId, Guid TenantId, Guid RecordingId) : IRequest<bool>;
