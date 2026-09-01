using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Extensions;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    [Command("delete")]
    [Description("Deletes a rule.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DeleteAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<RuleAutoCompleteProvider>] [Parameter("rule")] [Description("The rule to modify")]
        long ruleId)
    {
        var guild = context.Guild!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        await context.DeferResponseAsync();

        var builder = new DiscordWebhookBuilder();

        if (!_ruleService.GuildHasRule(guild, (int)ruleId))
        {
            builder.AddEmbed(RuleService.CreateRuleNotFoundEmbed((int)ruleId));
            await context.EditResponseAsync(builder);
            return;
        }

        _ruleService.DeleteRule(guild, (int)ruleId);

        var embed = guild.CreateDefaultEmbed(guildConfiguration, false);
        embed.WithColor(0x4CAF50);
        embed.WithTitle($"Rule {ruleId} deleted");
        embed.WithDescription("To view the new rules, use the `/rules` command.");

        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }
}
