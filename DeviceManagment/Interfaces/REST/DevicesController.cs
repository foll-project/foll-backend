using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using ConnectivityStatus = foll_backend.DeviceManagment.Domain.Model.Enums.ConnectivityStatus;
using foll_backend.DeviceManagment.Domain.Model.Commands;
using foll_backend.DeviceManagment.Domain.Model.Queries;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using foll_backend.DeviceManagment.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace foll_backend.DeviceManagment.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceCommandService _commandService;
    private readonly IDeviceQueryService _queryService;
    private readonly DeviceMonitoringOptions _monitoringOptions;

    public DevicesController(
        IDeviceCommandService commandService,
        IDeviceQueryService queryService,
        IOptions<DeviceMonitoringOptions> monitoringOptions)
    {
        _commandService = commandService;
        _queryService = queryService;
        _monitoringOptions = monitoringOptions.Value;
    }

    [HttpPost("{deviceId:long}/link")]
    public async Task<IActionResult> Link([FromRoute] long deviceId, [FromBody] LinkDeviceResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new LinkDeviceCommand(userId, deviceId, resource.PatientId));
            return Ok(new { message = "Dispositivo vinculado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{deviceId:long}/link")]
    public async Task<IActionResult> Unlink([FromRoute] long deviceId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new UnlinkDeviceCommand(userId, deviceId));
            return Ok(new { message = "Dispositivo desvinculado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{deviceId:long}/status")]
    public async Task<IActionResult> GetStatus([FromRoute] long deviceId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var device = await _queryService.Handle(new GetDeviceStatusByIdQuery(userId, deviceId));
            if (device is null) return NotFound(new { message = "Dispositivo no encontrado." });

            return Ok(ToResponse(device));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("patient/{patientId:long}")]
    public async Task<IActionResult> GetByPatient([FromRoute] long patientId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var device = await _queryService.Handle(new GetDeviceByPatientIdQuery(userId, patientId));
            if (device is null) return NotFound(new { message = "No hay dispositivo vinculado a este paciente." });

            return Ok(ToResponse(device));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private object ToResponse(DeviceEntity device)
    {
        var isOnline = device.ConnectivityStatus == ConnectivityStatus.Connected;

        var isLowBattery = device.CurrentBatteryLevel.HasValue &&
                           device.CurrentBatteryLevel.Value < _monitoringOptions.LowBatteryThreshold &&
                           device.IsCharging == false;

        return new
        {
            deviceId = device.DeviceId,
            assignedPatientId = device.AssignedPatientId,
            status = device.Status.ToString(),
            connectivityStatus = device.ConnectivityStatus?.ToString(),
            currentBatteryLevel = device.CurrentBatteryLevel,
            isCharging = device.IsCharging,
            lastHeartbeatAt = device.LastHeartbeatAt,
            monitoringStartedAt = device.MonitoringStartedAt,
            lastConnectivityChangeAt = device.LastConnectivityChangeAt,
            isOnline,
            isLowBattery,
            firmwareVersion = device.FirmwareVersion
        };
    }

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT inválido: userId no encontrado.");
        return userId;
    }
}
