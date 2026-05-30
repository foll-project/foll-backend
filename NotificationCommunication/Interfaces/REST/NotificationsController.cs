using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using foll_backend.NotificationCommunication.Domain.Model.Queries;
using foll_backend.NotificationCommunication.Domain.Services;
using foll_backend.NotificationCommunication.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.NotificationCommunication.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationCommandService _notificationCommandService;
    private readonly INotificationQueryService _notificationQueryService;
    private readonly IUserPushTokenCommandService _userPushTokenCommandService;
    private readonly IUserPushTokenQueryService _userPushTokenQueryService;

    public NotificationsController(
        INotificationCommandService notificationCommandService,
        INotificationQueryService notificationQueryService,
        IUserPushTokenCommandService userPushTokenCommandService,
        IUserPushTokenQueryService userPushTokenQueryService)
    {
        _notificationCommandService = notificationCommandService;
        _notificationQueryService = notificationQueryService;
        _userPushTokenCommandService = userPushTokenCommandService;
        _userPushTokenQueryService = userPushTokenQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> ListNotifications()
    {
        var userId = GetUserIdOrThrow();
        var notifications = await _notificationQueryService.Handle(new ListNotificationsForUserQuery(userId));
        return Ok(notifications.Select(ToNotificationResponse));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetNotification([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();
        var notification = await _notificationQueryService.Handle(new GetNotificationByIdQuery(userId, id));
        if (notification is null) return NotFound(new { message = "Notificacion no encontrada." });

        return Ok(ToNotificationResponse(notification));
    }

    [HttpGet("{id:long}/delivery-status")]
    public async Task<IActionResult> GetDeliveryStatus([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();
        var notification = await _notificationQueryService.Handle(new GetNotificationDeliveryStatusQuery(userId, id));
        if (notification is null) return NotFound(new { message = "Notificacion no encontrada." });

        return Ok(ToDeliveryStatusResponse(notification));
    }

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _notificationCommandService.Handle(new MarkNotificationReadCommand(userId, id));
            return Ok(new { message = "Notificacion marcada como leida." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/acknowledge")]
    public async Task<IActionResult> Acknowledge([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _notificationCommandService.Handle(new MarkNotificationAcknowledgedCommand(userId, id));
            return Ok(new { message = "Notificacion atendida." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("push-tokens")]
    public async Task<IActionResult> RegisterPushToken([FromBody] RegisterPushTokenResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var tokenId = await _userPushTokenCommandService.Handle(new RegisterUserPushTokenCommand(
                userId,
                resource.Token,
                ParsePlatform(resource.Platform),
                resource.DeviceName));

            return Ok(new { userPushTokenId = tokenId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("push-tokens")]
    public async Task<IActionResult> ListPushTokens()
    {
        var userId = GetUserIdOrThrow();
        var tokens = await _userPushTokenQueryService.Handle(new ListUserPushTokensQuery(userId));
        return Ok(tokens.Select(ToUserPushTokenResponse));
    }

    [HttpDelete("push-tokens/{id:long}")]
    public async Task<IActionResult> DeactivatePushToken([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _userPushTokenCommandService.Handle(new DeactivateUserPushTokenCommand(userId, id));
            return Ok(new { message = "Token push desactivado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static NotificationResponse ToNotificationResponse(NotificationLog notification)
    {
        return new NotificationResponse(
            notification.NotificationLogId,
            notification.UserId,
            notification.NotificationType.ToString(),
            notification.NotificationChannel.ToString(),
            notification.NotificationStatus.ToString(),
            notification.Title,
            notification.Body,
            notification.DataJson,
            notification.ProviderMessageId,
            notification.ErrorMessage,
            notification.DeviceEventId,
            notification.PatientId,
            notification.DeviceId,
            notification.SentAt,
            notification.ReadAt,
            notification.AcknowledgedAt,
            notification.CreatedAt,
            notification.UpdatedAt);
    }

    private static NotificationDeliveryStatusResponse ToDeliveryStatusResponse(NotificationLog notification)
    {
        return new NotificationDeliveryStatusResponse(
            notification.NotificationLogId,
            notification.NotificationStatus.ToString(),
            notification.ProviderMessageId,
            notification.ErrorMessage,
            notification.SentAt,
            notification.ReadAt,
            notification.AcknowledgedAt,
            notification.UpdatedAt);
    }

    private static UserPushTokenResponse ToUserPushTokenResponse(UserPushToken token)
    {
        return new UserPushTokenResponse(
            token.UserPushTokenId,
            token.UserId,
            token.Token,
            token.Platform.ToString(),
            token.DeviceName,
            token.IsActive,
            token.LastUsedAt,
            token.CreatedAt,
            token.UpdatedAt);
    }

    private static PushPlatform ParsePlatform(string? platform)
    {
        if (Enum.TryParse<PushPlatform>(platform, ignoreCase: true, out var parsed))
            return parsed;

        return PushPlatform.Unknown;
    }

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT invalido: userId no encontrado.");
        return userId;
    }
}
