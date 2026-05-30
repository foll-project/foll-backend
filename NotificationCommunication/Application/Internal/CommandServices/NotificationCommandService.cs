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
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationCommandService(
        INotificationLogRepository notificationLogRepository,
        IUserPushTokenRepository userPushTokenRepository,
        IPatientNotificationAccessService patientNotificationAccessService,
        IPushNotificationSender pushNotificationSender,
        IUnitOfWork unitOfWork)
    {
        _notificationLogRepository = notificationLogRepository;
        _userPushTokenRepository = userPushTokenRepository;
        _patientNotificationAccessService = patientNotificationAccessService;
        _pushNotificationSender = pushNotificationSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateNotificationFromEventCommand command)
    {
        var recipient = await _patientNotificationAccessService.GetRecipientForPatientAsync(command.PatientId);
        if (recipient is null)
            throw new InvalidOperationException("No se encontro destinatario para la notificacion del paciente.");

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

        await _notificationLogRepository.AddAsync(notification);

        var tokens = await _userPushTokenRepository.ListActiveByUserIdAsync(recipient.UserId);
        if (tokens.Count == 0)
        {
            notification.MarkFailed("No hay tokens push activos para el cuidador.", DateTime.UtcNow);
            await _unitOfWork.CompleteAsync();
            return notification.NotificationLogId;
        }

        var result = await _pushNotificationSender.SendAsync(new PushNotificationRequest(
            recipient.UserId,
            tokens.Select(t => t.Token).ToList(),
            command.Title,
            command.Body,
            ParseData(command.DataJson)));

        if (result.Success)
        {
            notification.MarkSent(result.ProviderMessageId, DateTime.UtcNow);
            foreach (var token in tokens)
                token.MarkUsed(DateTime.UtcNow);
        }
        else
        {
            notification.MarkFailed(result.ErrorMessage ?? "Error enviando push notification.", DateTime.UtcNow);
        }

        await _unitOfWork.CompleteAsync();
        return notification.NotificationLogId;
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
}
