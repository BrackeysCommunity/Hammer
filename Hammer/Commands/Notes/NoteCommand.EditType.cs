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
    [Command("edittype")]
    [Description("Edits the type of a note.")]
    [RequireGuild]
    public async Task EditTypeAsync(CommandContext context,
        [Autocomplete(typeof(NoteAutoCompleteProvider))] [Parameter("note"), Description("The note to edit.")]
        long noteId,
        [Parameter("type"), Description("The new type of the note.")]
        MemberNoteType type)
    {
        var embed = new DiscordEmbedBuilder();

        if (!Enum.IsDefined(type))
        {
            string validTypes = string.Join(", ", Enum.GetNames<MemberNoteType>());
            embed.WithColor(0xFF0000);
            embed.WithTitle("Invalid Note Type");
            embed.WithDescription($"The specified note type {type} is invalid. " +
                                  $"Please use one of the following types: {validTypes}");
            await context.CreateResponseAsync(embed, true);
            return;
        }

        MemberNote? note = await _noteService.GetNoteAsync(noteId);

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
            await context.CreateResponseAsync(embed, true);
            return;
        }

        await _noteService.EditNoteAsync(noteId, type: type);
        embed.WithTitle("Note Updated");
        embed.AddField("Note ID", noteId);
        embed.AddField("Note Type", type.ToString("G"));
        embed.WithColor(0x4CAF50);
        await context.CreateResponseAsync(embed, true);
    }
}
