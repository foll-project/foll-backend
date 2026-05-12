using System.Text.Json;

namespace foll_backend.Shared.Domain.Model.Entities;

public class OutboxMessage
{
    public long OutboxMessageId { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    protected OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public OutboxMessage(string type, string payload, DateTime occurredOn)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("El tipo del mensaje es obligatorio.", nameof(type));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("El payload del mensaje es obligatorio.", nameof(payload));

        Type = type.Trim();
        Payload = payload.Trim();
        OccurredOn = occurredOn;
    }

    public static OutboxMessage Create<TMessage>(TMessage message, DateTime occurredOn)
    {
        ArgumentNullException.ThrowIfNull(message);

        var type = message.GetType().AssemblyQualifiedName ?? message.GetType().FullName ?? message.GetType().Name;
        var payload = JsonSerializer.Serialize(message);
        return new OutboxMessage(type, payload, occurredOn);
    }

    public static OutboxMessage Create<TMessage>(string type, TMessage message, DateTime occurredOn)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("El tipo del mensaje es obligatorio.", nameof(type));

        var payload = JsonSerializer.Serialize(message);
        return new OutboxMessage(type, payload, occurredOn);
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
