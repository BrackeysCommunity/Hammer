using DSharpPlus.Entities;

namespace Hammer.Data;

/// <summary>
///     Represents the options for issuing a warning to a user.
/// </summary>
public sealed class WarningOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WarningOptions" /> structure.
    /// </summary>
    /// <param name="user">The user to be warned.</param>
    /// <param name="issuer">The staff member who issued the warning.</param>
    /// <param name="reason">The reason for the warning.</param>
    /// <param name="ruleBroken">The rule broken, or <see langword="null" /> if no rule is specified.</param>
    /// <param name="additionalInfo">Additional information about the warning.</param>
    public WarningOptions(DiscordUser user,
        DiscordMember issuer,
        string reason,
        Rule? ruleBroken = null,
        string? additionalInfo = null)
    {
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        User = user ?? throw new ArgumentNullException(nameof(user));
        RuleBroken = ruleBroken;
        AdditionalInfo = additionalInfo;
    }

    /// <summary>
    ///     Gets additional information about the warning.
    /// </summary>
    /// <value>Additional information about the warning, or <see langword="null" /> if none is specified.</value>
    public string? AdditionalInfo { get; } = null;

    /// <summary>
    ///     Gets the staff member who issued the warning.
    /// </summary>
    /// <value>The staff member who issued the warning.</value>
    public DiscordMember Issuer { get; }

    /// <summary>
    ///     Gets the reason for the warning.
    /// </summary>
    /// <value>The reason for the warning.</value>
    public string Reason { get; }

    /// <summary>
    ///     Gets the rule broken, if any.
    /// </summary>
    /// <value>The rule broken, or <see langword="null" /> if no rule is specified.</value>
    public Rule? RuleBroken { get; }

    /// <summary>
    ///     Gets the user to be warned.
    /// </summary>
    /// <value>The user to be warned.</value>
    public DiscordUser User { get; }
}
