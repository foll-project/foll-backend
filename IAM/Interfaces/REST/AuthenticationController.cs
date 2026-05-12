using foll_backend.IAM.Domain.Services;
using foll_backend.IAM.Interfaces.REST.Resources;
using foll_backend.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

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
}
