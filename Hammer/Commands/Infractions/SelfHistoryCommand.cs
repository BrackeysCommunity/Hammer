using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands.Infractions;

/// <summary>
///     Represents a class which implements the <c>selfhistory</c> command.
/// </summary>
internal sealed class SelfHistoryCommand
{
    private readonly InfractionService _infractionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SelfHistoryCommand" /> class.
    /// </summary>
    /// <param name="infractionService">The infraction service.</param>
    public SelfHistoryCommand(InfractionService infractionService)
    {
        _infractionService = infractionService;
    }

    [Command("selfhistory")]
    [Description("View your own infraction history.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task SelfHistoryAsync(SlashCommandContext context)
    {
        await context.DeferResponseAsync(true);

        var builder = new DiscordWebhookBuilder();
        var response = new InfractionHistoryResponse(_infractionService, context.User, context.User, context.Guild!, false);

        for (var pageIndex = 0; pageIndex < response.Pages; pageIndex++)
        {
            DiscordEmbedBuilder embed = _infractionService.BuildInfractionHistoryEmbed(response, pageIndex);
            builder.AddEmbed(embed);
        }

        await context.EditResponseAsync(builder);
    }
}
