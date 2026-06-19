using foll_backend.DeviceManagment.Application.Internal.Notifications;
using foll_backend.DeviceManagment.Application.OutboundServices;
using foll_backend.DeviceManagment.Domain.Events;
using foll_backend.DeviceManagment.Domain.Model.Commands;
using foll_backend.DeviceManagment.Domain.Model.Entities;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using foll_backend.Shared.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace foll_backend.DeviceManagment.Application.Internal.CommandServices;

public class DeviceCommandService : IDeviceCommandService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceEventRepository _deviceEventRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IPatientAccessService _patientAccessService;
    private readonly IDeviceTelemetryRealtimePublisher _telemetryRealtimePublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeviceMonitoringOptions _monitoringOptions;
    private readonly ILogger<DeviceCommandService> _logger;

    public DeviceCommandService(
        IDeviceRepository deviceRepository,
        IDeviceEventRepository deviceEventRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IPatientAccessService patientAccessService,
        IDeviceTelemetryRealtimePublisher telemetryRealtimePublisher,
        IUnitOfWork unitOfWork,
        IOptions<DeviceMonitoringOptions> monitoringOptions,
        ILogger<DeviceCommandService> logger)
    {
        _deviceRepository = deviceRepository;
        _deviceEventRepository = deviceEventRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _patientAccessService = patientAccessService;
        _telemetryRealtimePublisher = telemetryRealtimePublisher;
        _unitOfWork = unitOfWork;
        _monitoringOptions = monitoringOptions.Value;
        _logger = logger;
    }

    public async Task Handle(LinkDeviceCommand command)
    {
        if (!await _patientAccessService.CanManageDevicesAsync(command.ActorUserId, command.PatientId))
            throw new InvalidOperationException("No tienes permisos para vincular dispositivos a este paciente.");

        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null) throw new InvalidOperationException("Dispositivo no encontrado.");

        var existingDevice = await _deviceRepository.FindByAssignedPatientIdAsync(command.PatientId);
        if (existingDevice is not null && existingDevice.DeviceId != command.DeviceId)
            throw new InvalidOperationException("El paciente ya tiene un dispositivo vinculado.");

        device.AssignToPatient(command.PatientId, DateTime.UtcNow);

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(UnlinkDeviceCommand command)
    {
        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null) throw new InvalidOperationException("Dispositivo no encontrado.");
        if (!device.AssignedPatientId.HasValue) throw new InvalidOperationException("El dispositivo no está vinculado a ningún paciente.");

        if (!await _patientAccessService.CanManageDevicesAsync(command.ActorUserId, device.AssignedPatientId.Value))
            throw new InvalidOperationException("No tienes permisos para desvincular este dispositivo.");

        device.Unassign();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(RegisterDeviceTelemetryCommand command)
    {
        _logger.LogDebug(
            "Device telemetry recibido. DeviceId={DeviceId} BatteryLevel={BatteryLevel} IsCharging={IsCharging} ReportedAtUtc={ReportedAtUtc} FirmwareVersion={FirmwareVersion}",
            command.DeviceId,
            command.BatteryLevel,
            command.IsCharging,
            command.ReportedAtUtc,
            command.FirmwareVersion);

        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null)
        {
            _logger.LogWarning("Device telemetry ignorado. DeviceId={DeviceId} Reason=Dispositivo no encontrado.", command.DeviceId);
            throw new InvalidOperationException("Dispositivo no encontrado.");
        }

        var previousHeartbeatAt = device.LastHeartbeatAt;
        var previousConnectivityStatus = device.ConnectivityStatus;
        var previousBatteryLevel = device.CurrentBatteryLevel;
        var previousIsCharging = device.IsCharging;

        _logger.LogDebug(
            "Device telemetry estado previo. DeviceId={DeviceId} AssignedPatientId={AssignedPatientId} ConnectivityStatus={ConnectivityStatus} LastHeartbeatAt={LastHeartbeatAt} BatteryLevel={BatteryLevel} IsCharging={IsCharging} MonitoringStartedAt={MonitoringStartedAt}",
            device.DeviceId,
            device.AssignedPatientId,
            device.ConnectivityStatus,
            device.LastHeartbeatAt,
            device.CurrentBatteryLevel,
            device.IsCharging,
            device.MonitoringStartedAt);

        if (!string.IsNullOrWhiteSpace(command.FirmwareVersion))
            device.UpdateFirmware(command.FirmwareVersion);

        device.RegisterTelemetry(
            command.BatteryLevel,
            command.IsCharging,
            command.ReportedAtUtc,
            _monitoringOptions.LowBatteryThreshold);

        _logger.LogDebug(
            "Device telemetry estado posterior. DeviceId={DeviceId} AssignedPatientId={AssignedPatientId} PreviousConnectivityStatus={PreviousConnectivityStatus} ConnectivityStatus={ConnectivityStatus} PreviousLastHeartbeatAt={PreviousLastHeartbeatAt} LastHeartbeatAt={LastHeartbeatAt} PreviousBatteryLevel={PreviousBatteryLevel} BatteryLevel={BatteryLevel} PreviousIsCharging={PreviousIsCharging} IsCharging={IsCharging} DomainEvents={DomainEventCount}",
            device.DeviceId,
            device.AssignedPatientId,
            previousConnectivityStatus,
            device.ConnectivityStatus,
            previousHeartbeatAt,
            device.LastHeartbeatAt,
            previousBatteryLevel,
            device.CurrentBatteryLevel,
            previousIsCharging,
            device.IsCharging,
            device.DomainEvents.Count);

        if (previousHeartbeatAt.HasValue &&
            device.LastHeartbeatAt == previousHeartbeatAt &&
            command.ReportedAtUtc.ToUniversalTime() <= previousHeartbeatAt.Value)
        {
            _logger.LogDebug(
                "Device telemetry ignorado. DeviceId={DeviceId} Reason=Timestamp no es posterior al ultimo heartbeat. ReportedAtUtc={ReportedAtUtc} LastHeartbeatAt={LastHeartbeatAt}",
                command.DeviceId,
                command.ReportedAtUtc,
                previousHeartbeatAt);
            device.ClearDomainEvents();
            return;
        }

        if (device.DomainEvents.Count == 0)
        {
            _logger.LogDebug(
                "Device telemetry aplicado sin generar eventos de dominio. DeviceId={DeviceId} LastHeartbeatAt={LastHeartbeatAt} ConnectivityStatus={ConnectivityStatus}",
                device.DeviceId,
                device.LastHeartbeatAt,
                device.ConnectivityStatus);
        }

        await ProcessDomainEventsAsync(device);

        device.ClearDomainEvents();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();

        _logger.LogDebug(
            "Device telemetry persistido. DeviceId={DeviceId} LastHeartbeatAt={LastHeartbeatAt} ConnectivityStatus={ConnectivityStatus}",
            device.DeviceId,
            device.LastHeartbeatAt,
            device.ConnectivityStatus);

        // Push de telemetria en TIEMPO REAL (cada heartbeat) hacia los cuidadores
        // del paciente. Solo si el dispositivo esta vinculado: si no, no hay a quien notificar.
        if (device.AssignedPatientId.HasValue)
        {
            await _telemetryRealtimePublisher.PublishTelemetryAsync(new DeviceTelemetrySnapshot(
                device.DeviceId,
                device.AssignedPatientId.Value,
                device.CurrentBatteryLevel ?? 0,
                device.IsCharging ?? false,
                device.ConnectivityStatus == ConnectivityStatus.Connected,
                device.LastHeartbeatAt ?? command.ReportedAtUtc));
        }
    }

    public async Task Handle(CheckDeviceConnectivityCommand command)
    {
        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null) return;

        device.CheckConnectivity(command.DetectedAtUtc, command.DisconnectThreshold);

        await ProcessDomainEventsAsync(device);

        device.ClearDomainEvents();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(UpdateDevicePowerStateCommand command)
    {
        _logger.LogDebug(
            "Device power recibido. DeviceId={DeviceId} IsActive={IsActive} ReportedAtUtc={ReportedAtUtc}",
            command.DeviceId,
            command.IsActive,
            command.ReportedAtUtc);

        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null)
        {
            _logger.LogWarning("Device power ignorado. DeviceId={DeviceId} Reason=Dispositivo no encontrado.", command.DeviceId);
            return;
        }

        var previousConnectivityStatus = device.ConnectivityStatus;
        var previousLastConnectivityChangeAt = device.LastConnectivityChangeAt;

        _logger.LogDebug(
            "Device power estado previo. DeviceId={DeviceId} AssignedPatientId={AssignedPatientId} ConnectivityStatus={ConnectivityStatus} LastConnectivityChangeAt={LastConnectivityChangeAt}",
            device.DeviceId,
            device.AssignedPatientId,
            device.ConnectivityStatus,
            device.LastConnectivityChangeAt);

        device.UpdatePowerState(command.IsActive, command.ReportedAtUtc);

        _logger.LogDebug(
            "Device power estado posterior. DeviceId={DeviceId} AssignedPatientId={AssignedPatientId} PreviousConnectivityStatus={PreviousConnectivityStatus} ConnectivityStatus={ConnectivityStatus} PreviousLastConnectivityChangeAt={PreviousLastConnectivityChangeAt} LastConnectivityChangeAt={LastConnectivityChangeAt} DomainEvents={DomainEventCount}",
            device.DeviceId,
            device.AssignedPatientId,
            previousConnectivityStatus,
            device.ConnectivityStatus,
            previousLastConnectivityChangeAt,
            device.LastConnectivityChangeAt,
            device.DomainEvents.Count);

        if (device.DomainEvents.Count == 0)
        {
            _logger.LogDebug(
                "Device power aplicado sin generar eventos de dominio. DeviceId={DeviceId} Reason=Sin cambio de estado, dispositivo no asignado o estado ya coincidia.",
                device.DeviceId);
        }

        await ProcessDomainEventsAsync(device);

        device.ClearDomainEvents();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();

        _logger.LogDebug(
            "Device power persistido. DeviceId={DeviceId} ConnectivityStatus={ConnectivityStatus} LastConnectivityChangeAt={LastConnectivityChangeAt}",
            device.DeviceId,
            device.ConnectivityStatus,
            device.LastConnectivityChangeAt);
    }

    private async Task ProcessDomainEventsAsync(Device device)
    {
        foreach (var domainEvent in device.DomainEvents)
        {
            switch (domainEvent)
            {
                case LowBatteryDetectedDomainEvent lowBatteryDetected:
                    _logger.LogInformation(
                        "Device domain event generado. Event=LowBatteryDetected DeviceId={DeviceId} PatientId={PatientId} BatteryLevel={BatteryLevel} ReportedAtUtc={ReportedAtUtc}",
                        lowBatteryDetected.DeviceId,
                        lowBatteryDetected.AssignedPatientId,
                        lowBatteryDetected.BatteryLevel,
                        lowBatteryDetected.ReportedAtUtc);
                    await RegisterLowBatteryDetectedAsync(lowBatteryDetected);
                    break;

                case LowBatteryResolvedDomainEvent lowBatteryResolved:
                    _logger.LogInformation(
                        "Device domain event generado. Event=LowBatteryResolved DeviceId={DeviceId} PatientId={PatientId} BatteryLevel={BatteryLevel} ReportedAtUtc={ReportedAtUtc}",
                        lowBatteryResolved.DeviceId,
                        lowBatteryResolved.AssignedPatientId,
                        lowBatteryResolved.BatteryLevel,
                        lowBatteryResolved.ReportedAtUtc);
                    await ResolveDeviceEventAsync(lowBatteryResolved.DeviceId, DeviceEventType.LowBatteryDetected, lowBatteryResolved.ReportedAtUtc);
                    await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                        DeviceManagmentEventTypes.LowBatteryResolvedV1,
                        new LowBatteryResolvedIntegrationEvent(
                            lowBatteryResolved.DeviceId,
                            lowBatteryResolved.AssignedPatientId,
                            lowBatteryResolved.BatteryLevel,
                            lowBatteryResolved.ReportedAtUtc),
                        lowBatteryResolved.OccurredOn));
                    _logger.LogInformation(
                        "Device outbox message creado. Type={Type} DeviceId={DeviceId}",
                        DeviceManagmentEventTypes.LowBatteryResolvedV1,
                        lowBatteryResolved.DeviceId);
                    break;

                case DeviceDisconnectedDomainEvent deviceDisconnected:
                    _logger.LogInformation(
                        "Device domain event generado. Event=DeviceDisconnected DeviceId={DeviceId} PatientId={PatientId} DetectedAtUtc={DetectedAtUtc} LastSeenAtUtc={LastSeenAtUtc} Cause={Cause}",
                        deviceDisconnected.DeviceId,
                        deviceDisconnected.AssignedPatientId,
                        deviceDisconnected.DetectedAtUtc,
                        deviceDisconnected.LastSeenAtUtc,
                        deviceDisconnected.Cause);
                    await RegisterDeviceDisconnectedAsync(deviceDisconnected);
                    break;

                case DeviceReconnectedDomainEvent deviceReconnected:
                    _logger.LogInformation(
                        "Device domain event generado. Event=DeviceReconnected DeviceId={DeviceId} PatientId={PatientId} ReportedAtUtc={ReportedAtUtc} Cause={Cause}",
                        deviceReconnected.DeviceId,
                        deviceReconnected.AssignedPatientId,
                        deviceReconnected.ReportedAtUtc,
                        deviceReconnected.Cause);
                    await ResolveDeviceEventAsync(deviceReconnected.DeviceId, DeviceEventType.DeviceDisconnected, deviceReconnected.ReportedAtUtc);
                    await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                        DeviceManagmentEventTypes.DeviceReconnectedV1,
                        new DeviceReconnectedIntegrationEvent(
                            deviceReconnected.DeviceId,
                            deviceReconnected.AssignedPatientId,
                            deviceReconnected.ReportedAtUtc,
                            deviceReconnected.Cause),
                        deviceReconnected.OccurredOn));
                    _logger.LogInformation(
                        "Device outbox message creado. Type={Type} DeviceId={DeviceId}",
                        DeviceManagmentEventTypes.DeviceReconnectedV1,
                        deviceReconnected.DeviceId);
                    break;
            }
        }
    }

    private async Task RegisterLowBatteryDetectedAsync(LowBatteryDetectedDomainEvent lowBatteryDetected)
    {
        var activeEvent = await _deviceEventRepository.FindLatestUnresolvedByDeviceIdAndTypeAsync(
            lowBatteryDetected.DeviceId,
            DeviceEventType.LowBatteryDetected);

        if (activeEvent is null)
        {
            await _deviceEventRepository.AddAsync(DeviceEvent.CreateLowBatteryDetected(
                lowBatteryDetected.DeviceId,
                lowBatteryDetected.BatteryLevel,
                lowBatteryDetected.ReportedAtUtc));

            await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                DeviceManagmentEventTypes.LowBatteryDetectedV1,
                new LowBatteryDetectedIntegrationEvent(
                    lowBatteryDetected.DeviceId,
                    lowBatteryDetected.AssignedPatientId,
                    lowBatteryDetected.BatteryLevel,
                    lowBatteryDetected.ReportedAtUtc),
                lowBatteryDetected.OccurredOn));

            _logger.LogInformation(
                "Device event y outbox creados. Event=LowBatteryDetected Type={Type} DeviceId={DeviceId}",
                DeviceManagmentEventTypes.LowBatteryDetectedV1,
                lowBatteryDetected.DeviceId);
            return;
        }

        _logger.LogInformation(
            "Device outbox no creado. Event=LowBatteryDetected DeviceId={DeviceId} Reason=Ya existe evento activo no resuelto.",
            lowBatteryDetected.DeviceId);
    }

    private async Task RegisterDeviceDisconnectedAsync(DeviceDisconnectedDomainEvent deviceDisconnected)
    {
        var activeEvent = await _deviceEventRepository.FindLatestUnresolvedByDeviceIdAndTypeAsync(
            deviceDisconnected.DeviceId,
            DeviceEventType.DeviceDisconnected);

        if (activeEvent is null)
        {
            await _deviceEventRepository.AddAsync(DeviceEvent.CreateDeviceDisconnected(
                deviceDisconnected.DeviceId,
                deviceDisconnected.DetectedAtUtc,
                deviceDisconnected.LastSeenAtUtc,
                deviceDisconnected.Cause));

            await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                DeviceManagmentEventTypes.DeviceDisconnectedV1,
                new DeviceDisconnectedIntegrationEvent(
                    deviceDisconnected.DeviceId,
                    deviceDisconnected.AssignedPatientId,
                    deviceDisconnected.DetectedAtUtc,
                    deviceDisconnected.LastSeenAtUtc,
                    deviceDisconnected.Cause),
                deviceDisconnected.OccurredOn));

            _logger.LogInformation(
                "Device event y outbox creados. Event=DeviceDisconnected Type={Type} DeviceId={DeviceId}",
                DeviceManagmentEventTypes.DeviceDisconnectedV1,
                deviceDisconnected.DeviceId);
            return;
        }

        _logger.LogInformation(
            "Device outbox no creado. Event=DeviceDisconnected DeviceId={DeviceId} Reason=Ya existe evento activo no resuelto.",
            deviceDisconnected.DeviceId);
    }

    private async Task ResolveDeviceEventAsync(long deviceId, DeviceEventType eventType, DateTime resolvedAtUtc)
    {
        var activeEvent = await _deviceEventRepository.FindLatestUnresolvedByDeviceIdAndTypeAsync(deviceId, eventType);
        if (activeEvent is null)
        {
            _logger.LogInformation(
                "Device event no resuelto. DeviceId={DeviceId} EventType={EventType} Reason=No existe evento activo.",
                deviceId,
                eventType);
            return;
        }

        activeEvent.Resolve(resolvedAtUtc);
        _logger.LogInformation(
            "Device event resuelto. DeviceId={DeviceId} EventType={EventType} ResolvedAtUtc={ResolvedAtUtc}",
            deviceId,
            eventType,
            resolvedAtUtc);
    }
}
