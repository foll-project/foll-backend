using System.Text.Json;
using foll_backend.EmergencyAnalytics.Domain.Model.Commands;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.EmergencyAnalytics.Domain.Services;
using foll_backend.EmergencyAnalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Messaging;

public class EmergencyAnalyticsMqttSubscriberBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmergencyAnalyticsMqttSubscriberBackgroundService> _logger;
    private readonly EmergencyAnalyticsMqttOptions _mqttOptions;
    private IMqttClient? _mqttClient;

    public EmergencyAnalyticsMqttSubscriberBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<EmergencyAnalyticsMqttOptions> mqttOptions,
        ILogger<EmergencyAnalyticsMqttSubscriberBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _mqttOptions = mqttOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttClientFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += e => HandleMessageAsync(e);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    var optionsBuilder = new MqttClientOptionsBuilder()
                        .WithClientId(_mqttOptions.ClientId)
                        .WithTcpServer(_mqttOptions.Host, _mqttOptions.Port);

                    if (!string.IsNullOrWhiteSpace(_mqttOptions.Username))
                        optionsBuilder.WithCredentials(_mqttOptions.Username, _mqttOptions.Password);

                    var clientOptions = optionsBuilder.Build();
                    await _mqttClient.ConnectAsync(clientOptions, stoppingToken);

                    var subscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(_mqttOptions.FallDetectedTopic)
                        .WithTopicFilter(_mqttOptions.FallCancelledTopic)
                        .Build();

                    await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);

                    _logger.LogInformation(
                        "Emergency MQTT conectado a {Host}:{Port} y suscrito a {FallDetectedTopic} y {FallCancelledTopic}.",
                        _mqttOptions.Host,
                        _mqttOptions.Port,
                        _mqttOptions.FallDetectedTopic,
                        _mqttOptions.FallCancelledTopic);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error conectando o suscribiéndose al broker MQTT de EmergencyAnalytics.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        try
        {
            var topic = eventArgs.ApplicationMessage.Topic;
            var payload = eventArgs.ApplicationMessage.ConvertPayloadToString();

            if (string.IsNullOrWhiteSpace(payload))
                throw new InvalidOperationException("El mensaje MQTT llegó sin payload.");

            var deviceId = ParseDeviceId(topic);
            using var scope = _serviceScopeFactory.CreateScope();
            var commandService = scope.ServiceProvider.GetRequiredService<IEmergencyIncidentCommandService>();

            if (topic.EndsWith("/fall-detected", StringComparison.OrdinalIgnoreCase))
            {
                var message = JsonSerializer.Deserialize<MqttFallDetectedMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el evento MQTT de fall-detected.");

                if (message.DeviceId > 0 && message.DeviceId != deviceId)
                    throw new InvalidOperationException($"El device_id del payload ({message.DeviceId}) no coincide con el tópico ({deviceId}).");

                await commandService.Handle(new RegisterFallDetectedCommand(
                    deviceId,
                    message.FallTypeId,
                    message.FallTypeName ?? message.FallType,
                    message.ReportedAtUtc ?? DateTime.UtcNow,
                    message.AiConfidenceScore,
                    message.Latitude,
                    message.Longitude,
                    payload));
                return;
            }

            if (topic.EndsWith("/fall-cancelled", StringComparison.OrdinalIgnoreCase))
            {
                var message = JsonSerializer.Deserialize<MqttFallCancelledMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el evento MQTT de fall-cancelled.");

                if (message.DeviceId > 0 && message.DeviceId != deviceId)
                    throw new InvalidOperationException($"El device_id del payload ({message.DeviceId}) no coincide con el tópico ({deviceId}).");

                await commandService.Handle(new RegisterFallCancelledCommand(
                    deviceId,
                    message.ReportedAtUtc ?? DateTime.UtcNow,
                    ParseReason(message.Reason),
                    payload));
                return;
            }

            _logger.LogDebug("Tópico MQTT ignorado por EmergencyAnalytics: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando un mensaje MQTT de EmergencyAnalytics.");
        }
    }

    private static long ParseDeviceId(string topic)
    {
        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !long.TryParse(segments[^2], out var deviceId) || deviceId <= 0)
            throw new InvalidOperationException($"No se pudo obtener el deviceId desde el tópico '{topic}'.");

        return deviceId;
    }

    private static EmergencyCancellationReason ParseReason(string? reason)
    {
        return reason?.Trim().ToUpperInvariant() switch
        {
            "USER_BUTTON_PRESSED" => EmergencyCancellationReason.UserButtonPressed,
            _ => EmergencyCancellationReason.Unknown
        };
    }
}
