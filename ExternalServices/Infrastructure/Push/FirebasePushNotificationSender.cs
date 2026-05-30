using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;
using foll_backend.ExternalServices.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace foll_backend.ExternalServices.Infrastructure.Push;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(
        IOptions<FirebaseOptions> options,
        ILogger<FirebasePushNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return Task.FromResult(PushNotificationResult.Failed("Firebase esta deshabilitado por configuracion."));

        if (string.IsNullOrWhiteSpace(_options.ProjectId))
            return Task.FromResult(PushNotificationResult.Failed("Firebase:ProjectId no esta configurado."));

        if (string.IsNullOrWhiteSpace(_options.CredentialsPath))
            return Task.FromResult(PushNotificationResult.Failed("Firebase:CredentialsPath no esta configurado."));

        if (!File.Exists(_options.CredentialsPath))
            return Task.FromResult(PushNotificationResult.Failed($"No se encontro el archivo de credenciales Firebase: {_options.CredentialsPath}."));

        if (request.Tokens.Count == 0)
            return Task.FromResult(PushNotificationResult.Failed("No hay tokens push activos para el usuario."));

        _logger.LogWarning(
            "FirebasePushNotificationSender esta configurado, pero el envio real FCM queda preparado para una iteracion posterior. ProjectId={ProjectId}",
            _options.ProjectId);

        return Task.FromResult(PushNotificationResult.Failed("Firebase sender preparado, envio real FCM pendiente de credenciales y SDK."));
    }
}
