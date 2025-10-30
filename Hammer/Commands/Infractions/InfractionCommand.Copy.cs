using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Extensions;
using JetBrains.Annotations;

namespace Hammer.Commands.Infractions;

internal sealed partial class InfractionCommand
{
    [Command("copy")]
    [Description("Copies all infractions from one user to another.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task CopyAsync(SlashCommandContext context,
        [Parameter("source"), Description("The user whose infractions to copy.")]
        DiscordUser source,
        [Parameter("destination"), Description("The user who will acquire the copied infractions.")]
        DiscordUser destination)
    {
        if (source == destination)
        {
            await context.RespondAsync("You can't copy infractions to the same user.", true);
            return;
        }

        await context.DeferResponseAsync();

        IEnumerable<Infraction> infractions = _infractionService.EnumerateInfractions(source, context.Guild);
        List<Infraction> copies = infractions.Select(infraction => new Infraction(infraction) { UserId = destination.Id })
            .ToList();

        _infractionService.AddInfractions(copies);

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(destination);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Infractions Copied");
        embed.WithDescription($"{copies.Count} infractions for {source.Mention} have been copied to {destination.Mention}.");

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);

        embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle("Infractions Copied");
        embed.AddField("From", source.Mention, true);
        embed.AddField("To", destination.Mention, true);
        embed.AddField("Count", copies.Count, true);
        embed.AddField("Staff Member", context.Member.Mention, true);
        await _logService.LogAsync(context.Guild, embed);
    }
}
