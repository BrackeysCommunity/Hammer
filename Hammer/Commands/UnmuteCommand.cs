using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hammer.Extensions;
using Hammer.Services;
using Microsoft.Extensions.Logging;
using X10D.Text;

namespace Hammer.Commands;

/// <summary>
///     Represents a module which implements the <c>unmute</c> command.
/// </summary>
internal sealed class UnmuteCommand
{
    private readonly ILogger<UnmuteCommand> _logger;
    private readonly MuteService _muteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnmuteCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="muteService">The mute service.</param>
    public UnmuteCommand(ILogger<UnmuteCommand> logger, MuteService muteService)
    {
        _logger = logger;
        _muteService = muteService;
    }

    [Command("unmute")]
    [Description("Unmutes a user.")]
    [RequireGuild]
    public async Task UnmuteAsync(CommandContext context,
        [Parameter("user"), Description("The user to unmute.")]
        DiscordUser user,
        [Parameter("reason"), Description("The reason for the mute revocation.")]
        string? reason = null)
    {
        await context.DeferAsync(true);

        var embed = new DiscordEmbedBuilder();
        try
        {
            await _muteService.RevokeMuteAsync(user, context.Member!, reason);

            embed.WithAuthor(user);
            embed.WithColor(DiscordColor.SpringGreen);
            embed.WithTitle("Unmuted user");
            embed.WithDescription(reason);

            reason = reason.WithWhiteSpaceAlternative("None");
            _logger.LogInformation("{StaffMember} unmuted {User}. Reason: {Reason}", context.Member, user, reason);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not revoke mute");

            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("⚠️ Error revoking mute");
            embed.WithDescription($"{exception.GetType().Name} was thrown while revoking the mute.");
            embed.WithFooter("See log for further details.");
        }

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
