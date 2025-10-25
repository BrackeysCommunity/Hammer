using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Extensions;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("move")]
    [Description("Moves all infractions from one user to another.")]
    [RequireGuild]
    public async Task MoveAsync(SlashCommandContext context,
        [Parameter("source"), Description("The user whose infractions to move.")]
        DiscordUser source,
        [Parameter("destination"), Description("The user who will acquire the moved infractions.")]
        DiscordUser destination)
    {
        if (source == destination)
        {
            await context.RespondAsync("You can't move infractions to the same user.", true);
            return;
        }

        await context.DeferResponseAsync();

        IEnumerable<Infraction> infractions = _infractionService.EnumerateInfractions(source, context.Guild);
        var count = 0;
        foreach (Infraction infraction in infractions)
        {
            _infractionService.ModifyInfraction(infraction, i => i.UserId = destination.Id);
            count++;
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(destination);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Infractions Moved");
        embed.WithDescription($"{count} infractions for {source.Mention} have been moved to {destination.Mention}.");

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);

        embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle("Infractions Moved");
        embed.AddField("From", source.Mention, true);
        embed.AddField("To", destination.Mention, true);
        embed.AddField("Count", count, true);
        embed.AddField("Staff Member", context.Member.Mention, true);
        await _logService.LogAsync(context.Guild, embed);
    }
}
