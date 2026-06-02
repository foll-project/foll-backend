using System.Text.Json.Serialization;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Messaging;

public class MqttFallCancelledMessage
{
    [JsonPropertyName("device_id")]
    public long DeviceId { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime? ReportedAtUtc { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}
