using MediatR;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;

namespace foll_backend.EmergencyAnalytics.Domain.Model.Queries;

public record GetPatientFallsByMonthQuery(long ActorUserId, long PatientId, int Month, int Year) : IRequest<IEnumerable<EmergencyIncident>>;
