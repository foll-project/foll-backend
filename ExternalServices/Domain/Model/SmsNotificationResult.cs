namespace foll_backend.ExternalServices.Domain.Model;

public record SmsNotificationResult(bool Success, string? ProviderMessageId, string? ErrorMessage)
{
    public static SmsNotificationResult Sent(string? providerMessageId) => new(true, providerMessageId, null);
    public static SmsNotificationResult Failed(string errorMessage) => new(false, null, errorMessage);
}
