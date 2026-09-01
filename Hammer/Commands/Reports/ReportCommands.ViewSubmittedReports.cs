using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.Extensions;
using Humanizer;
using JetBrains.Annotations;

namespace Hammer.Commands.Reports;

internal sealed partial class ReportCommands
{
    [Command("viewsubmittedreports")]
    [Description("Views all reports submitted by a user.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ViewSubmittedReportsAsync(
        SlashCommandContext context,
        [Parameter("user")] [Description("The user whose submitted reports to view.")]
        DiscordUser user
    )
    {
        await context.DeferResponseAsync();

        var list = new List<string>();

        foreach (var reportedMessage in _reportService.EnumerateSubmittedReports(user, context.Guild!))
        {
            var id = reportedMessage.MessageId.ToString();

            try
            {
                var channel = await context.Client.GetChannelAsync(reportedMessage.ChannelId);
                var message = await channel.GetMessageAsync(reportedMessage.MessageId);
                id = Formatter.MaskedUrl(id, message.JumpLink);
            }
            catch (DiscordException)
            {
            }

            var channelMention = MentionUtility.MentionChannel(reportedMessage.ChannelId);
            var userMention = MentionUtility.MentionUser(reportedMessage.AuthorId);
            list.Add($"**ID {reportedMessage.Id}** \u2022 {id} in {channelMention} against {userMention}");
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user);

        if (list.Count == 0)
        {
            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("No reports");
            embed.WithDescription("No reports have been submitted by this user.");
        }
        else
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle($"{"report".ToQuantity(list.Count)}");
            embed.WithDescription(string.Join('\n', list));
        }

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
