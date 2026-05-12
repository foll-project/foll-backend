using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Queries;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.EmergencyAnalytics.Domain.Services;

namespace foll_backend.EmergencyAnalytics.Application.Internal.QueryServices;

public class EmergencyIncidentQueryService : IEmergencyIncidentQueryService
{
    private readonly IEmergencyIncidentRepository _incidentRepository;
    private readonly IPatientIncidentAccessService _patientIncidentAccessService;

    public EmergencyIncidentQueryService(
        IEmergencyIncidentRepository incidentRepository,
        IPatientIncidentAccessService patientIncidentAccessService)
    {
        _incidentRepository = incidentRepository;
        _patientIncidentAccessService = patientIncidentAccessService;
    }

    public async Task<EmergencyIncident?> Handle(GetActiveFallIncidentByPatientIdQuery query)
    {
        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(query.ActorUserId, query.PatientId))
            throw new InvalidOperationException("No tienes permisos para consultar incidentes de este paciente.");

        return await _incidentRepository.FindActiveByPatientIdAsync(query.PatientId);
    }

    public async Task<EmergencyIncident?> Handle(GetEmergencyIncidentByIdQuery query)
    {
        var incident = await _incidentRepository.FindByIdWithFallTypeAsync(query.IncidentId);
        if (incident is null) return null;

        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(query.ActorUserId, incident.PatientId))
            throw new InvalidOperationException("No tienes permisos para consultar este incidente.");

        return incident;
    }

    public async Task<IReadOnlyCollection<EmergencyIncident>> Handle(ListFallIncidentHistoryByPatientIdQuery query)
    {
        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(query.ActorUserId, query.PatientId))
            throw new InvalidOperationException("No tienes permisos para consultar incidentes de este paciente.");

        return await _incidentRepository.ListByPatientIdAsync(query.PatientId);
    }
}
