using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Data;
using Hammer.Extensions;
using JetBrains.Annotations;

namespace Hammer.Commands.Notes;

internal sealed partial class NoteCommand
{
    [Command("edittype")]
    [Description("Edits the type of a note.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task EditTypeAsync(SlashCommandContext context,
        [SlashAutoCompleteProvider<NoteAutoCompleteProvider>] [Parameter("note")] [Description("The note to edit.")]
        long noteId,
        [Parameter("type")] [Description("The new type of the note.")]
        MemberNoteType type)
    {
        var embed = new DiscordEmbedBuilder();

        if (!Enum.IsDefined(type))
        {
            var validTypes = string.Join(", ", Enum.GetNames<MemberNoteType>());
            embed.WithColor(0xFF0000);
            embed.WithTitle("Invalid Note Type");
            embed.WithDescription($"The specified note type {type} is invalid. " +
                                  $"Please use one of the following types: {validTypes}");
            await context.RespondAsync(embed, true);
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
            await _noteService.EditNoteAsync(noteId, type: type);
            embed.WithColor(0x4CAF50);
            embed.WithTitle("Note Updated");
            embed.AddField("Note ID", noteId);
            embed.AddField("Note Type", type.ToString("G"));
        }

        await context.RespondAsync(embed, true);
    }
}
