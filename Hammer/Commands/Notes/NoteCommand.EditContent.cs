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
    [Command("editcontent")]
    [Description("Edits the content of a note.")]
    [RequireGuild]
    public async Task EditContentAsync(CommandContext context,
        [Autocomplete(typeof(NoteAutoCompleteProvider))] [Parameter("note"), Description("The note to edit.")]
        long noteId,
        [Parameter("content"), Description("The new content of the note.")]
        string content)
    {
        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(content))
            return;

        MemberNote? note = await _noteService.GetNoteAsync(noteId);

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
            await context.CreateResponseAsync(embed, true);
            return;
        }

        await _noteService.EditNoteAsync(noteId, content);
        embed.WithTitle("Note Updated");
        embed.AddField("Note ID", note.Id);
        embed.AddField("Content", note.Content);
        embed.WithColor(0x4CAF50);
        await context.CreateResponseAsync(embed, true);
    }
}
