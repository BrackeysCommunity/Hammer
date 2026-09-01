using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using JetBrains.Annotations;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("stats")]
    [Description("View infraction stats.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task StatsAsync(SlashCommandContext context,
        [Parameter("staffMember")] [Description("The staff member whose infractions to view.")]
        DiscordMember? staffMember = null)
    {
        var guild = context.Guild!;
        var infractions = _infractionService.GetInfractions(guild);

        if (infractions.Count == 0)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("No infractions on record");
            embed.WithDescription("Statistics cannot be generated because there are no infractions on record.");

            await context.RespondAsync(embed, true);
            return;
        }

        if (!_configurationService.TryGetGuildConfiguration(guild, out _))
        {
            await context.RespondAsync("Guild is not configured!", true);
            return;
        }

        await context.DeferResponseAsync();
        var result = await _infractionStatisticsService.CreateStatisticsEmbedAsync(guild);

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(result);
        await context.EditResponseAsync(builder);
    }
}
