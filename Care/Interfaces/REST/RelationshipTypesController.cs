using foll_backend.Care.Domain.Model.Queries;
using foll_backend.Care.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace foll_backend.Care.Interfaces.REST;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/care/relationship-types")]
public class RelationshipTypesController : ControllerBase
{
    private readonly IRelationshipTypeQueryService _service;

    public RelationshipTypesController(IRelationshipTypeQueryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _service.Handle(new ListRelationshipTypesQuery());
        return Ok(items);
    }
}
