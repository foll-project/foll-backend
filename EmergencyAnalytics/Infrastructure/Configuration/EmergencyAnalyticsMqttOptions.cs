namespace foll_backend.EmergencyAnalytics.Infrastructure.Configuration;

public class EmergencyAnalyticsMqttOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "foll-emergency-analytics-worker";
    public string FallDetectedTopic { get; set; } = "foll/devices/+/fall-detected";
    public string FallCancelledTopic { get; set; } = "foll/devices/+/fall-cancelled";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseTls { get; set; }
}
