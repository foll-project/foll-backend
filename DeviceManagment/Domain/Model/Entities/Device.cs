using foll_backend.DeviceManagment.Domain.Events;
using ConnectivityState = foll_backend.DeviceManagment.Domain.Model.Enums.ConnectivityStatus;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.Shared.Domain.Events;

namespace foll_backend.DeviceManagment.Domain.Model.Entities;

public class Device : EntityWithDomainEvents
{
    public long DeviceId { get; private set; }
    public long? AssignedPatientId { get; private set; }
    public string FirmwareVersion { get; private set; }
    public DeviceStatus Status { get; private set; }
    public ConnectivityStatus? ConnectivityStatus { get; private set; }
    public short? CurrentBatteryLevel { get; private set; }
    public bool? IsCharging { get; private set; }
    public DateTime? LastHeartbeatAt { get; private set; }
    public DateTime? MonitoringStartedAt { get; private set; }
    public DateTime? LastConnectivityChangeAt { get; private set; }

    protected Device()
    {
        FirmwareVersion = string.Empty;
        Status = DeviceStatus.Active;
    }

    public Device(long deviceId, string firmwareVersion, DeviceStatus status = DeviceStatus.Active)
    {
        if (deviceId <= 0) throw new ArgumentOutOfRangeException(nameof(deviceId));
        if (string.IsNullOrWhiteSpace(firmwareVersion)) throw new ArgumentException("La versión de firmware es obligatoria.", nameof(firmwareVersion));

        DeviceId = deviceId;
        FirmwareVersion = firmwareVersion.Trim();
        Status = status;
    }

    public void AssignToPatient(long patientId, DateTime monitoringStartedAtUtc)
    {
        EnsureDeviceIsActive();

        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (AssignedPatientId.HasValue)
            throw new InvalidOperationException("El dispositivo ya está vinculado a un paciente.");

        var normalizedTimestamp = NormalizeTimestamp(monitoringStartedAtUtc);
        AssignedPatientId = patientId;
        MonitoringStartedAt = normalizedTimestamp;
        ConnectivityStatus = ConnectivityState.Connected;
        LastConnectivityChangeAt = normalizedTimestamp;
    }

    public void Unassign()
    {
        if (!AssignedPatientId.HasValue)
            throw new InvalidOperationException("El dispositivo no está vinculado a ningún paciente.");

        AssignedPatientId = null;
        ConnectivityStatus = null;
        MonitoringStartedAt = null;
        LastConnectivityChangeAt = null;
    }

    public void UpdateFirmware(string firmwareVersion)
    {
        if (string.IsNullOrWhiteSpace(firmwareVersion))
            throw new ArgumentException("La versión de firmware es obligatoria.", nameof(firmwareVersion));

        FirmwareVersion = firmwareVersion.Trim();
    }

    public void RegisterTelemetry(short batteryLevel, bool isCharging, DateTime reportedAtUtc, short lowBatteryThreshold)
    {
        EnsureDeviceIsActive();

        if (batteryLevel is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(batteryLevel), "La batería debe estar entre 0 y 100.");
        if (lowBatteryThreshold is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(lowBatteryThreshold), "El umbral de batería baja debe estar entre 1 y 100.");

        var normalizedTimestamp = NormalizeTimestamp(reportedAtUtc);

        if (LastHeartbeatAt.HasValue && normalizedTimestamp <= LastHeartbeatAt.Value)
            return;

        var wasLowBattery = IsLowBattery(CurrentBatteryLevel, IsCharging, lowBatteryThreshold);
        var wasDisconnected = ConnectivityStatus == ConnectivityState.Disconnected;

        CurrentBatteryLevel = batteryLevel;
        IsCharging = isCharging;
        LastHeartbeatAt = normalizedTimestamp;

        var isLowBatteryNow = IsLowBattery(CurrentBatteryLevel, IsCharging, lowBatteryThreshold);
        if (!AssignedPatientId.HasValue) return;

        if (wasDisconnected)
        {
            ConnectivityStatus = ConnectivityState.Connected;
            LastConnectivityChangeAt = normalizedTimestamp;
            RaiseDomainEvent(new DeviceReconnectedDomainEvent(
                DeviceId,
                AssignedPatientId.Value,
                normalizedTimestamp,
                ConnectivityChangeCause.HeartbeatReceived));
        }

        if (!wasLowBattery && isLowBatteryNow)
        {
            RaiseDomainEvent(new LowBatteryDetectedDomainEvent(DeviceId, AssignedPatientId.Value, batteryLevel, normalizedTimestamp));
            return;
        }

        if (wasLowBattery && !isLowBatteryNow)
        {
            RaiseDomainEvent(new LowBatteryResolvedDomainEvent(DeviceId, AssignedPatientId.Value, batteryLevel, normalizedTimestamp));
        }
    }

    public void CheckConnectivity(DateTime detectedAtUtc, TimeSpan offlineThreshold)
    {
        EnsureDeviceIsActive();

        if (!AssignedPatientId.HasValue) return;
        if (ConnectivityStatus != ConnectivityState.Connected) return;
        if (offlineThreshold <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(offlineThreshold), "El umbral offline debe ser mayor a cero.");

        var normalizedDetectedAt = NormalizeTimestamp(detectedAtUtc);
        var lastSeenAt = GetLatestPresenceAt(normalizedDetectedAt);
        if (!lastSeenAt.HasValue) return;

        if (lastSeenAt.Value > normalizedDetectedAt - offlineThreshold)
            return;

        ConnectivityStatus = ConnectivityState.Disconnected;
        LastConnectivityChangeAt = normalizedDetectedAt;
        RaiseDomainEvent(new DeviceDisconnectedDomainEvent(
            DeviceId,
            AssignedPatientId.Value,
            normalizedDetectedAt,
            lastSeenAt.Value,
            ConnectivityChangeCause.HeartbeatTimeout));
    }

    public void UpdatePowerState(bool isActive, DateTime reportedAtUtc)
    {
        EnsureDeviceIsActive();

        if (!AssignedPatientId.HasValue) return;

        var normalizedTimestamp = NormalizeTimestamp(reportedAtUtc);

        if (!isActive)
        {
            if (ConnectivityStatus == ConnectivityState.Disconnected) return;

            ConnectivityStatus = ConnectivityState.Disconnected;
            LastConnectivityChangeAt = normalizedTimestamp;
            RaiseDomainEvent(new DeviceDisconnectedDomainEvent(
                DeviceId,
                AssignedPatientId.Value,
                normalizedTimestamp,
                normalizedTimestamp,
                ConnectivityChangeCause.PowerOff));
            return;
        }

        if (ConnectivityStatus == ConnectivityState.Connected) return;

        ConnectivityStatus = ConnectivityState.Connected;
        LastConnectivityChangeAt = normalizedTimestamp;
        RaiseDomainEvent(new DeviceReconnectedDomainEvent(
            DeviceId,
            AssignedPatientId.Value,
            normalizedTimestamp,
            ConnectivityChangeCause.PowerOn));
    }

    private void EnsureDeviceIsActive()
    {
        if (Status != DeviceStatus.Active)
            throw new InvalidOperationException("Solo los dispositivos activos pueden operar en esta versión.");
    }

    private static bool IsLowBattery(short? batteryLevel, bool? isCharging, short lowBatteryThreshold)
    {
        return batteryLevel.HasValue && batteryLevel.Value < lowBatteryThreshold && isCharging == false;
    }

    private DateTime? GetLatestPresenceAt(DateTime fallbackUtc)
    {
        var candidates = new List<DateTime>();

        if (MonitoringStartedAt.HasValue) candidates.Add(MonitoringStartedAt.Value);
        if (LastHeartbeatAt.HasValue) candidates.Add(LastHeartbeatAt.Value);
        if (LastConnectivityChangeAt.HasValue && ConnectivityStatus == ConnectivityState.Connected)
            candidates.Add(LastConnectivityChangeAt.Value);

        if (candidates.Count == 0) return fallbackUtc;

        return candidates.Max();
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
