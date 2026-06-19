using System.Text.Json.Serialization;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;

public record DeviceTelemetryRealtimeMessage(
    [property: JsonPropertyName("deviceId")] long DeviceId,
    [property: JsonPropertyName("patientId")] long PatientId,
    [property: JsonPropertyName("batteryLevel")] short BatteryLevel,
    [property: JsonPropertyName("isCharging")] bool IsCharging,
    [property: JsonPropertyName("isOnline")] bool IsOnline,
    [property: JsonPropertyName("lastHeartbeatAt")] DateTime LastHeartbeatAtUtc);
