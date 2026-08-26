using Asp.Versioning;
using FluentResults.Extensions.AspNetCore;
using Hammer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Controllers.v1;

/// <summary>
///     Represents the API controller for managing infractions.
/// </summary>
[ApiController, ApiVersion(1), Route("api/v{version:apiVersion}/infraction")]
public sealed class InfractionV1Controller : ControllerBase
{
    private readonly InfractionService _infractionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfractionV1Controller" /> class.
    /// </summary>
    /// <param name="infractionService">The infraction service.</param>
    public InfractionV1Controller(InfractionService infractionService)
    {
        _infractionService = infractionService;
    }

    /// <summary>
    ///     Retrieves an infraction by its ID.
    /// </summary>
    /// <param name="id">The ID of the infraction to retrieve.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> containing the infraction details if found; otherwise, an error response.
    /// </returns>
    [HttpGet("{id:long}")]
    public IActionResult GetInfraction(long id)
    {
        var result = _infractionService.GetInfraction(id);
        return result.ToActionResult();
    }
}
