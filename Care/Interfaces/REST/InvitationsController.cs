using foll_backend.Care.Application.OutboundServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Queries;
using foll_backend.Care.Domain.Model.ValueObjects;
using foll_backend.Care.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.Care.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/care/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly IPatientCommandService _commandService;
    private readonly IPatientQueryService _queryService;
    private readonly IUserInfoService _userInfoService;

    public InvitationsController(
        IPatientCommandService commandService,
        IPatientQueryService queryService,
        IUserInfoService userInfoService)
    {
        _commandService = commandService;
        _queryService = queryService;
        _userInfoService = userInfoService;
    }

    // Invitaciones que el usuario actual debe aprobar/rechazar (es cuidador principal).
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived()
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var invitations = await _queryService.Handle(new GetReceivedInvitationsQuery(userId));
            return Ok(await BuildInvitationResponsesAsync(invitations));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Invitaciones que el usuario actual envió a otros pacientes.
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var invitations = await _queryService.Handle(new GetSentInvitationsQuery(userId));
            return Ok(await BuildInvitationResponsesAsync(invitations));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{invitationId:long}/accept")]
    public async Task<IActionResult> Accept([FromRoute] long invitationId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new AcceptInvitationCommand(userId, invitationId));
            return Ok(new { message = "Invitación aceptada." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{invitationId:long}/reject")]
    public async Task<IActionResult> Reject([FromRoute] long invitationId)
    {
        var userId = GetUserIdOrThrow();

        try
        {
            await _commandService.Handle(new RejectInvitationCommand(userId, invitationId));
            return Ok(new { message = "Invitación rechazada." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<List<object>> BuildInvitationResponsesAsync(IEnumerable<InvitationView> invitations)
    {
        var result = new List<object>();
        foreach (var invitation in invitations)
        {
            var requester = await _userInfoService.FindByIdAsync(invitation.InvitingUserId);
            result.Add(new
            {
                invitationId = invitation.InvitationId,
                patientId = invitation.PatientId,
                patientFirstName = invitation.PatientFirstName,
                patientLastName = invitation.PatientLastName,
                patientName = $"{invitation.PatientFirstName} {invitation.PatientLastName}".Trim(),
                patientDni = invitation.PatientDni,
                requesterUserId = invitation.InvitingUserId,
                requesterName = requester != null
                    ? $"{requester.FirstName} {requester.LastName}".Trim()
                    : "Usuario desconocido",
                requesterEmail = requester?.Email,
                relationshipTypeId = invitation.RelationshipTypeId,
                relationshipName = invitation.RelationshipName,
                status = invitation.Status,
                expiresAt = invitation.ExpiresAt
            });
        }

        return result;
    }

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT inválido: userId no encontrado.");
        return userId;
    }
}
