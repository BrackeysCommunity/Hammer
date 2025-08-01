using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Hammer.Extensions;

namespace Hammer.Commands.Reports;

internal sealed partial class ReportCommands
{
    [Command("unblockreports")]
    [Description("Unblocks a user, allowing them to report messages.")]
    [RequireGuild]
    public async Task UnblockReportsAsync(CommandContext context,
        [Parameter("user"), Description("The user to unblock.")] DiscordUser user)
    {
        await context.DeferResponseAsync(true);

        DiscordGuild guild = context.Guild;

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user);

        if (_reportService.IsUserBlocked(user, guild))
        {
            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("User Unblocked");
            embed.WithDescription($"{user.Mention} has been unblocked. Their message reports will now be acknowledged.");
            await _reportService.UnblockUserAsync(user, context.Member);
        }
        else
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("User Not Blocked");
            embed.WithDescription($"{user.Mention} was not previously blocked from reporting messages.");
        }

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
