using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hammer.Extensions;
using Hammer.Services;
using Microsoft.Extensions.Logging;

namespace Hammer.Commands.Infractions;

/// <summary>
///     Represents a class which implements the <c>Misplaced Jam Link</c> context menu command.
/// </summary>
internal sealed class JamWarnCommand : ApplicationCommandModule
{
    private readonly ILogger<JamWarnCommand> _logger;
    private readonly ConfigurationService _configurationService;
    private readonly RuleService _ruleService;
    private readonly WarningService _warningService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JamWarnCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="ruleService">The rule service.</param>
    /// <param name="warningService">The warning service.</param>
    public JamWarnCommand(ILogger<JamWarnCommand> logger,
        ConfigurationService configurationService,
        RuleService ruleService,
        WarningService warningService)
    {
        _logger = logger;
        _configurationService = configurationService;
        _ruleService = ruleService;
        _warningService = warningService;
    }

    /// <summary>
    ///     Handles the <c>Misplaced Jam Link</c> context menu command.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Misplaced Jam Link", false)]
    [SlashRequireGuild]
    public async Task JamWarnAsync(ContextMenuContext context)
    {
        await context.DeferAsync(true).ConfigureAwait(false);
        var builder = new DiscordEmbedBuilder();
        var message = new DiscordWebhookBuilder();
        var guild = context.Guild;
        var configuration = _configurationService.GetGuildConfiguration(guild);

        if (configuration is null || configuration.JamLinksChannel == 0 || configuration.JamLinksRule == 0)
        {
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing warning");
            builder.WithDescription($"The configuration for guild {guild.Id} could not be found.");

            message.AddEmbed(builder);
            await context.EditResponseAsync(message).ConfigureAwait(false);
            return;
        }

        DiscordUser user = context.Interaction.Data.Resolved.Users.First().Value;
        var reason = $"Jam submissions/streams belong in <#{configuration.JamLinksChannel}>";
        var rule = _ruleService.GetRuleById(guild, configuration.JamLinksRule);
        var importantNotes = new List<string>();

        (var infraction, bool dmSuccess) =
            await _warningService.WarnAsync(user, context.Member, reason, rule).ConfigureAwait(false);

        if (!dmSuccess)
            importantNotes.Add("The warning was successfully issued, but the user could not be DM'd.");

        if (importantNotes.Count > 0)
            builder.AddField("⚠️ Important Notes", string.Join("\n", importantNotes.Select(n => $"• {n}")));

        builder.WithAuthor(user);
        builder.WithColor(DiscordColor.Orange);
        builder.WithTitle("Warned user");
        builder.WithDescription(reason);
        builder.WithFooter($"Infraction {infraction.Id} \u2022 User {user.Id}");

        message.AddEmbed(builder);
        await context.EditResponseAsync(message).ConfigureAwait(false);
    }
}
