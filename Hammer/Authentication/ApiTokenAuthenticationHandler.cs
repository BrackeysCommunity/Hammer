using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Hammer.Authentication;

/// <summary>
///     Represents an authentication handler which authenticates requests using a single, pre-shared API token
///     supplied via the <c>Authorization: Bearer &lt;token&gt;</c> header.
/// </summary>
internal sealed class ApiTokenAuthenticationHandler : AuthenticationHandler<ApiTokenAuthenticationSchemeOptions>
{
    private const string BearerPrefix = "Bearer ";
    private static readonly Encoding Utf8Encoding = Encoding.UTF8;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiTokenAuthenticationHandler" /> class.
    /// </summary>
    /// <param name="options">The options monitor.</param>
    /// <param name="logger">The logger factory.</param>
    /// <param name="encoder">The URL encoder.</param>
    public ApiTokenAuthenticationHandler(IOptionsMonitor<ApiTokenAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        var header = headerValues.ToString();
        if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization header must use the Bearer scheme."));
        }

        var token = header.AsSpan()[BearerPrefix.Length..].Trim();
        if (!TokensMatch(token, Options.Token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API token."));
        }

        var identity = new ClaimsIdentity(ApiTokenDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiTokenDefaults.AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool TokensMatch(ReadOnlySpan<char> presented, ReadOnlySpan<char> expected)
    {
        var presentedByteCount = Utf8Encoding.GetByteCount(presented);
        var expectedByteCount = Utf8Encoding.GetByteCount(expected);
        Span<byte> presentedBytes = stackalloc byte[presentedByteCount];
        Span<byte> expectedBytes = stackalloc byte[expectedByteCount];

        Utf8Encoding.GetBytes(presented, presentedBytes);
        Utf8Encoding.GetBytes(expected, expectedBytes);

        return CryptographicOperations.FixedTimeEquals(presentedBytes, expectedBytes);
    }
}
