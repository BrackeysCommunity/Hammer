using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Extensions;
using JetBrains.Annotations;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    [Command("add")]
    [Description("Add a rule.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task AddAsync(SlashCommandContext context)
    {
        var modal = new DiscordModalBuilder();
        modal.WithCustomId(CustomIds.AddRule);
        modal.WithTitle("Add Rule");

        var briefInput = new DiscordTextInputComponent(customId: "brief", placeholder: "e.g. Be respectful", required: false);
        var descriptionInput = new DiscordTextInputComponent(
            customId: "description",
            placeholder: "e.g. Please treat other members with respect. Refrain from verbal insults and attacks.",
            required: true,
            style: DiscordTextInputStyle.Paragraph);

        modal.AddTextInput(briefInput, "Brief (optional)", "A brief summary of the rule, in few words.");
        modal.AddTextInput(descriptionInput, "Description", "A detailed description of the rule.");

        await context.RespondWithModalAsync(modal);
    }
}
