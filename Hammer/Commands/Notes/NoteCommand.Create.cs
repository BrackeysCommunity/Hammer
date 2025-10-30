using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [Command("create")]
    [Description("Creates a new note.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task CreateAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user for whom to create a note.")] DiscordUser user,
        [Parameter("content"), Description("The content of the note.")] string content)
    {
        DiscordGuild guild = context.Guild!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(guildConfiguration, false);

        try
        {
            MemberNote note = await _noteService.CreateNoteAsync(user, context.Member, content);

            embed.WithColor(0x4CAF50);
            embed.WithTitle("Note Created");
            embed.WithDescription($"Successfully created note for {user.Mention}");
            embed.WithFooter($"Note #{note.Id}");

            _logger.LogInformation("{User} created note for {Target} in {Guild}: {Content}",
                context.User, user, guild, content);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An exception was thrown when attempting to save a note for {User} (\"{Content}\")",
                user, content);

            embed.WithColor(0xFF0000);
            embed.WithTitle(exception.GetType().Name);
            embed.WithDescription(exception.Message);
            embed.WithFooter("See log for more details.");
        }

        await context.RespondAsync(embed, true);
    }
}
