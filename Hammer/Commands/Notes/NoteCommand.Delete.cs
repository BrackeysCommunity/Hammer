using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Extensions;
using JetBrains.Annotations;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [Command("delete")]
    [Description("Deletes a note.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DeleteAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<NoteAutoCompleteProvider>] [Parameter("note")] [Description("The note to delete.")]
        long noteId)
    {
        var embed = new DiscordEmbedBuilder();
        var note = await _noteService.GetNoteAsync(noteId);

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

        await context.RespondAsync(embed, true);
    }
}
