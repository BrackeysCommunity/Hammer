using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Hammer.Data;
using Hammer.Extensions;

namespace Hammer.Services;

/// <summary>
///     Represents a service which listens for staff reactions.
/// </summary>
internal sealed class StaffReactionService : IEventHandler<MessageReactionAddedEventArgs>
{
    private readonly ConfigurationService _configurationService;
    private readonly MessageDeletionService _deletionService;
    private readonly InfractionService _infractionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StaffReactionService" /> class.
    /// </summary>
    public StaffReactionService(ConfigurationService configurationService,
        InfractionService infractionService,
        MessageDeletionService deletionService)
    {
        _configurationService = configurationService;
        _infractionService = infractionService;
        _deletionService = deletionService;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs e)
    {
        if (e.Guild is not { } guild || e.User.IsBot)
        {
            return;
        }

        var message = e.Message;

        if (message.Author is null)
        {
            // not cached! fetch new message
            message = await message.Channel!.GetMessageAsync(message.Id);
        }

        var author = message.Author!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out var configuration))
        {
            return;
        }

        var staffMember = (DiscordMember)e.User;
        if (!staffMember.IsStaffMember(configuration))
        {
            return;
        }

        var reactionConfiguration = configuration.Reactions;
        var emoji = e.Emoji;
        var reaction = emoji.GetDiscordName();

        if (reaction == reactionConfiguration.GagReaction)
        {
            await message.DeleteReactionAsync(emoji, staffMember);
            await _infractionService.GagAsync(author, staffMember, message);
        }
        else if (reaction == reactionConfiguration.HistoryReaction)
        {
            await message.DeleteReactionAsync(emoji, staffMember);

            var builder = new DiscordMessageBuilder();
            var response = new InfractionHistoryResponse(_infractionService, author, staffMember, guild, true);

            for (var pageIndex = 0; pageIndex < response.Pages; pageIndex++)
            {
                var embed = _infractionService.BuildInfractionHistoryEmbed(response, pageIndex);
                builder.AddEmbed(embed);
            }

            await staffMember.SendMessageAsync(builder);
        }
        else if (reaction == reactionConfiguration.DeleteMessageReaction)
        {
            await message.DeleteReactionAsync(emoji, staffMember);
            await _deletionService.DeleteMessageAsync(message, staffMember);
        }
    }
}
