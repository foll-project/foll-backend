using System.Text.Json;
using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;
using foll_backend.NotificationCommunication.Application.Internal.Services;
using foll_backend.NotificationCommunication.Application.OutboundServices;
using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.NotificationCommunication.Domain.Services;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.NotificationCommunication.Application.Internal.CommandServices;

public class NotificationCommandService : INotificationCommandService
{
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly IUserPushTokenRepository _userPushTokenRepository;
    private readonly IPatientNotificationAccessService _patientNotificationAccessService;
    private readonly INotificationRealtimePublisher _notificationRealtimePublisher;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly ISmsNotificationSender _smsNotificationSender;
    private readonly ISmsNotificationLogRepository _smsNotificationLogRepository;
    private readonly IEmergencyLocationLinkService _emergencyLocationLinkService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationCommandService(
        INotificationLogRepository notificationLogRepository,
        IUserPushTokenRepository userPushTokenRepository,
        IPatientNotificationAccessService patientNotificationAccessService,
        INotificationRealtimePublisher notificationRealtimePublisher,
        IPushNotificationSender pushNotificationSender,
        ISmsNotificationSender smsNotificationSender,
        ISmsNotificationLogRepository smsNotificationLogRepository,
        IEmergencyLocationLinkService emergencyLocationLinkService,
        IUnitOfWork unitOfWork)
    {
        _notificationLogRepository = notificationLogRepository;
        _userPushTokenRepository = userPushTokenRepository;
        _patientNotificationAccessService = patientNotificationAccessService;
        _notificationRealtimePublisher = notificationRealtimePublisher;
        _pushNotificationSender = pushNotificationSender;
        _smsNotificationSender = smsNotificationSender;
        _smsNotificationLogRepository = smsNotificationLogRepository;
        _emergencyLocationLinkService = emergencyLocationLinkService;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateNotificationFromEventCommand command)
    {
        var recipients = await _patientNotificationAccessService.GetRecipientsForPatientAsync(command.PatientId);
        if (recipients is null || (recipients.PushRecipients.Count == 0 && recipients.SmsRecipients.Count == 0))
            throw new InvalidOperationException("No se encontro destinatario para la notificacion del paciente.");

        var data = ParseData(command.DataJson);
        var incidentKey = TryGetIncidentKey(data);
        var shouldSendEmergencySms = command.NotificationType == NotificationType.FallDetected && incidentKey.HasValue;
        var smsPhoneNumbersHandled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var notifications = new List<NotificationLog>();
        foreach (var recipient in recipients.PushRecipients)
        {
            var notification = new NotificationLog(
                recipient.UserId,
                command.NotificationType,
                NotificationChannel.Push,
                command.Title,
                command.Body,
                command.DataJson,
                command.DeviceEventId,
                command.PatientId,
                command.DeviceId,
                DateTime.UtcNow);

            notifications.Add(notification);
            await _notificationLogRepository.AddAsync(notification);

            var tokens = await _userPushTokenRepository.ListActiveByUserIdAsync(recipient.UserId);
            if (tokens.Count == 0)
            {
                notification.MarkFailed("No hay tokens push activos para el cuidador.", DateTime.UtcNow);
                if (shouldSendEmergencySms && HasPhoneNumber(recipient.PhoneNumber))
                {
                    await SendEmergencySmsAsync(
                        command,
                        incidentKey!.Value,
                        new PatientSmsRecipientDto(
                            recipient.PatientId,
                            recipient.UserId,
                            null,
                            recipient.FullName,
                            recipient.PhoneNumber!,
                            "UserFallback"),
                        smsPhoneNumbersHandled);
                }

                continue;
            }

            try
            {
                var result = await _pushNotificationSender.SendAsync(new PushNotificationRequest(
                    recipient.UserId,
                    tokens.Select(t => t.Token).ToList(),
                    command.Title,
                    command.Body,
                    data));

                var processedAt = DateTime.UtcNow;
                DeactivateInvalidTokens(tokens, result.InvalidTokens, processedAt);

                if (result.Success)
                {
                    notification.MarkSent(result.ProviderMessageId, processedAt);
                    MarkSuccessfulTokensUsed(tokens, result.FailedTokens, result.InvalidTokens, processedAt);
                }
                else
                {
                    notification.MarkFailed(result.ErrorMessage ?? "Error enviando push notification.", processedAt);
                    if (shouldSendEmergencySms &&
                        HasPhoneNumber(recipient.PhoneNumber) &&
                        AllActiveTokensInvalid(tokens, result.InvalidTokens))
                    {
                        await SendEmergencySmsAsync(
                            command,
                            incidentKey!.Value,
                            new PatientSmsRecipientDto(
                                recipient.PatientId,
                                recipient.UserId,
                                null,
                                recipient.FullName,
                                recipient.PhoneNumber!,
                                "UserFallback"),
                            smsPhoneNumbersHandled);
                    }
                }
            }
            catch (Exception exception)
            {
                notification.MarkFailed(exception.Message, DateTime.UtcNow);
            }
        }

        if (shouldSendEmergencySms)
        {
            foreach (var recipient in recipients.SmsRecipients)
            {
                await SendEmergencySmsAsync(command, incidentKey!.Value, recipient, smsPhoneNumbersHandled);
            }
        }

        await _unitOfWork.CompleteAsync();
        foreach (var notification in notifications)
            await _notificationRealtimePublisher.PublishCreatedAsync(notification);

        return notifications.Count > 0 ? notifications[0].NotificationLogId : 0;
    }

    public async Task Handle(MarkNotificationReadCommand command)
    {
        var notification = await _notificationLogRepository.FindByIdAndUserIdAsync(command.NotificationLogId, command.UserId);
        if (notification is null) throw new InvalidOperationException("Notificacion no encontrada.");

        notification.MarkRead(DateTime.UtcNow);
        _notificationLogRepository.Update(notification);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(MarkNotificationAcknowledgedCommand command)
    {
        var notification = await _notificationLogRepository.FindByIdAndUserIdAsync(command.NotificationLogId, command.UserId);
        if (notification is null) throw new InvalidOperationException("Notificacion no encontrada.");

        notification.MarkAcknowledged(DateTime.UtcNow);
        _notificationLogRepository.Update(notification);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(DeleteNotificationsByAccountCommand command)
    {
        await _notificationLogRepository.DeleteByUserIdAsync(command.UserId);
        await _unitOfWork.CompleteAsync();
    }

    private static IReadOnlyDictionary<string, string> ParseData(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private void DeactivateInvalidTokens(IReadOnlyCollection<UserPushToken> tokens, IReadOnlyCollection<string> invalidTokenValues, DateTime updatedAt)
    {
        if (invalidTokenValues.Count == 0) return;

        var invalidTokenSet = invalidTokenValues.ToHashSet(StringComparer.Ordinal);
        foreach (var token in tokens.Where(token => invalidTokenSet.Contains(token.Token)))
        {
            token.Deactivate(updatedAt);
            _userPushTokenRepository.Update(token);
        }
    }

    private static void MarkSuccessfulTokensUsed(
        IReadOnlyCollection<UserPushToken> tokens,
        IReadOnlyCollection<string> failedTokenValues,
        IReadOnlyCollection<string> invalidTokenValues,
        DateTime usedAt)
    {
        var failedTokenSet = failedTokenValues.ToHashSet(StringComparer.Ordinal);
        var invalidTokenSet = invalidTokenValues.ToHashSet(StringComparer.Ordinal);

        foreach (var token in tokens)
        {
            if (failedTokenSet.Contains(token.Token) || invalidTokenSet.Contains(token.Token))
                continue;

            token.MarkUsed(usedAt);
        }
    }

    private async Task SendEmergencySmsAsync(
        CreateNotificationFromEventCommand command,
        Guid incidentKey,
        PatientSmsRecipientDto recipient,
        ISet<string> smsPhoneNumbersHandled)
    {
        var normalizedPhoneNumber = NormalizePeruvianPhoneNumber(recipient.PhoneNumber);
        if (string.IsNullOrWhiteSpace(normalizedPhoneNumber)) return;

        if (!smsPhoneNumbersHandled.Add(normalizedPhoneNumber)) return;

        var existingLog = await _smsNotificationLogRepository.FindByIncidentKeyAndPhoneNumberAsync(
            incidentKey,
            normalizedPhoneNumber);

        if (existingLog is not null) return;

        var now = DateTime.UtcNow;
        var locationLink = await _emergencyLocationLinkService.CreateLinkAsync(
            incidentKey,
            command.PatientId,
            command.DeviceId,
            now);

        var message = BuildEmergencySmsMessage(locationLink.Url);
        var smsLog = new SmsNotificationLog(
            incidentKey,
            recipient.UserId,
            recipient.EmergencyContactId,
            recipient.PatientId,
            command.DeviceId,
            recipient.FullName,
            normalizedPhoneNumber,
            command.NotificationType,
            message,
            locationLink.Url,
            now);

        await _smsNotificationLogRepository.AddAsync(smsLog);

        try
        {
            var result = await _smsNotificationSender.SendAsync(new SmsNotificationRequest(normalizedPhoneNumber, message));
            var processedAt = DateTime.UtcNow;

            if (result.Success)
                smsLog.MarkSent(result.ProviderMessageId, processedAt);
            else
                smsLog.MarkFailed(result.ErrorMessage ?? "Error enviando SMS.", processedAt);
        }
        catch (Exception exception)
        {
            smsLog.MarkFailed(exception.Message, DateTime.UtcNow);
        }
    }

    private static Guid? TryGetIncidentKey(IReadOnlyDictionary<string, string> data)
    {
        return data.TryGetValue("incidentKey", out var incidentKeyValue) &&
               Guid.TryParse(incidentKeyValue, out var incidentKey)
            ? incidentKey
            : null;
    }

    private static bool HasPhoneNumber(string? phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(NormalizePeruvianPhoneNumber(phoneNumber));
    }

    private static bool AllActiveTokensInvalid(
        IReadOnlyCollection<UserPushToken> tokens,
        IReadOnlyCollection<string> invalidTokenValues)
    {
        if (tokens.Count == 0 || invalidTokenValues.Count == 0) return false;

        var invalidTokenSet = invalidTokenValues.ToHashSet(StringComparer.Ordinal);
        return tokens.All(token => invalidTokenSet.Contains(token.Token));
    }

    private static string? NormalizePeruvianPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

        var trimmed = phoneNumber.Trim();
        var hasPlus = trimmed.StartsWith("+", StringComparison.Ordinal);
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (hasPlus && digits.StartsWith("51", StringComparison.Ordinal) && digits.Length == 11)
            return $"+{digits}";

        if (!hasPlus && digits.StartsWith("51", StringComparison.Ordinal) && digits.Length == 11)
            return $"+{digits}";

        if (!hasPlus && digits.Length == 9)
            return $"+51{digits}";

        return null;
    }

    private static string BuildEmergencySmsMessage(string locationUrl)
    {
        return $"Foll alerta: posible caida detectada. Ubicacion temporal: {locationUrl}";
    }
}
