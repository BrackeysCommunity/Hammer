using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;
using JetBrains.Annotations;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("stats")]
    [Description("View infraction stats.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task StatsAsync(SlashCommandContext context)
    {
        DiscordGuild guild = context.Guild!;
        IReadOnlyList<Infraction> infractions = _infractionService.GetInfractions(guild);

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
        DiscordEmbed result = await _infractionStatisticsService.CreateStatisticsEmbedAsync(guild);

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(result);
        await context.EditResponseAsync(builder);
    }
}
