using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Extensions;
using JetBrains.Annotations;

namespace Hammer.Commands;

/// <summary>
///     Represents a module which implements staff commands.
/// </summary>
internal sealed class MessageCommand
{
    [Command("message")]
    [Description("Sends a private staff message to a member.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task MessageAsync(
        SlashCommandContext context,
        [Parameter("member"), Description("The member to message.")] DiscordUser user
    )
    {
        var embed = new DiscordEmbedBuilder();
        DiscordGuild guild = context.Guild!;
        DiscordMember? member = await user.GetAsMemberOfAsync(guild);

        if (member is null)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Not In Guild");
            embed.WithDescription($"User {user.Id} ({user.Mention}) was found, but is not in this guild.");
            await context.RespondAsync(embed, ephemeral: true);
        }
        else
        {
            var modal = new DiscordModalBuilder();
            modal.WithCustomId(new CustomIdBuilder().Type(CustomIds.MessageMember).Add("user", user.Id).ToString());
            modal.WithTitle("Send Message");


            var messageInput = new DiscordTextInputComponent(
                customId: "message",
                required: true,
                style: DiscordTextInputStyle.Paragraph);

            modal.AddTextInput(messageInput, "Message");

            await context.RespondWithModalAsync(modal);
        }
    }
}
