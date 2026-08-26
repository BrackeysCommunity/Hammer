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
    [Command("delete")]
    [Description("Deletes an infraction.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DeleteAsync(SlashCommandContext context,
        [Parameter("infraction"), Description("The infraction to delete.")]
        long infractionId
    )
    {
        await context.DeferResponseAsync();
        var embed = new DiscordEmbedBuilder();

        var result = _infractionService.GetInfraction(infractionId);
        if (result.IsFailed)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("Infraction not found");
            embed.WithDescription($"The infraction with the ID `{infractionId}` was not found.");
        }
        else
        {
            var infraction = result.Value;
            embed.WithColor(0x00FF00);
            embed.WithTitle("Infraction Redacted");
            embed.WithDescription($"{infraction.Type} #{infraction.Id} for {MentionUtility.MentionUser(infraction.UserId)} " +
                                  "has been redacted.");
            _infractionService.RemoveInfraction(infraction);
        }

        var builder = new DiscordWebhookBuilder();
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);

        if (result.IsSuccess)
        {
            var infraction = result.Value;
            embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle("Infraction Deleted");
            embed.AddField("ID", infraction.Id, true);
            embed.AddField("User", MentionUtility.MentionUser(infraction.UserId), true);
            embed.AddField("Type", infraction.Type, true);
            embed.AddField("Staff Member", context.Member!.Mention, true);
            await _logService.LogAsync(context.Guild!, embed);
        }
    }
}
