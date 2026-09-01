using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Hammer.Services;

/// <summary>
///     Represents a service which listens for user reactions.
/// </summary>
internal sealed class UserReactionService : IEventHandler<MessageReactionAddedEventArgs>
{
    private readonly ConfigurationService _configurationService;
    private readonly MessageReportService _messageReportService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserReactionService" /> class.
    /// </summary>
    public UserReactionService(
        ConfigurationService configurationService,
        MessageReportService messageReportService
    )
    {
        _configurationService = configurationService;
        _messageReportService = messageReportService;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs e)
    {
        if (e.Guild is not { } guild || e.User.IsBot)
        {
            return;
        }

        if (!_configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            return;
        }

        var reactionConfiguration = guildConfiguration.Reactions;
        var reaction = e.Emoji.GetDiscordName();
        if (reaction == reactionConfiguration.ReportReaction)
        {
            await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            await _messageReportService.ReportMessageAsync(e.Message, (DiscordMember)e.User);
        }
    }
}
