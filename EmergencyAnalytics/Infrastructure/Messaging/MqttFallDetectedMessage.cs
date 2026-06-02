using System.Text.Json.Serialization;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Messaging;

public class MqttFallDetectedMessage
{
    [JsonPropertyName("device_id")]
    public long DeviceId { get; init; }

    [JsonPropertyName("fall_type_id")]
    public short? FallTypeId { get; init; }

    [JsonPropertyName("fall_type")]
    public string? FallType { get; init; }

    [JsonPropertyName("fall_type_name")]
    public string? FallTypeName { get; init; }

    [JsonPropertyName("ai_confidence_score")]
    public decimal? AiConfidenceScore { get; init; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime? ReportedAtUtc { get; init; }
}
