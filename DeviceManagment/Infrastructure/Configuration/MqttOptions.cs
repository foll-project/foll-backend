namespace foll_backend.DeviceManagment.Infrastructure.Configuration;

public class MqttOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "foll-device-worker";
    public string HeartbeatTopic { get; set; } = "foll/devices/+/heartbeat";
    public string PowerTopic { get; set; } = "foll/devices/+/power";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseTls { get; set; }
}
