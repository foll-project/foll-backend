using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;

namespace foll_backend.ExternalServices.Infrastructure.Sms;

public class FakeSmsNotificationSender : ISmsNotificationSender
{
    private readonly ILogger<FakeSmsNotificationSender> _logger;

    public FakeSmsNotificationSender(ILogger<FakeSmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<SmsNotificationResult> SendAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var providerMessageId = $"fake-sms-{Guid.NewGuid():N}";
        _logger.LogInformation(
            "FakeSms no-op | PhoneNumber={PhoneNumber} | ProviderMessageId={ProviderMessageId}",
            request.PhoneNumber,
            providerMessageId);

        return Task.FromResult(SmsNotificationResult.Sent(providerMessageId));
    }
}
