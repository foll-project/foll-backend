using System.Text.Json;
using foll_backend.DeviceManagment.Application.Internal.Notifications;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using foll_backend.Shared.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace foll_backend.DeviceManagment.Infrastructure.Messaging;

public class OutboxPublisherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;
    private readonly OutboxOptions _outboxOptions;

    public OutboxPublisherBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxOptions> outboxOptions,
        ILogger<OutboxPublisherBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _outboxOptions = outboxOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                var pendingMessages = await outboxRepository.ListPendingAsync(_outboxOptions.BatchSize);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        var notification = DeserializeNotification(message.Type, message.Payload);
                        await publisher.Publish(notification, stoppingToken);
                        message.MarkProcessed(DateTime.UtcNow);
                    }
                    catch (Exception ex)
                    {
                        message.MarkFailed(ex.Message);
                        _logger.LogError(ex, "Error publicando el mensaje outbox {OutboxMessageId}.", message.OutboxMessageId);
                    }
                }

                if (pendingMessages.Count > 0)
                    await unitOfWork.CompleteAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando la outbox de DeviceManagment.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_outboxOptions.PollIntervalSeconds), stoppingToken);
        }
    }

    private static INotification DeserializeNotification(string typeName, string payload)
    {
        switch (typeName)
        {
            case DeviceManagmentEventTypes.LowBatteryDetectedV1:
                return JsonSerializer.Deserialize<LowBatteryDetectedIntegrationEvent>(payload)
                    ?? throw new InvalidOperationException($"No se pudo deserializar el payload para '{typeName}'.");
            case DeviceManagmentEventTypes.LowBatteryResolvedV1:
                return JsonSerializer.Deserialize<LowBatteryResolvedIntegrationEvent>(payload)
                    ?? throw new InvalidOperationException($"No se pudo deserializar el payload para '{typeName}'.");
            case DeviceManagmentEventTypes.DeviceDisconnectedV1:
                return JsonSerializer.Deserialize<DeviceDisconnectedIntegrationEvent>(payload)
                    ?? throw new InvalidOperationException($"No se pudo deserializar el payload para '{typeName}'.");
            case DeviceManagmentEventTypes.DeviceReconnectedV1:
                return JsonSerializer.Deserialize<DeviceReconnectedIntegrationEvent>(payload)
                    ?? throw new InvalidOperationException($"No se pudo deserializar el payload para '{typeName}'.");
        }

        var notificationType = Type.GetType(typeName)
            ?? throw new InvalidOperationException($"No se encontró el tipo '{typeName}' para el mensaje outbox.");

        if (!typeof(INotification).IsAssignableFrom(notificationType))
            throw new InvalidOperationException($"El tipo '{typeName}' no implementa INotification.");

        var notification = JsonSerializer.Deserialize(payload, notificationType)
            ?? throw new InvalidOperationException($"No se pudo deserializar el payload para '{typeName}'.");

        return (INotification)notification;
    }
}
