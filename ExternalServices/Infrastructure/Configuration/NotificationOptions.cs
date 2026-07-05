namespace foll_backend.ExternalServices.Infrastructure.Configuration;

public class NotificationOptions
{
    public string PushProvider { get; set; } = "Fake";
    public string SmsProvider { get; set; } = "Fake";
    public string PublicBaseUrl { get; set; } = "http://localhost:5000";
    public int EmergencyLocationLinkExpirationMinutes { get; set; } = 30;
}
