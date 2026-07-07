using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;
using foll_backend.ExternalServices.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace foll_backend.ExternalServices.Infrastructure.Sms;

public class TwilioSmsNotificationSender : ISmsNotificationSender
{
    private readonly SmsOptions _options;
    private readonly ILogger<TwilioSmsNotificationSender> _logger;

    public TwilioSmsNotificationSender(
        IOptions<SmsOptions> options,
        ILogger<TwilioSmsNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SmsNotificationResult> SendAsync(
        SmsNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var configurationError = ValidateConfiguration();
        if (configurationError is not null)
            return SmsNotificationResult.Failed(configurationError);

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return SmsNotificationResult.Failed("Twilio: el numero destino es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return SmsNotificationResult.Failed("Twilio: el mensaje SMS es obligatorio.");

        try
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(request.PhoneNumber),
                from: new PhoneNumber(_options.FromPhoneNumber),
                body: request.Message);

            if (message.ErrorCode.HasValue)
                return SmsNotificationResult.Failed(
                    $"Twilio error {message.ErrorCode}: {message.ErrorMessage ?? "sin detalle"}");

            _logger.LogInformation(
                "TwilioSms enviado | PhoneNumber={PhoneNumber} | ProviderMessageId={ProviderMessageId} | Status={Status}",
                request.PhoneNumber,
                message.Sid,
                message.Status);

            return SmsNotificationResult.Sent(message.Sid);
        }
        catch (ApiException exception)
        {
            _logger.LogError(
                exception,
                "Twilio API error enviando SMS a PhoneNumber={PhoneNumber}.",
                request.PhoneNumber);

            return SmsNotificationResult.Failed($"Twilio API error {exception.Code}: {exception.Message}");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error enviando SMS con Twilio a PhoneNumber={PhoneNumber}.",
                request.PhoneNumber);

            return SmsNotificationResult.Failed($"Twilio error: {exception.Message}");
        }
    }

    private string? ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.AccountSid))
            return "Twilio: Sms:AccountSid no esta configurado.";

        if (string.IsNullOrWhiteSpace(_options.AuthToken))
            return "Twilio: Sms:AuthToken no esta configurado.";

        if (string.IsNullOrWhiteSpace(_options.FromPhoneNumber))
            return "Twilio: Sms:FromPhoneNumber no esta configurado.";

        return null;
    }
}
