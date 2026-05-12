using System.Text.Json;
using foll_backend.DeviceManagment.Domain.Model.Commands;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace foll_backend.DeviceManagment.Infrastructure.Messaging;

public class MqttHeartbeatSubscriberBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MqttHeartbeatSubscriberBackgroundService> _logger;
    private readonly MqttOptions _mqttOptions;
    private IMqttClient? _mqttClient;

    public MqttHeartbeatSubscriberBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MqttOptions> mqttOptions,
        ILogger<MqttHeartbeatSubscriberBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _mqttOptions = mqttOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttClientFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += e => HandleMessageAsync(e, stoppingToken);

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
                        .WithTopicFilter(_mqttOptions.HeartbeatTopic)
                        .WithTopicFilter(_mqttOptions.PowerTopic)
                        .Build();

                    await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);

                    _logger.LogInformation(
                        "Subscriber MQTT conectado a {Host}:{Port} y suscrito a {HeartbeatTopic} y {PowerTopic}.",
                        _mqttOptions.Host,
                        _mqttOptions.Port,
                        _mqttOptions.HeartbeatTopic,
                        _mqttOptions.PowerTopic);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error conectando o suscribiéndose al broker MQTT.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            var topic = eventArgs.ApplicationMessage.Topic;
            var payload = eventArgs.ApplicationMessage.ConvertPayloadToString();

            if (string.IsNullOrWhiteSpace(payload))
                throw new InvalidOperationException("El mensaje MQTT llegó sin payload.");

            var deviceId = ParseDeviceId(topic);
            using var scope = _serviceScopeFactory.CreateScope();
            var commandService = scope.ServiceProvider.GetRequiredService<IDeviceCommandService>();

            if (topic.EndsWith("/heartbeat", StringComparison.OrdinalIgnoreCase))
            {
                var heartbeat = JsonSerializer.Deserialize<MqttHeartbeatMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el heartbeat MQTT.");

                if (heartbeat.DeviceId > 0 && heartbeat.DeviceId != deviceId)
                    throw new InvalidOperationException($"El device_id del payload ({heartbeat.DeviceId}) no coincide con el tópico ({deviceId}).");

                await commandService.Handle(new RegisterDeviceTelemetryCommand(
                    deviceId,
                    heartbeat.BatteryLevel,
                    heartbeat.IsCharging,
                    heartbeat.ReportedAtUtc ?? DateTime.UtcNow,
                    heartbeat.FirmwareVersion));
                return;
            }

            if (topic.EndsWith("/power", StringComparison.OrdinalIgnoreCase))
            {
                var power = JsonSerializer.Deserialize<MqttPowerMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el evento MQTT de power.");

                if (power.DeviceId > 0 && power.DeviceId != deviceId)
                    throw new InvalidOperationException($"El device_id del payload ({power.DeviceId}) no coincide con el tópico ({deviceId}).");

                await commandService.Handle(new UpdateDevicePowerStateCommand(
                    deviceId,
                    power.IsActive,
                    power.ReportedAtUtc ?? DateTime.UtcNow));
                return;
            }

            _logger.LogDebug("Tópico MQTT ignorado: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando un heartbeat MQTT.");
        }
    }

    private static long ParseDeviceId(string topic)
    {
        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !long.TryParse(segments[^2], out var deviceId) || deviceId <= 0)
            throw new InvalidOperationException($"No se pudo obtener el deviceId desde el tópico '{topic}'.");

        return deviceId;
    }
}
