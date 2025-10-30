using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>Warn For This</c> context menu.
/// </summary>
internal sealed class BadMessageCommand
{
    private readonly ConfigurationService _configurationService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadMessageCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="ruleService">The rule service.</param>
    public BadMessageCommand(ConfigurationService configurationService, RuleService ruleService)
    {
        _configurationService = configurationService;
        _ruleService = ruleService;
    }

    [Command("Warn For This")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [RequireGuild]
    public async Task BadMessageAsync(SlashCommandContext context, DiscordMessage message)
    {
        DiscordGuild guild = context.Guild!;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
        {
            configuration = new GuildConfiguration();
        }

        string defaultReason = configuration.DefaultBadMessageWarning;

        var id = new CustomIdBuilder();
        id.Type(CustomIds.BadMessageWarning);
        id.Add("channel", message.Channel!.Id.ToString());
        id.Add("message", message.Id.ToString());

        var modal = new DiscordModalBuilder();
        modal.WithCustomId(id.ToString());
        modal.WithTitle("Warning Details");

        IReadOnlyList<Rule> rules = _ruleService.GetGuildRules(guild);
        var options = new List<DiscordSelectComponentOption>();
        foreach (Rule rule in rules)
        {
            var option = new DiscordSelectComponentOption
            (
                label: $"#{rule.Id} - {rule.Brief}",
                value: rule.Id.ToString(),
                description: rule.Description
            );
            options.Add(option);
        }

        var ruleInput = new DiscordSelectComponent("rule", "Choose ...", options);

        var reasonInput = new DiscordTextInputComponent(
            value: defaultReason,
            customId: "reason",
            required: false,
            style: DiscordTextInputStyle.Paragraph,
            max_length: 250);

        modal.AddSelectMenu(ruleInput, "Rule", "The rule which was broken.");
        modal.AddTextInput(reasonInput, "Reason", "The reason for the warning.");

        await context.RespondWithModalAsync(modal);
    }
}
