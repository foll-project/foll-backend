using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;
using foll_backend.ExternalServices.Infrastructure.Configuration;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace foll_backend.ExternalServices.Infrastructure.Push;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private const string FirebaseAppName = "foll-backend-firebase";
    private static readonly object FirebaseAppLock = new();
    private static FirebaseApp? _firebaseApp;

    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(
        IOptions<FirebaseOptions> options,
        ILogger<FirebasePushNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return PushNotificationResult.Failed("Firebase esta deshabilitado por configuracion.", request.Tokens.Count);

        if (string.IsNullOrWhiteSpace(_options.ProjectId))
            return PushNotificationResult.Failed("Firebase:ProjectId no esta configurado.", request.Tokens.Count);

        if (string.IsNullOrWhiteSpace(_options.CredentialsPath))
            return PushNotificationResult.Failed("Firebase:CredentialsPath no esta configurado.", request.Tokens.Count);

        if (!File.Exists(_options.CredentialsPath))
            return PushNotificationResult.Failed($"No se encontro el archivo de credenciales Firebase: {_options.CredentialsPath}.", request.Tokens.Count);

        if (request.Tokens.Count == 0)
            return PushNotificationResult.Failed("No hay tokens push activos para el usuario.", 0);

        var tokens = request.Tokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (tokens.Count == 0)
            return PushNotificationResult.Failed("No hay tokens push validos para el usuario.", request.Tokens.Count);

        try
        {
            var app = GetOrCreateFirebaseApp();
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body
                },
                Data = NormalizeData(request.Data)
            };

            var response = await FirebaseMessaging.GetMessaging(app)
                .SendEachForMulticastAsync(message, cancellationToken);

            var failedTokens = new List<string>();
            var invalidTokens = new List<string>();
            string? firstProviderMessageId = null;

            for (var index = 0; index < response.Responses.Count; index++)
            {
                var sendResponse = response.Responses[index];
                var token = tokens[index];

                if (sendResponse.IsSuccess)
                {
                    firstProviderMessageId ??= sendResponse.MessageId;
                    continue;
                }

                failedTokens.Add(token);

                if (sendResponse.Exception is FirebaseMessagingException messagingException &&
                    IsInvalidTokenError(messagingException))
                {
                    invalidTokens.Add(token);
                }
            }

            if (response.SuccessCount > 0)
            {
                var partialError = response.FailureCount > 0
                    ? $"Firebase FCM envio parcial: {response.SuccessCount} enviados, {response.FailureCount} fallidos."
                    : null;

                return new PushNotificationResult(
                    true,
                    firstProviderMessageId,
                    partialError,
                    response.SuccessCount,
                    response.FailureCount,
                    invalidTokens,
                    failedTokens);
            }

            return PushNotificationResult.Failed(
                $"Firebase FCM no envio ningun token. Fallidos: {response.FailureCount}.",
                response.FailureCount,
                invalidTokens,
                failedTokens);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error enviando push con Firebase FCM para UserId={UserId}.", request.UserId);
            return PushNotificationResult.Failed($"Error Firebase FCM: {exception.Message}", tokens.Count, null, tokens);
        }
    }

    private FirebaseApp GetOrCreateFirebaseApp()
    {
        if (_firebaseApp is not null) return _firebaseApp;

        lock (FirebaseAppLock)
        {
            if (_firebaseApp is not null) return _firebaseApp;

            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(_options.CredentialsPath),
                ProjectId = _options.ProjectId
            }, FirebaseAppName);

            _logger.LogInformation("Firebase FCM inicializado para ProjectId={ProjectId}.", _options.ProjectId);
            return _firebaseApp;
        }
    }

    private static IReadOnlyDictionary<string, string> NormalizeData(IReadOnlyDictionary<string, string> data)
    {
        if (data.Count == 0) return new Dictionary<string, string>();

        return data
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value ?? string.Empty, StringComparer.Ordinal);
    }

    private static bool IsInvalidTokenError(FirebaseMessagingException exception)
    {
        return exception.MessagingErrorCode is MessagingErrorCode.Unregistered
            or MessagingErrorCode.InvalidArgument
            or MessagingErrorCode.SenderIdMismatch;
    }
}
