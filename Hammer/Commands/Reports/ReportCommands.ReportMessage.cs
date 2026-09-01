using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Resources;
using JetBrains.Annotations;
using SmartFormat;

namespace Hammer.Commands.Reports;

internal sealed partial class ReportCommands
{
    [Command("Report Message")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ReportMessageAsync(SlashCommandContext context, DiscordMessage message)
    {
        await context.DeferResponseAsync(true);

        var user = context.User;
        await _reportService.ReportMessageAsync(message, (DiscordMember)user);

        var builder = new DiscordWebhookBuilder();
        var embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Message Reported");
        embed.WithDescription(EmbedMessages.MessageReportFeedback.FormatSmart(new { user }));
        embed.WithFooter("Reporting this message again will have no impact.");
        builder.AddEmbed(embed);

        await context.EditResponseAsync(builder);
    }
}
