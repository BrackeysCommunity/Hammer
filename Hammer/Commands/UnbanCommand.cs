using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Extensions;
using Hammer.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using X10D.Text;

namespace Hammer.Commands;

/// <summary>
///     Represents a module which implements the <c>unban</c> command.
/// </summary>
internal sealed class UnbanCommand
{
    private readonly ILogger<UnbanCommand> _logger;
    private readonly BanService _banService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnbanCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="banService">The ban service.</param>
    public UnbanCommand(ILogger<UnbanCommand> logger, BanService banService)
    {
        _logger = logger;
        _banService = banService;
    }

    [Command("unban")]
    [Description("Unbans a user.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task UnbanAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user to unban.")] DiscordUser user,
        [Parameter("reason"), Description("The reason for the ban revocation.")] string? reason = null)
    {
        await context.DeferResponseAsync(true);

        var embed = new DiscordEmbedBuilder();
        try
        {
            await _banService.RevokeBanAsync(user, context.Member!, reason);

            embed.WithAuthor(user);
            embed.WithColor(DiscordColor.SpringGreen);
            embed.WithTitle("Unbanned user");
            embed.WithDescription(reason);

            reason = reason.WithWhiteSpaceAlternative("None");
            _logger.LogInformation("{StaffMember} unbanned {User}. Reason: {Reason}", context.Member, user, reason);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not revoke ban");

            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("⚠️ Error revoking ban");
            embed.WithDescription($"{exception.GetType().Name} was thrown while revoking the ban.");
            embed.WithFooter("See log for further details.");
        }

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
