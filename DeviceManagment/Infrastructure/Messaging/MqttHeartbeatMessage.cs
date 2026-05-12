using System.Text.Json.Serialization;

namespace foll_backend.DeviceManagment.Infrastructure.Messaging;

public class MqttHeartbeatMessage
{
    [JsonPropertyName("device_id")]
    public long DeviceId { get; init; }

    [JsonPropertyName("battery_level")]
    public short BatteryLevel { get; init; }

    [JsonPropertyName("is_charging")]
    public bool IsCharging { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime? ReportedAtUtc { get; init; }

    [JsonPropertyName("firmware_version")]
    public string? FirmwareVersion { get; init; }
}
