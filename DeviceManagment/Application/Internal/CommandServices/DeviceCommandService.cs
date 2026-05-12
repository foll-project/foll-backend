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
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeviceMonitoringOptions _monitoringOptions;

    public DeviceCommandService(
        IDeviceRepository deviceRepository,
        IDeviceEventRepository deviceEventRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IPatientAccessService patientAccessService,
        IUnitOfWork unitOfWork,
        IOptions<DeviceMonitoringOptions> monitoringOptions)
    {
        _deviceRepository = deviceRepository;
        _deviceEventRepository = deviceEventRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _patientAccessService = patientAccessService;
        _unitOfWork = unitOfWork;
        _monitoringOptions = monitoringOptions.Value;
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
        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null) throw new InvalidOperationException("Dispositivo no encontrado.");

        var previousHeartbeatAt = device.LastHeartbeatAt;

        if (!string.IsNullOrWhiteSpace(command.FirmwareVersion))
            device.UpdateFirmware(command.FirmwareVersion);

        device.RegisterTelemetry(
            command.BatteryLevel,
            command.IsCharging,
            command.ReportedAtUtc,
            _monitoringOptions.LowBatteryThreshold);

        if (previousHeartbeatAt.HasValue &&
            device.LastHeartbeatAt == previousHeartbeatAt &&
            command.ReportedAtUtc.ToUniversalTime() <= previousHeartbeatAt.Value)
        {
            device.ClearDomainEvents();
            return;
        }

        await ProcessDomainEventsAsync(device);

        device.ClearDomainEvents();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();
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
        var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
        if (device is null) return;

        device.UpdatePowerState(command.IsActive, command.ReportedAtUtc);

        await ProcessDomainEventsAsync(device);

        device.ClearDomainEvents();

        _deviceRepository.Update(device);
        await _unitOfWork.CompleteAsync();
    }

    private async Task ProcessDomainEventsAsync(Device device)
    {
        foreach (var domainEvent in device.DomainEvents)
        {
            switch (domainEvent)
            {
                case LowBatteryDetectedDomainEvent lowBatteryDetected:
                    await RegisterLowBatteryDetectedAsync(lowBatteryDetected);
                    break;

                case LowBatteryResolvedDomainEvent lowBatteryResolved:
                    await ResolveDeviceEventAsync(lowBatteryResolved.DeviceId, DeviceEventType.LowBatteryDetected, lowBatteryResolved.ReportedAtUtc);
                    await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                        DeviceManagmentEventTypes.LowBatteryResolvedV1,
                        new LowBatteryResolvedIntegrationEvent(
                            lowBatteryResolved.DeviceId,
                            lowBatteryResolved.AssignedPatientId,
                            lowBatteryResolved.BatteryLevel,
                            lowBatteryResolved.ReportedAtUtc),
                        lowBatteryResolved.OccurredOn));
                    break;

                case DeviceDisconnectedDomainEvent deviceDisconnected:
                    await RegisterDeviceDisconnectedAsync(deviceDisconnected);
                    break;

                case DeviceReconnectedDomainEvent deviceReconnected:
                    await ResolveDeviceEventAsync(deviceReconnected.DeviceId, DeviceEventType.DeviceDisconnected, deviceReconnected.ReportedAtUtc);
                    await _outboxMessageRepository.AddAsync(OutboxMessage.Create(
                        DeviceManagmentEventTypes.DeviceReconnectedV1,
                        new DeviceReconnectedIntegrationEvent(
                            deviceReconnected.DeviceId,
                            deviceReconnected.AssignedPatientId,
                            deviceReconnected.ReportedAtUtc,
                            deviceReconnected.Cause),
                        deviceReconnected.OccurredOn));
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
        }
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
        }
    }

    private async Task ResolveDeviceEventAsync(long deviceId, DeviceEventType eventType, DateTime resolvedAtUtc)
    {
        var activeEvent = await _deviceEventRepository.FindLatestUnresolvedByDeviceIdAndTypeAsync(deviceId, eventType);
        activeEvent?.Resolve(resolvedAtUtc);
    }
}
