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
                    _logger.LogInformation(
                        "MQTT subscriber intentando conectar a {Host}:{Port} con ClientId={ClientId}.",
                        _mqttOptions.Host,
                        _mqttOptions.Port,
                        _mqttOptions.ClientId);

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
                        "MQTT subscriber conectado a {Host}:{Port} y suscrito a HeartbeatTopic={HeartbeatTopic}, PowerTopic={PowerTopic}.",
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

            _logger.LogDebug("MQTT mensaje recibido. Topic={Topic}", topic);
            _logger.LogDebug("MQTT payload recibido. Topic={Topic} Payload={Payload}", topic, payload);

            if (string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("MQTT mensaje ignorado. Topic={Topic} Reason=Payload vacio.", topic);
                throw new InvalidOperationException("El mensaje MQTT llegó sin payload.");
            }

            var deviceId = ParseDeviceId(topic);
            _logger.LogDebug("MQTT deviceId detectado desde topic. Topic={Topic} DeviceId={DeviceId}", topic, deviceId);

            using var scope = _serviceScopeFactory.CreateScope();
            var commandService = scope.ServiceProvider.GetRequiredService<IDeviceCommandService>();

            if (topic.EndsWith("/heartbeat", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("MQTT topic interpretado como heartbeat. Topic={Topic} DeviceId={DeviceId}", topic, deviceId);

                var heartbeat = JsonSerializer.Deserialize<MqttHeartbeatMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el heartbeat MQTT.");

                _logger.LogDebug(
                    "MQTT heartbeat deserializado correctamente. TopicDeviceId={TopicDeviceId} PayloadDeviceId={PayloadDeviceId} BatteryLevel={BatteryLevel} IsCharging={IsCharging} ReportedAtUtc={ReportedAtUtc} FirmwareVersion={FirmwareVersion}",
                    deviceId,
                    heartbeat.DeviceId,
                    heartbeat.BatteryLevel,
                    heartbeat.IsCharging,
                    heartbeat.ReportedAtUtc,
                    heartbeat.FirmwareVersion);

                if (heartbeat.DeviceId > 0 && heartbeat.DeviceId != deviceId)
                {
                    _logger.LogWarning(
                        "MQTT heartbeat ignorado. Topic={Topic} Reason=Payload device_id {PayloadDeviceId} no coincide con topic deviceId {TopicDeviceId}.",
                        topic,
                        heartbeat.DeviceId,
                        deviceId);
                    throw new InvalidOperationException($"El device_id del payload ({heartbeat.DeviceId}) no coincide con el tópico ({deviceId}).");
                }

                var reportedAtUtc = heartbeat.ReportedAtUtc ?? DateTime.UtcNow;
                _logger.LogDebug(
                    "MQTT llamando RegisterDeviceTelemetry. DeviceId={DeviceId} BatteryLevel={BatteryLevel} IsCharging={IsCharging} ReportedAtUtc={ReportedAtUtc} FirmwareVersion={FirmwareVersion}",
                    deviceId,
                    heartbeat.BatteryLevel,
                    heartbeat.IsCharging,
                    reportedAtUtc,
                    heartbeat.FirmwareVersion);

                await commandService.Handle(new RegisterDeviceTelemetryCommand(
                    deviceId,
                    heartbeat.BatteryLevel,
                    heartbeat.IsCharging,
                    reportedAtUtc,
                    heartbeat.FirmwareVersion));

                _logger.LogDebug("MQTT heartbeat procesado por RegisterDeviceTelemetry. DeviceId={DeviceId}", deviceId);
                return;
            }

            if (topic.EndsWith("/power", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("MQTT topic interpretado como power. Topic={Topic} DeviceId={DeviceId}", topic, deviceId);

                var power = JsonSerializer.Deserialize<MqttPowerMessage>(payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("No se pudo deserializar el evento MQTT de power.");

                _logger.LogDebug(
                    "MQTT power deserializado correctamente. TopicDeviceId={TopicDeviceId} PayloadDeviceId={PayloadDeviceId} IsActive={IsActive} ReportedAtUtc={ReportedAtUtc}",
                    deviceId,
                    power.DeviceId,
                    power.IsActive,
                    power.ReportedAtUtc);

                if (power.DeviceId > 0 && power.DeviceId != deviceId)
                {
                    _logger.LogWarning(
                        "MQTT power ignorado. Topic={Topic} Reason=Payload device_id {PayloadDeviceId} no coincide con topic deviceId {TopicDeviceId}.",
                        topic,
                        power.DeviceId,
                        deviceId);
                    throw new InvalidOperationException($"El device_id del payload ({power.DeviceId}) no coincide con el tópico ({deviceId}).");
                }

                var reportedAtUtc = power.ReportedAtUtc ?? DateTime.UtcNow;
                _logger.LogDebug(
                    "MQTT llamando UpdateDevicePowerState. DeviceId={DeviceId} IsActive={IsActive} ReportedAtUtc={ReportedAtUtc}",
                    deviceId,
                    power.IsActive,
                    reportedAtUtc);

                await commandService.Handle(new UpdateDevicePowerStateCommand(
                    deviceId,
                    power.IsActive,
                    reportedAtUtc));

                _logger.LogDebug("MQTT power procesado por UpdateDevicePowerState. DeviceId={DeviceId}", deviceId);
                return;
            }

            _logger.LogInformation(
                "MQTT mensaje ignorado. Topic={Topic} Reason=El topic no termina en /heartbeat ni /power.",
                topic);
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
