using System.Text.Json;
using System.Text.Json.Serialization;

namespace foll_backend.EmergencyAnalytics.Domain.Model.Entities;

public class EmergencyOutboxMessage
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public long EmergencyOutboxMessageId { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    protected EmergencyOutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public EmergencyOutboxMessage(string type, string payload, DateTime occurredOn)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("El tipo del mensaje es obligatorio.", nameof(type));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("El payload del mensaje es obligatorio.", nameof(payload));

        Type = type.Trim();
        Payload = payload.Trim();
        OccurredOn = occurredOn;
    }

    public static EmergencyOutboxMessage Create<TMessage>(string type, TMessage message, DateTime occurredOn)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new EmergencyOutboxMessage(type, JsonSerializer.Serialize(message, SerializerOptions), occurredOn);
    }

    public void MarkProcessed(DateTime processedOn)
    {
        ProcessedOn = processedOn;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = string.IsNullOrWhiteSpace(error) ? "Error desconocido al publicar el mensaje." : error.Trim();
        RetryCount++;
    }
}
