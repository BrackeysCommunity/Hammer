using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.Data;
using Hammer.Extensions;

namespace Hammer.Commands.Reports;

internal sealed partial class ReportCommands
{
    [Command("viewreport")]
    [Description("Views all reports made against this user.")]
    [RequireGuild]
    public async Task ViewReportAsync(CommandContext context,
        [Parameter("id"), Description("The ID of the report to view.")] long id)
    {
        await context.DeferResponseAsync();

        var embed = new DiscordEmbedBuilder();

        if (_reportService.TryGetReport(id, out ReportedMessage? reportedMessage))
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle($"Report {id}");
            embed.AddField("Channel", MentionUtility.MentionChannel(reportedMessage.ChannelId), true);
            embed.AddField("Author", MentionUtility.MentionUser(reportedMessage.AuthorId), true);
            embed.AddField("Reporter", MentionUtility.MentionUser(reportedMessage.ReporterId), true);

            try
            {
                DiscordChannel channel = await context.Client.GetChannelAsync(reportedMessage.ChannelId);
                DiscordMessage message = await channel.GetMessageAsync(reportedMessage.MessageId);
                embed.AddField("Message ID", Formatter.MaskedUrl(message.Id.ToString(), message.JumpLink), true);
                embed.AddField("Message Time", Formatter.Timestamp(message.CreationTimestamp, TimestampFormat.LongDateTime),
                    true);
            }
            catch (DiscordException)
            {
                embed.AddField("Message ID", reportedMessage.MessageId, true);
                embed.AddField("Message Time", "*Deleted*", true);
            }

            if (!string.IsNullOrWhiteSpace(reportedMessage.Content))
                embed.AddField("Content", Formatter.BlockCode(Formatter.Sanitize(reportedMessage.Content)));

            if (reportedMessage.Attachments.Count > 0)
                embed.AddField("Attachments", string.Join('\n', reportedMessage.Attachments));

            DiscordUser user = await context.Client.GetUserAsync(reportedMessage.AuthorId);
            embed.WithAuthor(user);
        }
        else
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("No such report");
            embed.WithDescription($"No report with the ID {id} could be found.");
        }

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
