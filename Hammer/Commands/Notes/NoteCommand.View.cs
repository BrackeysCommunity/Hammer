using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using Hammer.AutocompleteProviders;
using Hammer.Data;
using Hammer.Extensions;
using JetBrains.Annotations;
using PermissionLevel = Hammer.Data.PermissionLevel;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [Command("view")]
    [Description("Views a note.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ViewAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<NoteAutoCompleteProvider>] [Parameter("note")] [Description("The note to view.")]
        long noteId)
    {
        var guild = context.Guild!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        var note = await _noteService.GetNoteAsync(noteId);
        var embed = guild.CreateDefaultEmbed(guildConfiguration, false);

        if (note?.GuildId != guild.Id)
            // cannot view notes saved for other guilds
        {
            note = null;
        }

        if (note?.Type == MemberNoteType.Staff &&
            context.Member!.GetPermissionLevel(guildConfiguration) < PermissionLevel.Moderator)
            // non-staff cannot see staff notes
        {
            note = null;
        }

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
            await context.RespondAsync(embed, true);
            return;
        }

        var author = await context.Client.GetUserAsync(note.AuthorId);
        var user = await context.Client.GetUserAsync(note.UserId);
        var timestamp = Formatter.Timestamp(note.CreationTimestamp, TimestampFormat.ShortDateTime);

        embed.WithAuthor(user);
        embed.AddField("Note ID", note.Id, true);
        embed.AddField("Note Type", note.Type.ToString("G"), true);
        embed.AddField("Author", author.Mention, true);
        embed.AddField("Creation Time", timestamp, true);
        embed.AddField("Content", note.Content);
        await context.RespondAsync(embed, true);
    }
}
