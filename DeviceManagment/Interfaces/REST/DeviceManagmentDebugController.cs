using System.Text;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.DeviceManagment.Interfaces.REST;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/device-managment/debug")]
public class DeviceManagmentDebugController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeviceManagmentDebugController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("events")]
    public async Task<ContentResult> GetEventsDashboard()
    {
        var devices = await _context.Devices
            .OrderBy(d => d.DeviceId)
            .ToListAsync();

        var deviceEvents = await _context.DeviceEvents
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .ToListAsync();

        var outboxMessages = await _context.OutboxMessages
            .OrderByDescending(o => o.OccurredOn)
            .Take(50)
            .ToListAsync();

        var html = new StringBuilder();
        html.AppendLine("""
<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <meta http-equiv="refresh" content="5" />
  <title>DeviceManagment Debug</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 24px; background: #111827; color: #f9fafb; }
    h1, h2 { margin-bottom: 8px; }
    .muted { color: #9ca3af; margin-bottom: 20px; }
    table { width: 100%; border-collapse: collapse; margin-bottom: 24px; background: #1f2937; }
    th, td { border: 1px solid #374151; padding: 8px; text-align: left; vertical-align: top; font-size: 14px; }
    th { background: #0f172a; }
    .ok { color: #34d399; font-weight: bold; }
    .warn { color: #fbbf24; font-weight: bold; }
    .bad { color: #f87171; font-weight: bold; }
    code { color: #93c5fd; white-space: pre-wrap; word-break: break-word; }
  </style>
</head>
<body>
  <h1>DeviceManagment Debug</h1>
  <div class="muted">Actualiza cada 5 segundos. Aquí puedes ver snapshot, anomalías y outbox.</div>
""");

        html.AppendLine("<h2>Devices</h2>");
        html.AppendLine("<table><thead><tr><th>DeviceId</th><th>AssignedPatientId</th><th>Status</th><th>Connectivity</th><th>Battery</th><th>Charging</th><th>MonitoringStartedAt</th><th>LastHeartbeatAt</th><th>LastConnectivityChangeAt</th><th>Firmware</th></tr></thead><tbody>");
        foreach (var device in devices)
        {
            var connectivityClass = device.ConnectivityStatus?.ToString() == "Connected" ? "ok" : "warn";
            html.AppendLine($"""
<tr>
  <td>{device.DeviceId}</td>
  <td>{device.AssignedPatientId?.ToString() ?? "-"}</td>
  <td>{device.Status}</td>
  <td class="{connectivityClass}">{device.ConnectivityStatus?.ToString() ?? "-"}</td>
  <td>{device.CurrentBatteryLevel?.ToString() ?? "-"}</td>
  <td>{device.IsCharging?.ToString() ?? "-"}</td>
  <td>{device.MonitoringStartedAt?.ToString("u") ?? "-"}</td>
  <td>{device.LastHeartbeatAt?.ToString("u") ?? "-"}</td>
  <td>{device.LastConnectivityChangeAt?.ToString("u") ?? "-"}</td>
  <td>{System.Net.WebUtility.HtmlEncode(device.FirmwareVersion)}</td>
</tr>
""");
        }
        html.AppendLine("</tbody></table>");

        html.AppendLine("<h2>Device Events</h2>");
        html.AppendLine("<table><thead><tr><th>Id</th><th>DeviceId</th><th>Type</th><th>Resolved</th><th>CreatedAt</th><th>ResolvedAt</th><th>Payload</th></tr></thead><tbody>");
        foreach (var deviceEvent in deviceEvents)
        {
            var resolvedClass = deviceEvent.IsResolved ? "ok" : "warn";
            html.AppendLine($"""
<tr>
  <td>{deviceEvent.DeviceEventId}</td>
  <td>{deviceEvent.DeviceId}</td>
  <td>{deviceEvent.EventType}</td>
  <td class="{resolvedClass}">{deviceEvent.IsResolved}</td>
  <td>{deviceEvent.CreatedAt:u}</td>
  <td>{deviceEvent.ResolvedAt?.ToString("u") ?? "-"}</td>
  <td><code>{System.Net.WebUtility.HtmlEncode(deviceEvent.EventPayload)}</code></td>
</tr>
""");
        }
        html.AppendLine("</tbody></table>");

        html.AppendLine("<h2>Outbox Messages</h2>");
        html.AppendLine("<table><thead><tr><th>Id</th><th>Type</th><th>OccurredOn</th><th>ProcessedOn</th><th>RetryCount</th><th>Error</th><th>Payload</th></tr></thead><tbody>");
        foreach (var outboxMessage in outboxMessages)
        {
            var outboxClass = outboxMessage.ProcessedOn.HasValue ? "ok" : "bad";
            html.AppendLine($"""
<tr>
  <td>{outboxMessage.OutboxMessageId}</td>
  <td class="{outboxClass}">{System.Net.WebUtility.HtmlEncode(outboxMessage.Type)}</td>
  <td>{outboxMessage.OccurredOn:u}</td>
  <td>{outboxMessage.ProcessedOn?.ToString("u") ?? "-"}</td>
  <td>{outboxMessage.RetryCount}</td>
  <td>{System.Net.WebUtility.HtmlEncode(outboxMessage.Error ?? "-")}</td>
  <td><code>{System.Net.WebUtility.HtmlEncode(outboxMessage.Payload)}</code></td>
</tr>
""");
        }
        html.AppendLine("</tbody></table>");

        html.AppendLine("</body></html>");

        return Content(html.ToString(), "text/html", Encoding.UTF8);
    }
}
