using FluentResults;

namespace Hammer.Data.Errors;

/// <summary>
///     Represents a <see cref="FluentResults" /> error which carries the HTTP status code it should be translated to
///     when returned from an API endpoint.
/// </summary>
internal class HttpError : Error
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpError" /> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code this error should be translated to.</param>
    public HttpError(string message, int statusCode) : base(message)
    {
        Metadata["StatusCode"] = statusCode;
    }

    /// <summary>
    ///     Gets the HTTP status code this error should be translated to.
    /// </summary>
    /// <value>The HTTP status code.</value>
    public int StatusCode => (int)Metadata["StatusCode"];
}
