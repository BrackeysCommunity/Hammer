using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Hammer.Data;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Services;

/// <summary>
///     Represents a service which tracks specific user messages.
/// </summary>
internal sealed class MessageTrackingService : IEventHandler<GuildAvailableEventArgs>,
    IEventHandler<MessageDeletedEventArgs>,
    IEventHandler<MessageUpdatedEventArgs>
{
    private readonly IDbContextFactory<HammerContext> _dbContextFactory;
    private readonly ILogger<MessageTrackingService> _logger;
    private readonly List<TrackedMessage> _trackedMessages = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageReportService" /> class.
    /// </summary>
    public MessageTrackingService(ILogger<MessageTrackingService> logger,
        IDbContextFactory<HammerContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public Task HandleEventAsync(DiscordClient sender, GuildAvailableEventArgs e)
    {
        return RefreshFromDatabaseAsync(e.Guild);
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, MessageDeletedEventArgs e)
    {
        if (GetMessageTrackState(e.Message) != MessageTrackState.Tracked)
        {
            return;
        }

        var trackedMessage = await GetTrackedMessageAsync(e.Message);
        trackedMessage.IsDeleted = true;
        trackedMessage.DeletionTimestamp = DateTimeOffset.UtcNow;

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Update(trackedMessage);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, MessageUpdatedEventArgs e)
    {
        if (e.Message.Channel?.Guild is null)
        {
            return;
        }

        if (GetMessageTrackState(e.Message) != MessageTrackState.Tracked)
        {
            return;
        }

        var trackedMessage = await GetTrackedMessageAsync(e.Message);
        trackedMessage.Content = e.Message.Content;

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Update(trackedMessage);
        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Enumerates the tracked messages written by a user in a specified guild.
    /// </summary>
    /// <param name="user">The user whose tracked messages to retrieve.</param>
    /// <param name="guild">The guild whose messages to search.</param>
    /// <returns>An enumerable collection of <see cref="TrackedMessage" /> instances.</returns>
    public async IAsyncEnumerable<TrackedMessage> EnumerateTrackedMessagesAsync(DiscordUser user, DiscordGuild guild)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        foreach (var message in context.TrackedMessages.Where(m => m.AuthorId == user.Id && m.GuildId == guild.Id))
        {
            yield return message;
        }
    }

    /// <summary>
    ///     Determines the current tracking state of a specified message ID.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="messageId">The message ID.</param>
    /// <returns>A <see cref="MessageTrackState" /> representing the tracked state of the specified message.</returns>
    public MessageTrackState GetMessageTrackState(ulong guildId, ulong channelId, ulong messageId)
    {
        var trackedMessage = _trackedMessages.Find(m => messageId == m.Id
                                                        && m.ChannelId == channelId
                                                        && m.GuildId == guildId);

        if (trackedMessage is null)
        {
            return MessageTrackState.NotTracked;
        }

        if (trackedMessage.IsDeleted)
        {
            return MessageTrackState.Tracked | MessageTrackState.Deleted;
        }

        return MessageTrackState.Tracked;
    }

    /// <summary>
    ///     Determines the current tracking state of a specified message.
    /// </summary>
    /// <param name="message">The message whose status to retrieve.</param>
    /// <returns>A <see cref="MessageTrackState" /> representing the tracked state of the specified message.</returns>
    public MessageTrackState GetMessageTrackState(DiscordMessage message)
    {
        return GetMessageTrackState(message.Channel!.Guild.Id, message.Channel.Id, message.Id);
    }

    /// <summary>
    ///     Gets the <see cref="TrackedMessage" /> for a specified <see cref="DiscordMessage" />, creating a new one if the
    ///     message is not already being tracked.
    /// </summary>
    /// <param name="message">The <see cref="DiscordMessage" /> to track.</param>
    /// <param name="deleted"><see langword="true" /> to mark the message as deleted; otherwise, <see langword="false" />.</param>
    /// <returns>
    ///     A <see cref="TrackedMessage" /> representing the tracked message mapping of <paramref name="message" />.
    /// </returns>
    public async Task<TrackedMessage> GetTrackedMessageAsync(DiscordMessage message, bool deleted = false)
    {
        var trackedMessage = _trackedMessages.Find(m => m.Id == message.Id);

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        if (trackedMessage is null)
        {
            trackedMessage = await context.TrackedMessages.FirstOrDefaultAsync(m => m.Id == message.Id);

            if (trackedMessage is null)
            {
                trackedMessage = TrackedMessage.FromDiscordMessage(message);
                trackedMessage.IsDeleted = deleted;
                if (deleted)
                {
                    trackedMessage.DeletionTimestamp = DateTimeOffset.UtcNow;
                }

                var entry = await context.AddAsync(trackedMessage);
                trackedMessage = entry.Entity;
            }
            else
            {
                trackedMessage.IsDeleted = deleted;
                if (deleted)
                {
                    trackedMessage.DeletionTimestamp = DateTimeOffset.UtcNow;
                }

                context.Update(trackedMessage);
            }

            _trackedMessages.Add(trackedMessage);
        }
        else
        {
            trackedMessage.IsDeleted = deleted;
            if (deleted)
            {
                trackedMessage.DeletionTimestamp = DateTimeOffset.UtcNow;
            }

            context.Update(trackedMessage);
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An exception was thrown when saving TrackedMessage to the database");
        }

        return trackedMessage;
    }

    private async Task RefreshFromDatabaseAsync(DiscordGuild guild)
    {
        var guildId = guild.Id;

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var messages = context.TrackedMessages.Where(m => m.GuildId == guildId).AsEnumerable();

        foreach (var channelGroups in messages.GroupBy(m => m.ChannelId))
        {
            DiscordChannel channel;
            try
            {
                channel = await guild.GetChannelAsync(channelGroups.Key);
            }
            catch (NotFoundException)
            {
                foreach (var trackedMessage in channelGroups)
                {
                    trackedMessage.IsDeleted = true;
                }

                context.UpdateRange(channelGroups);
                continue;
            }

            foreach (var trackedMessage in channelGroups)
            {
                try
                {
                    await channel.GetMessageAsync(trackedMessage.Id);
                    _trackedMessages.Add(trackedMessage);
                }
                catch (NotFoundException)
                {
                    trackedMessage.IsDeleted = true;
                }
            }

            context.UpdateRange(channelGroups);
        }

        await context.SaveChangesAsync();
    }
}
