using Hammer.Data;

namespace Hammer.Services;

/// <summary>
///     Represents a service which manages member warnings.
/// </summary>
public interface IWarningService
{
    /// <summary>
    ///     Warns a user with the specified reason.
    /// </summary>
    /// <param name="options">The warning options.</param>
    /// <returns>
    ///     A tuple containing the created infraction, and a boolean indicating whether the user was successfully DMd.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="options" /> is <see langword="null" />, empty, or consists of only whitespace.
    /// </exception>
    ValueTask<InfractionResult> WarnAsync(WarningOptions options);
}
