using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using JetBrains.Annotations;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    [Command("display")]
    [Description("Displays 1 or more embeds with the guild rules.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DisplayAsync(SlashCommandContext context,
        [Parameter("channel")] [Description("The channel in which to display the rules. Defaults to the current channel.")]
        DiscordChannel? channel = null)
    {
        channel ??= context.Channel;
        await context.RespondAsync($"Sending rules to {channel.Mention}", true);
        await _ruleService.SendRulesMessageAsync(channel);
    }
}
