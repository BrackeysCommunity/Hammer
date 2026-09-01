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
    [Command("editcontent")]
    [Description("Edits the content of a note.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task EditContentAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<NoteAutoCompleteProvider>] [Parameter("note")] [Description("The note to edit.")]
        long noteId,
        [Parameter("content")] [Description("The new content of the note.")]
        string content)
    {
        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var note = await _noteService.GetNoteAsync(noteId);

        if (note is null)
        {
            embed.WithColor(0xFF0000);
            embed.WithTitle("No Such Note");
            embed.WithDescription($"No note with the ID {noteId} could be found.");
        }
        else
        {
            await _noteService.EditNoteAsync(noteId, content);
            embed.WithColor(0x4CAF50);
            embed.WithTitle("Note Updated");
            embed.AddField("Note ID", note.Id);
            embed.AddField("Content", note.Content);
        }

        await context.RespondAsync(embed, true);
    }
}
