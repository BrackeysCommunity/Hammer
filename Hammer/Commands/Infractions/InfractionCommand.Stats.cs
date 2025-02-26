using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("stats")]
    [Description("View infraction stats.")]
    [RequireGuild]
    public async Task StatsAsync(CommandContext context)
    {
        IReadOnlyList<Infraction> infractions = _infractionService.GetInfractions(context.Guild);

        if (infractions.Count == 0)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("No infractions on record");
            embed.WithDescription("Statistics cannot be generated because there are no infractions on record.");

            await context.CreateResponseAsync(embed, true);
            return;
        }

        if (!_configurationService.TryGetGuildConfiguration(context.Guild, out GuildConfiguration? guildConfiguration))
        {
            await context.CreateResponseAsync("Guild is not configured!", true);
            return;
        }

        await context.DeferResponseAsync();
        DiscordEmbed result = await _infractionStatisticsService.CreateStatisticsEmbedAsync(context.Guild);

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(result);
        await context.EditResponseAsync(builder);
    }
}
