using Microsoft.AspNetCore.Authentication;

namespace Hammer.Authentication;

/// <summary>
///     Represents the options for <see cref="ApiTokenAuthenticationHandler" />.
/// </summary>
internal sealed class ApiTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    ///     Gets or sets the expected API token.
    /// </summary>
    /// <value>The expected API token.</value>
    public string Token { get; set; } = string.Empty;
}
