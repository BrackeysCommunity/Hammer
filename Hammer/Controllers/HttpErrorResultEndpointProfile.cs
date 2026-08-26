using FluentResults.Extensions.AspNetCore;
using Hammer.Data.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Controllers;

/// <summary>
///     Represents an <see cref="IAspNetCoreResultEndpointProfile" /> which translates a failed
///     <see cref="FluentResults.Result" /> into an HTTP response using the status code carried by its
///     <see cref="HttpError" /> reasons, falling back to <see cref="StatusCodes.Status400BadRequest" /> for any
///     failure that does not specify one.
/// </summary>
internal sealed class HttpErrorResultEndpointProfile : DefaultAspNetCoreResultEndpointProfile
{
    /// <inheritdoc />
    public override ActionResult TransformFailedResultToActionResult(FailedResultToActionResultTransformationContext context)
    {
        int statusCode = context.Result.Errors
            .OfType<HttpError>()
            .Select(e => e.StatusCode)
            .DefaultIfEmpty(StatusCodes.Status400BadRequest)
            .Max();

        return new ObjectResult(new { Errors = context.Result.Errors.Select(e => e.Message) })
        {
            StatusCode = statusCode
        };
    }
}
