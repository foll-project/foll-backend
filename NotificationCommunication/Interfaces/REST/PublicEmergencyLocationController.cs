using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.NotificationCommunication.Application.Internal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.NotificationCommunication.Interfaces.REST;

[ApiController]
[AllowAnonymous]
[Route("api/emergency/public-location")]
public class PublicEmergencyLocationController : ControllerBase
{
    private readonly IEmergencyLocationLinkService _locationLinkService;
    private readonly IEmergencyIncidentRepository _incidentRepository;

    public PublicEmergencyLocationController(
        IEmergencyLocationLinkService locationLinkService,
        IEmergencyIncidentRepository incidentRepository)
    {
        _locationLinkService = locationLinkService;
        _incidentRepository = incidentRepository;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetByToken([FromRoute] string token)
    {
        var resolution = await _locationLinkService.ResolveAsync(token, DateTime.UtcNow);
        if (resolution is null)
            return NotFound(new { message = "El enlace no existe o ya expiro." });

        var link = resolution.Link;
        var incident = await _incidentRepository.FindByIncidentKeyWithFallTypeAsync(link.IncidentKey);
        if (incident is null || incident.PatientId != link.PatientId)
            return NotFound(new { message = "Incidente no encontrado." });

        return Ok(new
        {
            incidentKey = incident.IncidentKey,
            patientId = incident.PatientId,
            deviceId = incident.DeviceId,
            status = incident.Status.ToString(),
            openedAt = incident.OpenedAt,
            fallType = incident.FallType is null
                ? null
                : new
                {
                    name = incident.FallType.Name,
                    severityLevel = incident.FallType.SeverityLevel
                },
            location = new
            {
                latitude = incident.Latitude,
                longitude = incident.Longitude
            },
            expiresAt = link.ExpiresAt
        });
    }
}
