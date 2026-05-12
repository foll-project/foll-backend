using System.Text.Json;
using foll_backend.DeviceManagment.Domain.Model.Enums;

namespace foll_backend.DeviceManagment.Domain.Model.Entities;

public class DeviceEvent
{
    public long DeviceEventId { get; private set; }
    public long DeviceId { get; private set; }
    public DeviceEventType EventType { get; private set; }
    public string EventPayload { get; private set; }
    public bool IsResolved { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    protected DeviceEvent()
    {
        EventPayload = string.Empty;
    }

    public DeviceEvent(long deviceId, DeviceEventType eventType, string eventPayload, DateTime createdAt)
    {
        if (deviceId <= 0) throw new ArgumentOutOfRangeException(nameof(deviceId));
        if (string.IsNullOrWhiteSpace(eventPayload)) throw new ArgumentException("El payload del evento es obligatorio.", nameof(eventPayload));

        DeviceId = deviceId;
        EventType = eventType;
        EventPayload = eventPayload.Trim();
        CreatedAt = NormalizeTimestamp(createdAt);
    }

    public static DeviceEvent CreateLowBatteryDetected(long deviceId, short batteryLevel, DateTime reportedAtUtc)
    {
        var payload = JsonSerializer.Serialize(new
        {
            batteryLevel,
            reportedAtUtc = NormalizeTimestamp(reportedAtUtc)
        });

        return new DeviceEvent(deviceId, DeviceEventType.LowBatteryDetected, payload, reportedAtUtc);
    }

    public static DeviceEvent CreateDeviceDisconnected(long deviceId, DateTime detectedAtUtc, DateTime lastSeenAtUtc)
    {
        return CreateDeviceDisconnected(deviceId, detectedAtUtc, lastSeenAtUtc, DeviceManagment.Domain.Model.Enums.ConnectivityChangeCause.HeartbeatTimeout);
    }

    public static DeviceEvent CreateDeviceDisconnected(long deviceId, DateTime detectedAtUtc, DateTime lastSeenAtUtc, ConnectivityChangeCause cause)
    {
        var payload = JsonSerializer.Serialize(new
        {
            detectedAtUtc = NormalizeTimestamp(detectedAtUtc),
            lastSeenAtUtc = NormalizeTimestamp(lastSeenAtUtc),
            cause = cause.ToString()
        });

        return new DeviceEvent(deviceId, DeviceEventType.DeviceDisconnected, payload, detectedAtUtc);
    }

    public void Resolve(DateTime resolvedAt)
    {
        if (IsResolved) return;

        IsResolved = true;
        ResolvedAt = NormalizeTimestamp(resolvedAt);
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
