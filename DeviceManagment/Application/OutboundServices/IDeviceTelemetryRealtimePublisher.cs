namespace foll_backend.DeviceManagment.Application.OutboundServices;

/// <summary>
/// Puerto de salida para empujar telemetría del dispositivo en TIEMPO REAL
/// (cada heartbeat) hacia los cuidadores del paciente, sin pasar por el Outbox
/// ni por la tabla de notificaciones persistentes. Es un canal liviano y efímero.
/// </summary>
public interface IDeviceTelemetryRealtimePublisher
{
    Task PublishTelemetryAsync(DeviceTelemetrySnapshot snapshot, CancellationToken cancellationToken = default);
}

/// <summary>
/// Foto instantánea del estado del dispositivo en el momento de un heartbeat.
/// </summary>
public record DeviceTelemetrySnapshot(
    long DeviceId,
    long PatientId,
    short BatteryLevel,
    bool IsCharging,
    bool IsOnline,
    DateTime LastHeartbeatAtUtc);
