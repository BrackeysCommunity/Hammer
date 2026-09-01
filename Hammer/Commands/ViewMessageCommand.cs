using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Extensions;
using Hammer.Resources;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands;

/// <summary>
///     Represents a module which implements the <c>viewmessage</c> command.
/// </summary>
internal sealed class ViewMessageCommand
{
    private readonly ConfigurationService _configurationService;
    private readonly MessageDeletionService _messageDeletionService;
    private readonly MessageService _messageService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViewMessageCommand" /> class.
    /// </summary>
    public ViewMessageCommand(
        ConfigurationService configurationService,
        MessageService messageService,
        MessageDeletionService messageDeletionService)
    {
        _configurationService = configurationService;
        _messageService = messageService;
        _messageDeletionService = messageDeletionService;
    }

    [Command("viewmessage")]
    [Description("Views a staff message, or deleted message, by its ID.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ViewMessageAsync(
        SlashCommandContext context,
        [Parameter("id")] [Description("The ID of the message to retrieve.")]
        string rawId
    )
    {
        await context.DeferResponseAsync();
        var embed = new DiscordEmbedBuilder();
        var guild = context.Guild!;

        if (!_configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            throw new InvalidOperationException(ExceptionMessages.NoConfigurationForGuild);
        }

        if (long.TryParse(rawId, out var staffMessageId) &&
            await _messageService.GetStaffMessage(staffMessageId) is { } staffMessage &&
            staffMessage.GuildId == guild.Id)
        {
            embed.WithColor(guildConfiguration.PrimaryColor);
            embed.WithTitle($"Message {staffMessage.Id}");
            embed.AddField("Recipient", MentionUtility.MentionUser(staffMessage.RecipientId), true);
            embed.AddField("Staff Member", MentionUtility.MentionUser(staffMessage.StaffMemberId), true);
            embed.AddField("Sent", Formatter.Timestamp(staffMessage.SentAt), true);
            embed.AddField("Content", Formatter.BlockCode(staffMessage.Content));
        }
        else if (ulong.TryParse(rawId, out var deletedMessageId) &&
                 await _messageDeletionService.GetDeletedMessage(deletedMessageId) is { } deletedMessage &&
                 deletedMessage.GuildId == guild.Id)
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle($"Deleted Message {deletedMessage.MessageId}");
            embed.AddField("Author", MentionUtility.MentionUser(deletedMessage.AuthorId), true);
            embed.AddField("Channel", MentionUtility.MentionChannel(deletedMessage.ChannelId), true);
            embed.AddField("Created", Formatter.Timestamp(deletedMessage.CreationTimestamp), true);
            embed.AddField("Deleted", Formatter.Timestamp(deletedMessage.DeletionTimestamp), true);
            embed.AddField("Staff Member", MentionUtility.MentionUser(deletedMessage.StaffMemberId), true);

            var hasContent = !string.IsNullOrWhiteSpace(deletedMessage.Content);
            var hasAttachments = deletedMessage.Attachments.Count > 0;

            var content = hasContent ? Formatter.Sanitize(deletedMessage.Content!) : null;
            var attachments =
                hasAttachments ? string.Join('\n', deletedMessage.Attachments.Select(a => a.AbsoluteUri)) : null;

            embed.AddFieldIf(hasContent, "Content",
                () => Formatter.BlockCode(content!.Length >= 1014 ? content[..1011] + "..." : content));
            embed.AddFieldIf(hasAttachments, "Attachments", attachments);
        }
        else
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("No such message");
            embed.WithDescription($"Could not find a message with the ID {rawId}");
        }

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
