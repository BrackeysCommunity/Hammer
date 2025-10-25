using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("stats")]
    [Description("View infraction stats.")]
    [RequireGuild]
    public async Task StatsAsync(SlashCommandContext context)
    {
        IReadOnlyList<Infraction> infractions = _infractionService.GetInfractions(context.Guild);

        if (infractions.Count == 0)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("No infractions on record");
            embed.WithDescription("Statistics cannot be generated because there are no infractions on record.");

            await context.RespondAsync(embed, true);
            return;
        }

        if (!_configurationService.TryGetGuildConfiguration(context.Guild, out _))
        {
            await context.RespondAsync("Guild is not configured!", true);
            return;
        }

        await context.DeferResponseAsync();
        DiscordEmbed result = await _infractionStatisticsService.CreateStatisticsEmbedAsync(context.Guild);

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(result);
        await context.EditResponseAsync(builder);
    }
}
