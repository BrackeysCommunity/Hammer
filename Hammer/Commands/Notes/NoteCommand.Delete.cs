using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hammer.AutocompleteProviders;
using Hammer.Data;
using Hammer.Extensions;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [SlashCommand("delete", "Deletes a note.", false)]
    [SlashRequireGuild]
    public async Task DeleteAsync(InteractionContext context,
        [Autocomplete(typeof(NoteAutocompleteProvider))] [Option("note", "The note to delete.")] long noteId)
    {
        var embed = new DiscordEmbedBuilder();
        MemberNote? note = await _noteService.GetNoteAsync(noteId);

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
        }
        else
        {
            await _noteService.DeleteNoteAsync(note.Id);
            embed.WithColor(0x4CAF50);
            embed.WithTitle("Note Deleted");
            embed.AddField("Note ID", note.Id);
            embed.AddField("Content", note.Content);
        }

        await context.CreateResponseAsync(embed, true);
    }
}
