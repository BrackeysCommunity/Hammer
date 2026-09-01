using DSharpPlus;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Resources;
using Microsoft.EntityFrameworkCore;
using SmartFormat;

namespace Hammer.Services;

/// <summary>
///     Represents a service which handles message deletions from staff.
/// </summary>
internal sealed class MessageDeletionService
{
    private readonly ConfigurationService _configurationService;
    private readonly IDbContextFactory<HammerContext> _dbContextFactory;
    private readonly DiscordLogService _logService;
    private readonly ILogger<MessageDeletionService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageDeletionService" /> class.
    /// </summary>
    public MessageDeletionService(
        ILogger<MessageDeletionService> logger,
        IDbContextFactory<HammerContext> dbContextFactory,
        ConfigurationService configurationService,
        DiscordLogService logService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _configurationService = configurationService;
        _logService = logService;
    }

    /// <summary>
    ///     Returns the count of deleted messages in the specified guild.
    /// </summary>
    /// <param name="guild">The guild whose deleted messages to count.</param>
    /// <param name="staffMemberId">The ID of the staff member who deleted the messages.</param>
    /// <returns>The count of deleted messages in <paramref name="guild" />.</returns>
    public async Task<int> CountMessageDeletionsAsync(DiscordGuild guild, ulong? staffMemberId = null)
    {
        if (guild is null)
        {
            throw new ArgumentNullException(nameof(guild));
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return staffMemberId is null
            ? context.DeletedMessages.Count(m => m.GuildId == guild.Id)
            : context.DeletedMessages.Count(m => m.GuildId == guild.Id && m.StaffMemberId == staffMemberId);
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
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (staffMember is null)
        {
            throw new ArgumentNullException(nameof(staffMember));
        }

        var channel = message.Channel!;
        _logger.LogInformation("{Message} in channel {Channel} is requested to be deleted by {StaffMember}", message, channel,
            staffMember);

        message = await channel.GetMessageAsync(message.Id);
        var guild = channel.Guild;

        if (guild is null)
        {
            throw new InvalidOperationException(ExceptionMessages.CannotDeleteNonGuildMessage);
        }

        if (guild != staffMember.Guild)
        {
            throw new ArgumentException(ExceptionMessages.MessageStaffMemberGuildMismatch);
        }

        if (!_configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            throw new InvalidOperationException(ExceptionMessages.NoConfigurationForGuild);
        }

        var user = message.Author!;
        var member = await user.GetAsMemberOfAsync(guild);

        if (!staffMember.IsStaffMember(guildConfiguration))
        {
            var exceptionMessage = ExceptionMessages.NotAStaffMember.FormatSmart(new { user = staffMember, guild });
            throw new InvalidOperationException(exceptionMessage);
        }

        if (member is not null)
        {
            if (member.IsHigherLevelThan(staffMember, guildConfiguration))
            {
                var formatObject = new { lower = staffMember, higher = member };
                var exceptionMessage = ExceptionMessages.StaffIsHigherLevel.FormatSmart(formatObject);
                throw new InvalidOperationException(exceptionMessage);
            }

            if (notifyAuthor)
            {
                try
                {
                    var toAuthorEmbed = CreateMessageDeletionToAuthorEmbed(message, guildConfiguration);
                    await member.SendMessageAsync(toAuthorEmbed);
                }
                catch
                {
                    _logger.LogWarning("{Member} could not be notified of the deletion", member);
                    // ignored
                }
            }
        }

        var staffLogEmbed = CreateMessageDeletionToStaffLogEmbed(message, staffMember, guildConfiguration);

        var deletedMessage = DeletedMessage.Create(message, staffMember);
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.AddAsync(deletedMessage);
        await context.SaveChangesAsync();

        _logger.LogInformation("{Message} in {Channel} was deleted by {StaffMember}", message, channel, staffMember);
        await message.DeleteAsync($"Deleted by {staffMember.GetUsernameWithDiscriminator()}");
        await _logService.LogAsync(guild, staffLogEmbed);
    }

    /// <summary>
    ///     Returns a deleted message by its ID.
    /// </summary>
    /// <param name="id">The ID of the message to retrieve.</param>
    /// <returns>A <see cref="DeletedMessage" />, or <see langword="null" /> if no such message was found.</returns>
    public async Task<DeletedMessage?> GetDeletedMessage(ulong id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
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
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        foreach (var deletedMessage in
                 context.DeletedMessages.Where(m => m.AuthorId == author.Id && m.GuildId == guild.Id)
                     .AsEnumerable()
                     .OrderBy(m => m.DeletionTimestamp))
        {
            yield return deletedMessage;
        }
    }

    private static DiscordEmbed CreateMessageDeletionToAuthorEmbed(DiscordMessage message, GuildConfiguration guildConfiguration)
    {
        var author = message.Author!;
        if (message.Interaction is not null)
        {
            author = message.Interaction.User!;
        }

        var formatObject = new { user = author, channel = message.Channel };
        var description = EmbedMessages.MessageDeletionDescription.FormatSmart(formatObject);

        var hasContent = !string.IsNullOrWhiteSpace(message.Content);
        var hasAttachments = message.Attachments.Count > 0;

        var content = hasContent ? Formatter.Sanitize(message.Content) : null;
        var attachments = hasAttachments ? string.Join('\n', message.Attachments.Select(a => a.Url)) : null;

        return message.Channel!.Guild.CreateDefaultEmbed(guildConfiguration)
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
        var hasContent = !string.IsNullOrWhiteSpace(message.Content);
        var hasAttachments = message.Attachments.Count > 0;

        var content = hasContent ? Formatter.Sanitize(message.Content) : null;
        var attachments = hasAttachments ? string.Join('\n', message.Attachments.Select(a => a.Url)) : null;
        var mention = message.Author!.IsBot && message.Interaction is not null
            ? $"{message.Interaction.User!.Mention} via {message.Author.Mention}"
            : message.Author.Mention;

        var embed = message.Channel!.Guild.CreateDefaultEmbed(guildConfiguration, false)
            .WithColor(0xFF0000)
            .WithTitle("Message Deleted")
            .WithDescription($"A message in {message.Channel.Mention} was deleted by a staff member.")
            .AddField("Channel", message.Channel.Mention, true)
            .AddField("Author", mention, true)
            .AddField("Staff Member", staffMember.Mention, true)
            .AddField("Message ID", message.Id, true)
            .AddField("Message Time", Formatter.Timestamp(message.CreationTimestamp, TimestampFormat.ShortDateTime), true);

        if (hasContent)
        {
            var index = 0;
            foreach (var chars in content!.Chunk(1014))
            {
                var chunk = new string(chars);
                embed.AddField(index++ == 0 ? "Content" : "\u200B", Formatter.BlockCode(chunk));
            }
        }

        return embed.AddFieldIf(hasAttachments, "Attachments", attachments);
    }
}
