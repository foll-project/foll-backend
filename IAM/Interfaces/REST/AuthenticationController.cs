using foll_backend.IAM.Domain.Services;
using foll_backend.IAM.Interfaces.REST.Resources;
using foll_backend.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace foll_backend.IAM.Interfaces.REST;

[ApiController]
[Route("api/iam/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly IUserQueryService _userQueryService;

    public AuthenticationController(IUserCommandService userCommandService, IUserQueryService userQueryService)
    {
        _userCommandService = userCommandService;
        _userQueryService = userQueryService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterResource resource)
    {
        var command = RegisterCommandFromResourceAssembler.ToCommandFromResource(resource);

        try
        {
            await _userCommandService.Handle(command);
            return Ok(new { message = "Usuario registrado exitosamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = LoginQueryFromRequestAssembler.ToQuery(request);

        var auth = await _userQueryService.AuthenticateAsync(query);

        if (auth == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var (user, token) = auth.Value;

        var response = LoginQueryFromRequestAssembler.ToResponse(user, token);

        return Ok(response);
    }

    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountResource resource)
    {
        var claim = User.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
            return Unauthorized(new { message = "JWT inválido: userId no encontrado." });

        var command = new foll_backend.IAM.Domain.Model.Commands.DeleteaccountCommand(userId, resource.Password);

        try
        {
            await _userCommandService.Handle(command);
            return Ok(new { message = "Cuenta eliminada exitosamente. Adiós." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error interno al intentar eliminar la cuenta." });
        }
    }
}
