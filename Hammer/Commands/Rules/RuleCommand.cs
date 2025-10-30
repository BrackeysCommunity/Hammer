using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands.Rules;

/// <summary>
///     Represents a class which implements the <c>rule</c> command.
/// </summary>
internal sealed class RuleCommand
{
    private readonly ConfigurationService _configurationService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuleCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="ruleService">The rule service.</param>
    public RuleCommand(ConfigurationService configurationService, RuleService ruleService)
    {
        _configurationService = configurationService;
        _ruleService = ruleService;
    }

    [Command("rule")]
    [Description("Displays a rule.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task RuleAsync(SlashCommandContext context,
        [Parameter("rule"), Description("The rule to display.")]
        [SlashAutoCompleteProvider<RuleAutoCompleteProvider>]
        string search,
        [Parameter("mention"), Description("The user to mention.")] DiscordUser? mentionUser = null)
    {
        DiscordGuild guild = context.Guild!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        Rule? rule;

        if (int.TryParse(search, out int ruleId))
        {
            if (!_ruleService.GuildHasRule(guild, ruleId))
            {
                await context.RespondAsync(RuleService.CreateRuleNotFoundEmbed(ruleId), true);
                return;
            }

            rule = _ruleService.GetRuleById(guild, ruleId);
        }
        else
        {
            rule = _ruleService.SearchForRule(guild, search);
            if (rule is null)
            {
                await context.RespondAsync(RuleService.CreateRuleNotFoundEmbed(search), true);
                return;
            }
        }

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(guildConfiguration, false);
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle(string.IsNullOrWhiteSpace(rule.Brief) ? $"Rule #{rule.Id}" : $"Rule #{rule.Id}. {rule.Brief}");
        embed.WithDescription(rule.Description);

        var response = new DiscordInteractionResponseBuilder();
        response.AddEmbed(embed);

        if (mentionUser is not null)
        {
            response.WithContent(mentionUser.Mention);
            response.AddMention(new UserMention(mentionUser.Id));
        }

        await context.RespondAsync(response).ConfigureAwait(false);
    }
}
