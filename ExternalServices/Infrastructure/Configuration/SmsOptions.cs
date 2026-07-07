namespace foll_backend.ExternalServices.Infrastructure.Configuration;

public class SmsOptions
{
    public string Provider { get; set; } = "Fake";
    public string? FromPhoneNumber { get; set; }
    public string? AccountSid { get; set; }
    public string? AuthToken { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
}
