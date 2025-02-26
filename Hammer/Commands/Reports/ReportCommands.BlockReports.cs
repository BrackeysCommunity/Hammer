using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hammer.Extensions;

namespace Hammer.Commands.Reports;

internal sealed partial class ReportCommands
{
    [Command("blockreports")]
    [Description("Blocks a user from reporting messages.")]
    [RequireGuild]
    public async Task BlockReportsAsync(CommandContext context, [Option("user", "The user to block.")] DiscordUser user)
    {
        await context.DeferAsync(true);
        DiscordGuild guild = context.Guild;

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user);

        if (_reportService.IsUserBlocked(user, guild))
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("User Already Blocked");
            embed.WithDescription($"{user.Mention} is already blocked from reporting messages.");
        }
        else
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("User Blocked");
            embed.WithDescription($"{user.Mention} will no longer be able to make message reports.");
            await _reportService.BlockUserAsync(user, context.Member);
        }

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
