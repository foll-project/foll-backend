using System.Text.Json;
using foll_backend.ExternalServices.Application.OutboundServices;
using foll_backend.ExternalServices.Domain.Model;
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
    private readonly IUnitOfWork _unitOfWork;

    public NotificationCommandService(
        INotificationLogRepository notificationLogRepository,
        IUserPushTokenRepository userPushTokenRepository,
        IPatientNotificationAccessService patientNotificationAccessService,
        INotificationRealtimePublisher notificationRealtimePublisher,
        IPushNotificationSender pushNotificationSender,
        IUnitOfWork unitOfWork)
    {
        _notificationLogRepository = notificationLogRepository;
        _userPushTokenRepository = userPushTokenRepository;
        _patientNotificationAccessService = patientNotificationAccessService;
        _notificationRealtimePublisher = notificationRealtimePublisher;
        _pushNotificationSender = pushNotificationSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateNotificationFromEventCommand command)
    {
        var recipients = await _patientNotificationAccessService.GetRecipientsForPatientAsync(command.PatientId);
        if (recipients.Count == 0)
            throw new InvalidOperationException("No se encontro destinatario para la notificacion del paciente.");

        var notifications = new List<NotificationLog>();
        foreach (var recipient in recipients)
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
                continue;
            }

            try
            {
                var result = await _pushNotificationSender.SendAsync(new PushNotificationRequest(
                    recipient.UserId,
                    tokens.Select(t => t.Token).ToList(),
                    command.Title,
                    command.Body,
                    ParseData(command.DataJson)));

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
                }
            }
            catch (Exception exception)
            {
                notification.MarkFailed(exception.Message, DateTime.UtcNow);
            }
        }

        await _unitOfWork.CompleteAsync();
        foreach (var notification in notifications)
            await _notificationRealtimePublisher.PublishCreatedAsync(notification);

        return notifications[0].NotificationLogId;
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
}
