using foll_backend.EmergencyAnalytics.Domain.Model.Commands;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Queries;
using foll_backend.EmergencyAnalytics.Domain.Services;
using foll_backend.EmergencyAnalytics.Interfaces.REST.Resources;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.EmergencyAnalytics.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/emergency/incidents")]
public class EmergencyIncidentsController : ControllerBase
{
    private readonly IEmergencyIncidentCommandService _commandService;
    private readonly IEmergencyIncidentQueryService _queryService;

    public EmergencyIncidentsController(
        IEmergencyIncidentCommandService commandService,
        IEmergencyIncidentQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpGet("active/patient/{patientId:long}")]
    public async Task<IActionResult> GetActiveByPatient([FromRoute] long patientId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var incident = await _queryService.Handle(new GetActiveFallIncidentByPatientIdQuery(userId, patientId));
            if (incident is null) return NotFound(new { message = "No hay caída activa para este paciente." });

            return Ok(ToResponse(incident));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history/patient/{patientId:long}")]
    public async Task<IActionResult> GetHistoryByPatient([FromRoute] long patientId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var incidents = await _queryService.Handle(new ListFallIncidentHistoryByPatientIdQuery(userId, patientId));
            return Ok(incidents.Select(ToResponse));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history/patient/{patientId:long}/monthly")]
    public async Task<IActionResult> GetMonthlyHistoryByPatient([FromRoute] long patientId, [FromQuery] int month, [FromQuery] int year, [FromServices] IMediator mediator)
    {
        var userId = GetUserIdOrThrow();

        if (month < 1 || month > 12 || year < 2000 || year > 2100)
            return BadRequest(new { message = "Mes o año inválidos." });

        try
        {
            var incidents = await mediator.Send(new GetPatientFallsByMonthQuery(userId, patientId, month, year));
            return Ok(incidents.Select(ToResponse));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{incidentId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long incidentId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var incident = await _queryService.Handle(new GetEmergencyIncidentByIdQuery(userId, incidentId));
            if (incident is null) return NotFound(new { message = "Incidente no encontrado." });

            return Ok(ToResponse(incident));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{incidentId:long}/false-positive")]
    public async Task<IActionResult> MarkFalsePositive([FromRoute] long incidentId, [FromBody] CloseEmergencyIncidentResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new MarkFallIncidentFalsePositiveCommand(
                incidentId,
                userId,
                resource.Observation,
                DateTime.UtcNow));

            return Ok(new { message = "Incidente marcado como falso positivo." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{incidentId:long}/resolve")]
    public async Task<IActionResult> Resolve([FromRoute] long incidentId, [FromBody] CloseEmergencyIncidentResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new ResolveFallIncidentCommand(
                incidentId,
                userId,
                resource.Observation,
                DateTime.UtcNow));

            return Ok(new { message = "Incidente resuelto." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{incidentId:long}/observation")]
    public async Task<IActionResult> UpdateObservation([FromRoute] long incidentId, [FromBody] UpdateEmergencyIncidentObservationResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new UpdateEmergencyIncidentObservationCommand(
                incidentId,
                userId,
                resource.Observation));

            return Ok(new { message = "Observación actualizada." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static object ToResponse(EmergencyIncident incident)
    {
        return new
        {
            incidentId = incident.EmergencyIncidentId,
            incidentKey = incident.IncidentKey,
            deviceId = incident.DeviceId,
            patientId = incident.PatientId,
            fallTypeId = incident.FallTypeId,
            fallType = incident.FallType is null
                ? null
                : new
                {
                    id = incident.FallType.FallTypeId,
                    name = incident.FallType.Name,
                    description = incident.FallType.Description,
                    severityLevel = incident.FallType.SeverityLevel
                },
            status = incident.Status.ToString(),
            openedAt = incident.OpenedAt,
            lastSignalAt = incident.LastSignalAt,
            cancelledAt = incident.CancelledAt,
            resolvedAt = incident.ResolvedAt,
            closedAt = incident.ClosedAt,
            closedByUserId = incident.ClosedByUserId,
            aiConfidenceScore = incident.AiConfidenceScore,
            latitude = incident.Latitude,
            longitude = incident.Longitude,
            cancellationReason = incident.CancellationReason?.ToString(),
            finalObservation = incident.FinalObservation
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
