using System.ComponentModel;
using System.Text.RegularExpressions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using JetBrains.Annotations;

namespace Hammer.Commands.Rules;

internal sealed partial class RulesCommand
{
    private static readonly Regex MessageLinkRegex = GetMessageLinkRegex();

    [Command("update")]
    [Description("Sends the rule embed.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task UpdateAsync(SlashCommandContext context,
        [Parameter("messageLink"), Description("The link to the message to edit.")] string messageLink)
    {
        Match match = MessageLinkRegex.Match(messageLink);

        if (!match.Success)
        {
            await context.RespondAsync("Invalid message link.", true);
            return;
        }

        var guildId = ulong.Parse(match.Groups[1].Value);
        if (guildId != context.Guild!.Id)
        {
            await context.RespondAsync("Invalid message link.", true);
            return;
        }

        DiscordChannel channel;
        try
        {
            channel = await context.Guild.GetChannelAsync(ulong.Parse(match.Groups[2].Value));
        }
        catch (NotFoundException)
        {
            await context.RespondAsync("Invalid message link.", true);
            return;
        }

        DiscordMessage message;

        try
        {
            message = await channel.GetMessageAsync(ulong.Parse(match.Groups[3].Value));
        }
        catch (NotFoundException)
        {
            await context.RespondAsync("Invalid message link.", true);
            return;
        }

        if (message.Author != context.Client.CurrentUser)
        {
            await context.RespondAsync("Invalid message link.", true);
            return;
        }

        await context.RespondAsync($"Sending rules to {channel.Mention}", true);
        await _ruleService.ModifyRulesMessageAsync(message);
    }

    [GeneratedRegex(@"https://discord\.com/channels/(\d+)/(\d+)/(\d+)", RegexOptions.Compiled)]
    private static partial Regex GetMessageLinkRegex();
}
