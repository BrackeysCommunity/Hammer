using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Services;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for rules.
/// </summary>
internal sealed class RuleAutoCompleteProvider : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var ruleService = context.ServiceProvider.GetRequiredService<RuleService>();
        var rules = ruleService.GetGuildRules(context.Guild!);

        var result = new List<DiscordAutoCompleteChoice>();
        var optionValue = context.UserInput ?? string.Empty;
        var hasOptionValue = !string.IsNullOrWhiteSpace(optionValue);
        var searchTerms = optionValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var rule in rules)
        {
            if (!hasOptionValue || (int.TryParse(optionValue, out var ruleId) && rule.Id == ruleId) ||
                RuleService.RuleMatches(rule, searchTerms))
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
        var summary = rule.Brief;
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
