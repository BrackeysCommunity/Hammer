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
using JetBrains.Annotations;
using X10D.Text;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    [Command("edit")]
    [Description("Edits a rule.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task EditAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<RuleAutoCompleteProvider>] [Parameter("rule"), Description("The rule to modify.")] long ruleId)
    {
        DiscordGuild guild = context.Guild!;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? _))
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        if (!_ruleService.GuildHasRule(guild, (int)ruleId))
        {
            DiscordEmbed embed = Services.RuleService.CreateRuleNotFoundEmbed((int)ruleId);
            await context.RespondAsync(embed, true);
            return;
        }

        var ruleResult = _ruleService.GetRuleById(guild, (int)ruleId);
        if (!ruleResult.IsSuccess)
        {
            await context.RespondAsync("An error occurred while fetching the rule.", true);
            return;
        }

        Rule rule = ruleResult.Value;

        var id = new CustomIdBuilder();
        id.Type(CustomIds.EditRule);
        id.Add("rule", rule.Id.ToString());

        var modal = new DiscordModalBuilder();
        modal.WithCustomId(id.ToString());
        modal.WithTitle("Edit Rule");

        var briefInput = new DiscordTextInputComponent(
            customId: "brief",
            placeholder: "e.g. Be respectful",
            required: false,
            value: rule.Brief?.AsNullIfWhiteSpace());

        var descriptionInput = new DiscordTextInputComponent(
            customId: "description",
            placeholder: "e.g. Please treat other members with respect. Refrain from verbal insults and attacks.",
            required: true,
            style: DiscordTextInputStyle.Paragraph,
            value: rule.Description.AsNullIfWhiteSpace());

        modal.AddTextInput(briefInput, "Brief (optional)", "A brief summary of the rule, in few words.");
        modal.AddTextInput(descriptionInput, "Description", "A detailed description of the rule.");

        await context.RespondWithModalAsync(modal);
    }
}
