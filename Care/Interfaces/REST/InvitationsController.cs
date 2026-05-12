using foll_backend.Care.Domain.Model.Commands;
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

    public InvitationsController(IPatientCommandService commandService)
    {
        _commandService = commandService;
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

    private long GetUserIdOrThrow()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("JWT inválido: userId no encontrado.");
        return userId;
    }
}
