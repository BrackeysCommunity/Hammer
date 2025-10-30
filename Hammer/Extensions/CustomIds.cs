namespace Hammer.Extensions;

/// <summary>
///     Contains custom IDs used throughout the application.
/// </summary>
internal static class CustomIds
{
    public static readonly string BadMessageWarning = new CustomIdBuilder().Type("warning:bad_message").ToString();
    public static readonly string MessageMember = new CustomIdBuilder().Type("staff:message").ToString();
    public static readonly string AddRule = new CustomIdBuilder().Type("rule:add").ToString();
    public static readonly string EditRule = new CustomIdBuilder().Type("rule:edit").ToString();
}
