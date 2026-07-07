using System.Text.Json;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Messaging;

public sealed class EmergencyAnalyticsMqttPublisher : IEmergencyAnalyticsMqttPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    private readonly EmergencyAnalyticsMqttOptions _options;
    private readonly ILogger<EmergencyAnalyticsMqttPublisher> _logger;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private IMqttClient? _client;

    public EmergencyAnalyticsMqttPublisher(
        IOptions<EmergencyAnalyticsMqttOptions> options,
        ILogger<EmergencyAnalyticsMqttPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishIncidentClosedAsync(
        long deviceId,
        string status,
        string? cancellationReason,
        DateTime closedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (deviceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(deviceId));

        await EnsureConnectedAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            device_id = deviceId,
            status,
            cancellation_reason = cancellationReason,
            timestamp = closedAtUtc.ToUniversalTime().ToString("O")
        }, JsonSerializerOptions);

        var topic = BuildIncidentClosedTopic(deviceId);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        if (_client is null || !_client.IsConnected)
            throw new InvalidOperationException("El cliente MQTT de publicación no está conectado.");

        await _client.PublishAsync(message, cancellationToken);

        _logger.LogInformation(
            "MQTT incident-closed publicado. Topic={Topic} DeviceId={DeviceId} Status={Status}",
            topic,
            deviceId,
            status);
    }

    private string BuildIncidentClosedTopic(long deviceId)
    {
        return _options.IncidentClosedTopic
            .Replace("{deviceId}", deviceId.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("+", deviceId.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected == true)
            return;

        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            if (_client?.IsConnected == true)
                return;

            _client ??= new MqttClientFactory().CreateMqttClient();

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(_options.PublisherClientId)
                .WithTcpServer(_options.Host, _options.Port);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                optionsBuilder.WithCredentials(_options.Username, _options.Password);

            await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

            _logger.LogInformation(
                "Emergency MQTT publisher conectado a {Host}:{Port} como {ClientId}.",
                _options.Host,
                _options.Port,
                _options.PublisherClientId);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is null)
            return;

        if (_client.IsConnected)
            await _client.DisconnectAsync();

        _client.Dispose();
        _connectLock.Dispose();
    }
}
