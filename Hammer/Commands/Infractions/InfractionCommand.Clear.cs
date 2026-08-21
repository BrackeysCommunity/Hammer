using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Extensions;
using Humanizer;
using JetBrains.Annotations;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("clear")]
    [Description("Clears all infractions from the specified user.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ClearAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user whose infractions to clear.")]
        DiscordUser user)
    {
        await context.DeferResponseAsync();

        DiscordGuild guild = context.Guild!;
        IReadOnlyList<Infraction> infractions = _infractionService.GetInfractions(user, guild);
        _infractionService.RemoveInfractions(infractions);

        int infractionCount = _infractionService.GetInfractionCount(user, guild);
        int differential = infractions.Count - infractionCount;

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Infractions cleared");
        embed.WithDescription($"Cleared {"infraction".ToQuantity(differential)} infractions " +
                              $"for {user.Mention}.");

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);

        if (differential > 0)
        {
            embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("Infractions Cleared");
            embed.AddField("User", user.Mention, true);
            embed.AddField("Count", differential, true);
            embed.AddField("Staff Member", context.Member!.Mention, true);
            await _logService.LogAsync(guild, embed);
        }
    }
}
