using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>Delete Message</c> context menu.
/// </summary>
internal sealed class DeleteMessageCommand
{
    private readonly MessageDeletionService _deletionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteMessageCommand" /> class.
    /// </summary>
    /// <param name="deletionService">The message deletion service.</param>
    public DeleteMessageCommand(MessageDeletionService deletionService)
    {
        _deletionService = deletionService;
    }

    [Command("Delete Message")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DeleteMessageAsync(SlashCommandContext context, DiscordMessage message)
    {
        await context.DeferResponseAsync(true);
        var builder = new DiscordWebhookBuilder();
        var embed = new DiscordEmbedBuilder();

        try
        {
            await _deletionService.DeleteMessageAsync(message, context.Member!);
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().ToString());
            embed.WithTitle("Deletion failed");
            embed.WithDescription(exception.Message);
            builder.AddEmbed(embed);
            await context.EditResponseAsync(builder);
            return;
        }

        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Message deleted");
        embed.WithDescription($"Message {message.Id} by {message.Author?.Mention} deleted.");
        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
