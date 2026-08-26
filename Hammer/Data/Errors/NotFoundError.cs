using Microsoft.AspNetCore.Http;

namespace Hammer.Data.Errors;

/// <summary>
///     Represents an <see cref="HttpError" /> which indicates that a requested resource could not be found.
/// </summary>
internal sealed class NotFoundError : HttpError
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotFoundError" /> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundError(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}
