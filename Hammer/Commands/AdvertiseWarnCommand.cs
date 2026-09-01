using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hammer.Configuration;
using Hammer.Extensions;
using Hammer.Services;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>Misplaced Ad</c> context menu command.
/// </summary>
internal sealed class AdvertiseWarnCommand : ApplicationCommandModule
{
    private readonly ConfigurationService _configurationService;
    private readonly MessageDeletionService _messageDeletionService;
    private readonly RuleService _ruleService;
    private readonly WarningService _warningService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AdvertiseWarnCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="messageDeletionService">The message deletion service.</param>
    /// <param name="ruleService">The rule service.</param>
    /// <param name="warningService">The warning service.</param>
    public AdvertiseWarnCommand(ConfigurationService configurationService,
        MessageDeletionService messageDeletionService,
        RuleService ruleService,
        WarningService warningService)
    {
        _configurationService = configurationService;
        _messageDeletionService = messageDeletionService;
        _ruleService = ruleService;
        _warningService = warningService;
    }

    /// <summary>
    ///     Handles the <c>Misplaced Ad</c> context menu command.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Misplaced Ad", false)]
    [SlashRequireGuild]
    public async Task AdvertiseWarnAsync(ContextMenuContext context)
    {
        await context.DeferAsync(true).ConfigureAwait(false);
        var builder = new DiscordEmbedBuilder();
        var message = new DiscordWebhookBuilder();
        var guild = context.Guild;

        if (!_configurationService.TryGetGuildConfiguration(context.Guild, out GuildConfiguration? configuration))
        {
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing warning");
            builder.WithDescription($"The configuration for guild {guild.Id} could not be found.");

            message.AddEmbed(builder);
            await context.EditResponseAsync(message).ConfigureAwait(false);
            return;
        }

        if (configuration.AdvertiseChannel == 0)
        {
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing warning");
            builder.WithDescription($"The advertise channel for guild {guild.Id} is not configured.");

            message.AddEmbed(builder);
            await context.EditResponseAsync(message).ConfigureAwait(false);
            return;
        }

        if (configuration.AdvertiseRule == 0)
        {
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing warning");
            builder.WithDescription($"The advertise rule for guild {guild.Id} is not configured.");

            message.AddEmbed(builder);
            await context.EditResponseAsync(message).ConfigureAwait(false);
            return;
        }

        await _messageDeletionService.DeleteMessageAsync(context.TargetMessage, context.Member).ConfigureAwait(false);

        DiscordUser user = context.TargetMessage.Author;
        var reason = $"Advertising outside of <#{configuration.AdvertiseChannel}>";
        var rule = _ruleService.GetRuleById(guild, configuration.AdvertiseRule);
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
