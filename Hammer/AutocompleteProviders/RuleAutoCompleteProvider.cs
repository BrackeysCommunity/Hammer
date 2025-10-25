using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for rules.
/// </summary>
internal sealed class RuleAutoCompleteProvider : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var ruleService = context.ServiceProvider.GetRequiredService<RuleService>();
        IReadOnlyList<Rule> rules = ruleService.GetGuildRules(context.Guild);

        var result = new List<DiscordAutoCompleteChoice>();
        string optionValue = context.UserInput ?? string.Empty;
        bool hasOptionValue = !string.IsNullOrWhiteSpace(optionValue);
        string[] searchTerms = optionValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (Rule rule in rules)
        {
            if (!hasOptionValue || (int.TryParse(optionValue, out int ruleId) && rule.Id == ruleId) ||
                ruleService.RuleMatches(rule, searchTerms))
            {
                result.Add(new DiscordAutoCompleteChoice(GetRuleDescription(rule), rule.Id.ToString()));
            }

            if (result.Count >= 25)
            {
                // Discord only allows 25 choices per autocomplete response
                break;
            }
        }

        return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(result);
    }

    private static string GetRuleDescription(Rule rule)
    {
        string? summary = rule.Brief;
        if (string.IsNullOrWhiteSpace(summary))
        {
            summary = rule.Description;
            if (summary.Length > 50)
            {
                summary = summary[..50] + "...";
            }
        }

        return $"{rule.Id}: {summary}";
    }
}
