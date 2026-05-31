using System.Text.Json;
using foll_backend.Care.Application.OutboundServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Queries;
using foll_backend.Care.Domain.Model.ValueObjects;
using foll_backend.Care.Domain.Services;
using foll_backend.Care.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.Care.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/care/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientCommandService _commandService;
    private readonly IPatientQueryService _queryService;
    private readonly IUserInfoService _userInfoService;

    public PatientsController(IPatientCommandService commandService, IPatientQueryService queryService, IUserInfoService userInfoService)
    {
        _commandService = commandService;
        _queryService = queryService;
        _userInfoService = userInfoService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var id = await _commandService.Handle(new CreatePatientCommand(
                userId,
                resource.Dni,
                resource.FirstName,
                resource.LastName,
                resource.BirthDate,
                resource.RelationshipTypeId,
                resource.BloodType,
                resource.MedicalConditions,
                resource.Medications));

            var created = await _queryService.Handle(new GetPatientByIdQuery(userId, id));
            if (created is null) return Ok(new { patientId = id });
            return Ok(await BuildPatientResponseAsync(created));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var patient = await _queryService.Handle(new GetPatientByIdQuery(userId, id));
            if (patient is null) return NotFound(new { message = "Paciente no encontrado." });
            return Ok(await BuildPatientResponseAsync(patient));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdatePatientResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new UpdatePatientCommand(
                userId,
                id,
                resource.FirstName,
                resource.LastName,
                resource.BirthDate,
                resource.BloodType,
                resource.MedicalConditions,
                resource.Medications));

            return Ok(new { message = "Paciente actualizado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}/guard-shift")]
    public async Task<IActionResult> AssignGuardShift([FromRoute] long id, [FromBody] AssignGuardShiftResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new AssignGuardShiftCommand(userId, id, resource.NewCurrentGuardianUserId));
            return Ok(new { message = "Turno asignado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/guard-shift/restore")]
    public async Task<IActionResult> RestoreGuardShift([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new RestoreGuardShiftCommand(userId, id));
            return Ok(new { message = "Turno restaurado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/emergency-contacts")]
    public async Task<IActionResult> AddEmergencyContact([FromRoute] long id, [FromBody] AddEmergencyContactResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var relationship = ParseRelationship(resource.Relationship);
            var contactId = await _commandService.Handle(new AddEmergencyContactCommand(
                userId,
                id,
                resource.FullName,
                resource.PhoneNumber,
                relationship));

            return Ok(new { emergencyContactId = contactId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}/emergency-contacts/{contactId:long}")]
    public async Task<IActionResult> RemoveEmergencyContact([FromRoute] long id, [FromRoute] long contactId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new RemoveEmergencyContactCommand(userId, id, contactId));
            return Ok(new { message = "Contacto eliminado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("{id:long}/annotations")]
    public async Task<IActionResult> AddAnnotation([FromRoute] long id, [FromBody] AddPatientAnnotationResource resource)
    {
        var userId = GetUserIdOrThrow();
        try
        {
            await _commandService.Handle(new AddPatientAnnotationCommand(userId, id, resource.Content));
            return Ok(new { message = "Anotación guardada exitosamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{id:long}/annotations")]
    public async Task<IActionResult> GetAnnotations([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var annotations = (await _queryService.Handle(new GetPatientAnnotationsQuery(userId, id))).ToList();
        
            // Mapeamos para enviar el nombre del autor y cumplir el contrato del front
            var result = new List<object>();
            foreach (var ann in annotations)
            {
                var user = await _userInfoService.FindByIdAsync(ann.AuthorUserId);
                result.Add(new
                {
                    id = ann.PatientAnnotationId.ToString(),
                    date = ann.CreatedAt.ToString("o"), // Formato ISO 8601
                    text = ann.Content,
                    author = user != null ? $"{user.FirstName} {user.LastName}" : "Usuario Desconocido"
                });
            }
        
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{dni}/invitations")]
    public async Task<IActionResult> CreateInvitation([FromRoute] string dni, [FromBody] CreateInvitationResource resource)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var expiresAt = DateTime.UtcNow.AddDays(2);
            var invitationId = await _commandService.Handle(new CreateInvitationCommand(
                userId,
                dni,
                resource.RelationshipTypeId,
                expiresAt));

            return Ok(new { invitationId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:long}/caregivers")]
    public async Task<IActionResult> GetCaregivers([FromRoute] long id)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            var caregivers = (await _queryService.Handle(new GetCaregiversByPatientIdQuery(userId, id))).ToList();

            return Ok(await BuildCaregiverResponsesAsync(caregivers, userId));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("by-caregiver/{caregiverUserId:long}")]
    public async Task<IActionResult> GetPatientsByCaregiverUserId([FromRoute] long caregiverUserId)
    {
        var actorUserId = GetUserIdOrThrow();
        if (actorUserId != caregiverUserId)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Solo puedes consultar tus propios pacientes." });

        try
        {
            var patients = await _queryService.Handle(new GetPatientsForUserQuery(caregiverUserId));

            var response = new List<object>();
            foreach (var patient in patients)
            {
                response.Add(new
                {
                    patient = await BuildPatientResponseAsync(patient),
                    caregiverKind = patient.OfficialGuardianUserId == caregiverUserId ? "official" : "caregiver",
                    relationshipTypeId = patient.Caregivers.FirstOrDefault(c => c.UserId == caregiverUserId)?.RelationshipTypeId
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT inválido: userId no encontrado.");
        return userId;
    }

    private async Task<object> BuildPatientResponseAsync(Patient patient)
    {
        return new
        {
            patient.PatientId,
            patient.Dni,
            patient.FirstName,
            patient.LastName,
            patient.BirthDate,
            patient.BloodType,
            patient.MedicalConditions,
            patient.Medications, //
            patient.CurrentGuardianUserId,
            patient.OfficialGuardianUserId,
            caregivers = await BuildCaregiverResponsesAsync(patient.Caregivers, patient.OfficialGuardianUserId),
            patient.EmergencyContacts,
            patient.Invitations
        };
    }

    private async Task<List<object>> BuildCaregiverResponsesAsync(IEnumerable<CaregiverRole> caregivers, long officialGuardianUserId)
    {
        var caregiverRoles = caregivers
            .Select(c => new
            {
                c.UserId,
                RelationshipTypeId = (short?)c.RelationshipTypeId
            })
            .ToList();

        if (officialGuardianUserId > 0 && caregiverRoles.All(c => c.UserId != officialGuardianUserId))
        {
            caregiverRoles.Add(new
            {
                UserId = officialGuardianUserId,
                RelationshipTypeId = (short?)null
            });
        }

        var result = new List<object>();
        foreach (var caregiverRole in caregiverRoles)
        {
            result.Add(new
            {
                userId = caregiverRole.UserId,
                user = await _userInfoService.FindByIdAsync(caregiverRole.UserId),
                relationshipTypeId = caregiverRole.RelationshipTypeId,
                caregiverKind = caregiverRole.UserId == officialGuardianUserId ? "official" : "caregiver"
            });
        }

        return result;
    }

    private static string ParseRelationship(object? relationship)
    {
        if (relationship is null)
            throw new InvalidOperationException("La relación es obligatoria.");

        if (relationship is string value)
            return value.Trim();

        if (relationship is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString()?.Trim() ?? string.Empty,
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.Object when element.TryGetProperty("name", out var name) => name.GetString()?.Trim() ?? string.Empty,
                JsonValueKind.Object when element.TryGetProperty("relationship", out var nestedRelationship) => ParseRelationship(nestedRelationship),
                JsonValueKind.Object when element.TryGetProperty("relationshipTypeId", out var relationshipTypeId) => relationshipTypeId.GetRawText(),
                _ => throw new InvalidOperationException("El campo relationship debe ser texto, número o un objeto con name.")
            };
        }

        return relationship.ToString()?.Trim() ?? string.Empty;
    }
}
