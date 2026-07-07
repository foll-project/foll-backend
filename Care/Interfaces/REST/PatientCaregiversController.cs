using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Interfaces.REST.Resources;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.Care.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/patients/{patientId:long}/caregivers")]
public class PatientCaregiversController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientCaregiversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("qr")]
    public async Task<IActionResult> LinkCaregiverViaQr([FromRoute] long patientId, [FromBody] LinkCaregiverViaQrResource resource)
    {
        try
        {
            var command = new LinkCaregiverViaQrCommand(patientId, resource.CaregiverId);
            await _mediator.Send(command);
            
            return Ok(new { message = "Cuidador vinculado exitosamente al paciente a través de QR." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ocurrió un error interno al intentar vincular el cuidador." });
        }
    }

    [HttpDelete("{caregiverId:long}")]
    public async Task<IActionResult> RemoveCaregiver([FromRoute] long patientId, [FromRoute] long caregiverId)
    {
        try
        {
            var claim = User.FindFirst("userId")?.Value;
            if (!long.TryParse(claim, out var currentUserId) || currentUserId <= 0)
                return Unauthorized(new { message = "JWT inválido: userId no encontrado." });

            var command = new RemoveCaregiverCommand(currentUserId, patientId, caregiverId);
            await _mediator.Send(command);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ocurrió un error interno al intentar eliminar al cuidador." });
        }
    }
}
