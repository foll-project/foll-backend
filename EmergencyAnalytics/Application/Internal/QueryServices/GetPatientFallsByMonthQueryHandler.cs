using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Queries;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.QueryServices;

public class GetPatientFallsByMonthQueryHandler : IRequestHandler<GetPatientFallsByMonthQuery, IEnumerable<EmergencyIncident>>
{
    private readonly IEmergencyIncidentRepository _incidentRepository;
    private readonly IPatientIncidentAccessService _patientIncidentAccessService;

    public GetPatientFallsByMonthQueryHandler(
        IEmergencyIncidentRepository incidentRepository,
        IPatientIncidentAccessService patientIncidentAccessService)
    {
        _incidentRepository = incidentRepository;
        _patientIncidentAccessService = patientIncidentAccessService;
    }

    public async Task<IEnumerable<EmergencyIncident>> Handle(GetPatientFallsByMonthQuery request, CancellationToken cancellationToken)
    {
        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(request.ActorUserId, request.PatientId))
            throw new InvalidOperationException("No tienes permisos para consultar incidentes de este paciente.");

        // Calculate UTC boundaries for the given month and year
        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        // Fetching and filtering in memory to remain purely additive without altering the existing repository interface
        var allIncidents = await _incidentRepository.ListByPatientIdAsync(request.PatientId);
        
        return allIncidents
            .Where(i => i.OpenedAt >= startDate && i.OpenedAt <= endDate)
            .OrderByDescending(i => i.OpenedAt);
    }
}
