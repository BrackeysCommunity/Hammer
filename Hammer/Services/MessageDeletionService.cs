﻿using DSharpPlus;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using SmartFormat;
using X10D.DSharpPlus;
using ILogger = NLog.ILogger;

namespace Hammer.Services;

/// <summary>
///     Represents a service which handles message deletions from staff.
/// </summary>
internal sealed class MessageDeletionService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConfigurationService _configurationService;
    private readonly DiscordLogService _logService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageDeletionService" /> class.
    /// </summary>
    public MessageDeletionService(
        IServiceScopeFactory scopeFactory,
        ConfigurationService configurationService,
        DiscordLogService logService
    )
    {
        _scopeFactory = scopeFactory;
        _configurationService = configurationService;
        _logService = logService;
    }

    /// <summary>
    ///     Deletes a specified message, logging the deletion in the staff log and optionally notifying the author.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="staffMember">The staff member responsible for the deletion.</param>
    /// <param name="notifyAuthor">
    ///     <see langword="true" /> to notify the author of the deletion; otherwise, <see langword="false" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="message" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="staffMember" /> is <see langword="null" />.</para>
    /// </exception>
    /// <exception cref="NotSupportedException">The message does not belong to a guild.</exception>
    /// <exception cref="ArgumentException">
    ///     The guild in which the message appears does not match the guild of <paramref name="staffMember" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <para><paramref name="staffMember" /> is not a staff member.</para>
    ///     -or-
    ///     <para><paramref name="staffMember" /> is a lower level than the author of <paramref name="message" />.</para>
    /// </exception>
    public async Task DeleteMessageAsync(DiscordMessage message, DiscordMember staffMember, bool notifyAuthor = true)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (staffMember is null) throw new ArgumentNullException(nameof(staffMember));

        Logger.Info($"{message} in channel {message.Channel} is requested to be deleted by {staffMember}");

        message = await message.Channel.GetMessageAsync(message.Id).ConfigureAwait(false);
        DiscordGuild guild = message.Channel.Guild;

        if (guild is null)
            throw new InvalidOperationException(ExceptionMessages.CannotDeleteNonGuildMessage);

        if (guild != staffMember.Guild)
            throw new ArgumentException(ExceptionMessages.MessageStaffMemberGuildMismatch);

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
            throw new InvalidOperationException(ExceptionMessages.NoConfigurationForGuild);

        DiscordUser user = message.Author;
        DiscordMember? member = await user.GetAsMemberOfAsync(guild).ConfigureAwait(false);

        if (!staffMember.IsStaffMember(guildConfiguration))
        {
            string exceptionMessage = ExceptionMessages.NotAStaffMember.FormatSmart(new {user = staffMember, guild});
            throw new InvalidOperationException(exceptionMessage);
        }

        if (member is not null)
        {
            if (member.IsHigherLevelThan(staffMember, guildConfiguration))
            {
                var formatObject = new {lower = staffMember, higher = member};
                string exceptionMessage = ExceptionMessages.StaffIsHigherLevel.FormatSmart(formatObject);
                throw new InvalidOperationException(exceptionMessage);
            }

            if (notifyAuthor)
            {
                try
                {
                    DiscordEmbed toAuthorEmbed = CreateMessageDeletionToAuthorEmbed(message, guildConfiguration);
                    await member.SendMessageAsync(toAuthorEmbed).ConfigureAwait(false);
                }
                catch
                {
                    Logger.Warn($"{member} could not be notified of the deletion");
                    // ignored
                }
            }
        }

        DiscordEmbed staffLogEmbed = CreateMessageDeletionToStaffLogEmbed(message, staffMember, guildConfiguration);

        var deletedMessage = DeletedMessage.Create(message, staffMember);
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        await context.AddAsync(deletedMessage).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        Logger.Info($"{message} in channel {message.Channel} was deleted by {staffMember}");
        await message.DeleteAsync($"Deleted by {staffMember.GetUsernameWithDiscriminator()}").ConfigureAwait(false);
        await _logService.LogAsync(guild, staffLogEmbed).ConfigureAwait(false);
    }

    /// <summary>
    ///     Returns a deleted message by its ID. 
    /// </summary>
    /// <param name="id">The ID of the message to retrieve.</param>
    /// <returns>A <see cref="DeletedMessage" />, or <see langword="null" /> if no such message was found.</returns>
    public async Task<DeletedMessage?> GetDeletedMessage(ulong id)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();

        return await context.DeletedMessages.FirstOrDefaultAsync(m => m.MessageId == id);
    }

    /// <summary>
    ///     Returns an enumerable collection of deleted messages sent by the specified user.
    /// </summary>
    /// <param name="author">The author of the messages.</param>
    /// <param name="guild">The guild.</param>
    /// <returns>An asynchronously enumerable collection of <see cref="DeletedMessage" /> values.</returns>
    public async IAsyncEnumerable<DeletedMessage> GetDeletedMessages(DiscordUser author, DiscordGuild guild)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();

        foreach (DeletedMessage deletedMessage in
                 context.DeletedMessages.Where(m => m.AuthorId == author.Id && m.GuildId == guild.Id))
        {
            yield return deletedMessage;
        }
    }

    private static DiscordEmbed CreateMessageDeletionToAuthorEmbed(DiscordMessage message, GuildConfiguration guildConfiguration)
    {
        DiscordUser author = message.Author;
        if (message.Interaction is not null)
            author = message.Interaction.User;

        var formatObject = new {user = author, channel = message.Channel};
        string description = EmbedMessages.MessageDeletionDescription.FormatSmart(formatObject);

        bool hasContent = !string.IsNullOrWhiteSpace(message.Content);
        bool hasAttachments = message.Attachments.Count > 0;

        string? content = hasContent ? Formatter.Sanitize(message.Content) : null;
        string? attachments = hasAttachments ? string.Join('\n', message.Attachments.Select(a => a.Url)) : null;

        return message.Channel.Guild.CreateDefaultEmbed(guildConfiguration)
            .WithColor(0xFF0000)
            .WithTitle("Message Deleted")
            .WithDescription(description)
            .AddFieldIf(hasContent, "Content", Formatter.BlockCode(content!.Length >= 1014 ? content[..1011] + "..." : content))
            .AddFieldIf(hasAttachments, "Attachments", attachments)
            .AddModMailNotice();
    }

    private static DiscordEmbed CreateMessageDeletionToStaffLogEmbed(
        DiscordMessage message,
        DiscordMember staffMember,
        GuildConfiguration guildConfiguration
    )
    {
        bool hasContent = !string.IsNullOrWhiteSpace(message.Content);
        bool hasAttachments = message.Attachments.Count > 0;

        string? content = hasContent ? Formatter.Sanitize(message.Content) : null;
        string? attachments = hasAttachments ? string.Join('\n', message.Attachments.Select(a => a.Url)) : null;
        string mention = message.Author.IsBot && message.Interaction is not null
            ? $"{message.Interaction.User.Mention} via {message.Author.Mention}"
            : message.Author.Mention;

        DiscordEmbedBuilder embed = message.Channel.Guild.CreateDefaultEmbed(guildConfiguration, false)
            .WithColor(0xFF0000)
            .WithTitle("Message Deleted")
            .WithDescription($"A message in {message.Channel.Mention} was deleted by a staff member.")
            .AddField("Channel", message.Channel.Mention, true)
            .AddField("Author", mention, true)
            .AddField("Staff Member", staffMember.Mention, true)
            .AddField("Message ID", message.Id, true)
            .AddField("Message Time", Formatter.Timestamp(message.CreationTimestamp, TimestampFormat.ShortDateTime),
                true);

        if (hasContent)
        {
            var index = 0;
            foreach (char[] chars in content!.Chunk(1014))
            {
                var chunk = new string(chars);
                embed.AddField(index++ == 0 ? "Content" : "\u200B", Formatter.BlockCode(chunk));
            }
        }

        return embed.AddFieldIf(hasAttachments, "Attachments", attachments);
    }
}
