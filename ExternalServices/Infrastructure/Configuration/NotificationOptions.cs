namespace foll_backend.ExternalServices.Infrastructure.Configuration;

public class NotificationOptions
{
    public string PushProvider { get; set; } = "Fake";
    public string SmsProvider { get; set; } = "Fake";
}
