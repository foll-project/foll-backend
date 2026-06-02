using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Queries;

namespace foll_backend.EmergencyAnalytics.Domain.Services;

public interface IEmergencyIncidentQueryService
{
    Task<EmergencyIncident?> Handle(GetActiveFallIncidentByPatientIdQuery query);
    Task<EmergencyIncident?> Handle(GetEmergencyIncidentByIdQuery query);
    Task<IReadOnlyCollection<EmergencyIncident>> Handle(ListFallIncidentHistoryByPatientIdQuery query);
}
