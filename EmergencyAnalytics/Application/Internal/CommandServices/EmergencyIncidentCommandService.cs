using foll_backend.EmergencyAnalytics.Application.Internal.Notifications;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Events;
using foll_backend.EmergencyAnalytics.Domain.Model.Commands;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.EmergencyAnalytics.Domain.Services;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.EmergencyAnalytics.Application.Internal.CommandServices;

public class EmergencyIncidentCommandService : IEmergencyIncidentCommandService
{
    private readonly IEmergencyIncidentRepository _incidentRepository;
    private readonly IFallTypeRepository _fallTypeRepository;
    private readonly IEmergencyOutboxMessageRepository _outboxMessageRepository;
    private readonly IDeviceIncidentAssignmentService _deviceIncidentAssignmentService;
    private readonly IPatientIncidentAccessService _patientIncidentAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public EmergencyIncidentCommandService(
        IEmergencyIncidentRepository incidentRepository,
        IFallTypeRepository fallTypeRepository,
        IEmergencyOutboxMessageRepository outboxMessageRepository,
        IDeviceIncidentAssignmentService deviceIncidentAssignmentService,
        IPatientIncidentAccessService patientIncidentAccessService,
        IUnitOfWork unitOfWork)
    {
        _incidentRepository = incidentRepository;
        _fallTypeRepository = fallTypeRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _deviceIncidentAssignmentService = deviceIncidentAssignmentService;
        _patientIncidentAccessService = patientIncidentAccessService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RegisterFallDetectedCommand command)
    {
        var patientId = await _deviceIncidentAssignmentService.FindAssignedPatientIdAsync(command.DeviceId);
        if (!patientId.HasValue)
            throw new InvalidOperationException("El dispositivo no está vinculado a ningún paciente.");

        var fallType = await ResolveFallTypeAsync(command.FallTypeId, command.FallTypeName);
        var incident = await _incidentRepository.FindLatestOpenByDeviceIdAsync(command.DeviceId);
        if (incident is null)
        {
            incident = EmergencyIncident.CreateFallDetected(
                command.DeviceId,
                patientId.Value,
                fallType.FallTypeId,
                command.ReportedAtUtc,
                command.AiConfidenceScore,
                command.Latitude,
                command.Longitude,
                command.RawPayload);

            await _incidentRepository.AddAsync(incident);
        }
        else
        {
            incident.RefreshDetection(
                fallType.FallTypeId,
                command.ReportedAtUtc,
                command.AiConfidenceScore,
                command.Latitude,
                command.Longitude,
                command.RawPayload);

            _incidentRepository.Update(incident);
        }

        await ProcessDomainEventsAsync(incident);
        incident.ClearDomainEvents();
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(RegisterFallCancelledCommand command)
    {
        var incident = await _incidentRepository.FindLatestOpenByDeviceIdAsync(command.DeviceId);
        if (incident is null) return;

        incident.Cancel(command.Reason, command.ReportedAtUtc, command.RawPayload);

        await ProcessDomainEventsAsync(incident);
        incident.ClearDomainEvents();
        _incidentRepository.Update(incident);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(MarkFallIncidentFalsePositiveCommand command)
    {
        var incident = await GetIncidentForManualActionAsync(command.IncidentId, command.ActorUserId);

        incident.MarkAsFalsePositive(command.ActorUserId, command.Observation, command.MarkedAtUtc);

        await ProcessDomainEventsAsync(incident);
        incident.ClearDomainEvents();
        _incidentRepository.Update(incident);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(ResolveFallIncidentCommand command)
    {
        var incident = await GetIncidentForManualActionAsync(command.IncidentId, command.ActorUserId);

        incident.Resolve(command.ActorUserId, command.Observation, command.ResolvedAtUtc);

        await ProcessDomainEventsAsync(incident);
        incident.ClearDomainEvents();
        _incidentRepository.Update(incident);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(UpdateEmergencyIncidentObservationCommand command)
    {
        var incident = await _incidentRepository.FindByIdAsync(command.IncidentId)
            ?? throw new InvalidOperationException("Incidente no encontrado.");

        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(command.ActorUserId, incident.PatientId))
            throw new InvalidOperationException("No tienes permisos para actualizar este incidente.");

        incident.UpdateFinalObservation(command.ActorUserId, command.Observation);

        await ProcessDomainEventsAsync(incident);
        incident.ClearDomainEvents();
        _incidentRepository.Update(incident);
        await _unitOfWork.CompleteAsync();
    }

    private async Task ProcessDomainEventsAsync(EmergencyIncident incident)
    {
        foreach (var domainEvent in incident.DomainEvents)
        {
            switch (domainEvent)
            {
                case EmergencyIncidentOpenedDomainEvent opened:
                    var openedFallType = await GetFallTypeOrThrowAsync(opened.FallTypeId);
                    await _outboxMessageRepository.AddAsync(EmergencyOutboxMessage.Create(
                        EmergencyAnalyticsEventTypes.IncidentOpenedV1,
                        new EmergencyIncidentOpenedIntegrationEvent(
                            opened.IncidentKey,
                            opened.DeviceId,
                            opened.PatientId,
                            opened.FallTypeId,
                            openedFallType.Name,
                            openedFallType.Description,
                            openedFallType.SeverityLevel,
                            opened.OpenedAtUtc,
                            opened.AiConfidenceScore,
                            opened.Latitude,
                            opened.Longitude),
                        opened.OccurredOn));
                    break;

                case EmergencyIncidentCancelledDomainEvent cancelled:
                    var cancelledFallType = await GetFallTypeOrThrowAsync(cancelled.FallTypeId);
                    await _outboxMessageRepository.AddAsync(EmergencyOutboxMessage.Create(
                        EmergencyAnalyticsEventTypes.IncidentClosedV1,
                        new EmergencyIncidentClosedIntegrationEvent(
                            cancelled.IncidentKey,
                            cancelled.DeviceId,
                            cancelled.PatientId,
                            cancelled.FallTypeId,
                            cancelledFallType.Name,
                            cancelledFallType.Description,
                            cancelledFallType.SeverityLevel,
                            EmergencyIncidentStatus.Cancelled,
                            cancelled.CancelledAtUtc,
                            cancelled.Reason,
                            cancelled.ActorUserId,
                            cancelled.Observation),
                        cancelled.OccurredOn));
                    break;

                case EmergencyIncidentResolvedDomainEvent resolved:
                    var resolvedFallType = await GetFallTypeOrThrowAsync(resolved.FallTypeId);
                    await _outboxMessageRepository.AddAsync(EmergencyOutboxMessage.Create(
                        EmergencyAnalyticsEventTypes.IncidentClosedV1,
                        new EmergencyIncidentClosedIntegrationEvent(
                            resolved.IncidentKey,
                            resolved.DeviceId,
                            resolved.PatientId,
                            resolved.FallTypeId,
                            resolvedFallType.Name,
                            resolvedFallType.Description,
                            resolvedFallType.SeverityLevel,
                            EmergencyIncidentStatus.Resolved,
                            resolved.ResolvedAtUtc,
                            null,
                            resolved.ActorUserId,
                            resolved.Observation),
                        resolved.OccurredOn));
                    break;
            }
        }
    }

    private async Task<EmergencyIncident> GetIncidentForManualActionAsync(long incidentId, long actorUserId)
    {
        var incident = await _incidentRepository.FindByIdWithFallTypeAsync(incidentId)
            ?? throw new InvalidOperationException("Incidente no encontrado.");

        if (!await _patientIncidentAccessService.CanAccessIncidentAsync(actorUserId, incident.PatientId))
            throw new InvalidOperationException("No tienes permisos para modificar este incidente.");

        return incident;
    }

    private async Task<FallType> ResolveFallTypeAsync(short? fallTypeId, string? fallTypeName)
    {
        FallType? fallType = null;

        if (fallTypeId.HasValue && fallTypeId.Value > 0)
            fallType = await _fallTypeRepository.FindByIdAsync(fallTypeId.Value);

        if (fallType is null && !string.IsNullOrWhiteSpace(fallTypeName))
            fallType = await _fallTypeRepository.FindByNameAsync(fallTypeName);

        fallType ??= await _fallTypeRepository.FindByIdAsync(FallType.UnknownId);

        return fallType ?? throw new InvalidOperationException("No existe el tipo de caída UNKNOWN en el catálogo.");
    }

    private async Task<FallType> GetFallTypeOrThrowAsync(short fallTypeId)
    {
        return await _fallTypeRepository.FindByIdAsync(fallTypeId)
            ?? throw new InvalidOperationException($"No existe el tipo de caída con id {fallTypeId}.");
    }
}
