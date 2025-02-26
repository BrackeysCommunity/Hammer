using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Data;
using Hammer.Extensions;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [Command("delete")]
    [Description("Deletes a note.")]
    [RequireGuild]
    public async Task DeleteAsync(CommandContext context,
        [Autocomplete(typeof(NoteAutocompleteProvider))] [Parameter("note"), Description("The note to delete.")]
        long noteId)
    {
        var embed = new DiscordEmbedBuilder();
        MemberNote? note = await _noteService.GetNoteAsync(noteId);

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
            await context.CreateResponseAsync(embed, true);
            return;
        }

        await _noteService.DeleteNoteAsync(note.Id);
        embed.WithTitle("Note Deleted");
        embed.AddField("Note ID", note.Id);
        embed.AddField("Content", note.Content);
        embed.WithColor(0x4CAF50);
        await context.CreateResponseAsync(embed, true);
    }
}
