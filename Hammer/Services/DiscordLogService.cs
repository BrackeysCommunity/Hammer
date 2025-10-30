using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Hammer.Configuration;
using Hammer.Data;

namespace Hammer.Services;

/// <summary>
///     Represents a service which can send embeds to a log channel.
/// </summary>
internal sealed class DiscordLogService : IEventHandler<GuildAvailableEventArgs>
{
    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;
    private readonly Dictionary<DiscordGuild, DiscordChannel> _logChannels = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordLogService" /> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="configurationService">The configuration service.</param>
    public DiscordLogService(ConfigurationService configurationService, DiscordClient discordClient)
    {
        _discordClient = discordClient;
        _configurationService = configurationService;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, GuildAvailableEventArgs e)
    {
        if (!_configurationService.TryGetGuildConfiguration(e.Guild, out GuildConfiguration? configuration))
        {
            return;
        }

        ulong logChannel = configuration.LogChannel;
        if (logChannel == 0)
        {
            return;
        }

        try
        {
            DiscordChannel channel = await _discordClient.GetChannelAsync(logChannel);
            _logChannels[e.Guild] = channel;
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     Sends an embed to the log channel of the specified guild.
    /// </summary>
    /// <param name="guild">The guild whose log channel in which to post the embed.</param>
    /// <param name="embed">The embed to post.</param>
    /// <param name="notificationOptions">
    ///     Optional. The staff notification options. Defaults to <see cref="StaffNotificationOptions.None" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" /> or <paramref name="embed" /> is <see langword="null" />.
    /// </exception>
    public async Task LogAsync(DiscordGuild guild, DiscordEmbed embed,
        StaffNotificationOptions notificationOptions = StaffNotificationOptions.None)
    {
        if (guild is null)
        {
            throw new ArgumentNullException(nameof(guild));
        }

        if (embed is null)
        {
            throw new ArgumentNullException(nameof(embed));
        }

        if (_logChannels.TryGetValue(guild, out DiscordChannel? logChannel))
        {
            if (embed.Timestamp is null)
            {
                embed = new DiscordEmbedBuilder(embed).WithTimestamp(DateTimeOffset.UtcNow);
            }

            string? mentionString = BuildMentionString(guild, notificationOptions);
            if (mentionString is null)
            {
                await logChannel.SendMessageAsync(embed);
            }
            else
            {
                await logChannel.SendMessageAsync(mentionString, embed);
            }
        }
    }

    /// <summary>
    ///     Gets the log channel for a specified guild.
    /// </summary>
    /// <param name="guild">The guild whose log channel to retrieve.</param>
    /// <param name="channel">
    ///     When this method returns, contains the log channel; or <see langword="null" /> if no such channel is found.
    /// </param>
    /// <returns><see langword="true" /> if the log channel was successfully found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="guild" /> is <see langword="null" />.</exception>
    public bool TryGetLogChannel(DiscordGuild guild, [NotNullWhen(true)] out DiscordChannel? channel)
    {
        if (guild is null)
        {
            throw new ArgumentNullException(nameof(guild));
        }

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
        {
            channel = null;
            return false;
        }

        if (!_logChannels.TryGetValue(guild, out channel) && guild.Channels.TryGetValue(configuration.LogChannel, out channel))
        {
            _logChannels.Add(guild, channel);
        }

        return channel is not null;
    }

    private string? BuildMentionString(DiscordGuild guild, StaffNotificationOptions notificationOptions)
    {
        if (!TryGetLogChannel(guild, out DiscordChannel? logChannel))
        {
            return null;
        }

        if (notificationOptions == StaffNotificationOptions.None)
        {
            return null;
        }

        if (!_configurationService.TryGetGuildConfiguration(logChannel.Guild, out GuildConfiguration? configuration))
        {
            return null;
        }

        RoleConfiguration roleConfiguration = configuration.Roles;
        DiscordRole administratorRole = logChannel.Guild.Roles[roleConfiguration.AdministratorRoleId];
        DiscordRole moderatorRole = logChannel.Guild.Roles[roleConfiguration.ModeratorRoleId];

        var mentions = new List<string>();

        if ((notificationOptions & StaffNotificationOptions.Administrator) != 0)
        {
            mentions.Add(administratorRole.Mention);
        }

        if ((notificationOptions & StaffNotificationOptions.Moderator) != 0)
        {
            mentions.Add(moderatorRole.Mention);
        }

        if ((notificationOptions & StaffNotificationOptions.Here) != 0)
        {
            mentions.Add("@here");
        }

        if ((notificationOptions & StaffNotificationOptions.Everyone) != 0)
        {
            mentions.Add("@everyone");
        }

        return string.Join(' ', mentions);
    }
}
