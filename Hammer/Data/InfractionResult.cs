namespace Hammer.Data;

/// <summary>
///     Represents the result of an infraction issue.
/// </summary>
public struct InfractionResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InfractionResult" /> structure.
    /// </summary>
    /// <param name="directMessageSuccess">
    ///     A value indicating whether the direct message to the target user was successful.
    /// </param>
    /// <param name="infraction">The infraction representing the warning.</param>
    public InfractionResult(Infraction infraction, bool directMessageSuccess)
    {
        DirectMessageSuccess = directMessageSuccess;
        Infraction = infraction;
    }

    /// <summary>
    ///     Gets a value indicating whether the direct message to the warned user was successful.
    /// </summary>
    /// <value><see langword="true" /> if the direct message was successful; otherwise, <see langword="false" />.</value>
    public bool DirectMessageSuccess { get; init; }

    /// <summary>
    ///     Gets the infraction.
    /// </summary>
    /// <value>The infraction.</value>
    public Infraction Infraction { get; init; }
}
