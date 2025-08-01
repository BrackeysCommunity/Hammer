using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Interactivity;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    [Command("add")]
    [Description("Add a rule.")]
    [RequireGuild]
    public async Task AddAsync(CommandContext context)
    {
        DiscordGuild guild = context.Guild;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            await context.CreateResponseAsync("This guild is not configured.", true);
            return;
        }

        var modal = new DiscordModalBuilder(context.Client);
        modal.WithTitle("Add Rule");
        DiscordModalTextInput brief = modal.AddInput("Brief Description",
            "e.g. Be respectful",
            isRequired: false);
        DiscordModalTextInput description = modal.AddInput("Description",
            "e.g. Please treat other members with respect. Refrain from verbal insults and attacks.",
            isRequired: true,
            inputStyle: DiscordTextInputStyle.Paragraph);

        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5));

        if (response == DiscordModalResponse.Success)
        {
            Rule rule = _ruleService.AddRule(guild, description.Value!, brief.Value);
            DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(guildConfiguration, false);
            embed.WithColor(DiscordColor.Green);
            embed.WithTitle($"Rule #{rule.Id} added");
            if (string.IsNullOrWhiteSpace(brief.Value))
                embed.WithDescription(rule.Description);
            else
                embed.AddField(rule.Brief, rule.Description);

            var webhook = new DiscordWebhookBuilder();
            webhook.AddEmbed(embed);
            await context.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
        }
    }
}
