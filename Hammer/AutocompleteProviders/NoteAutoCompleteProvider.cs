using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for notes.
/// </summary>
internal sealed class NoteAutoCompleteProvider : IAutoCompleteProvider
{
    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var serviceProvider = context.ServiceProvider;
        var noteService = serviceProvider.GetRequiredService<MemberNoteService>();
        var configurationService = serviceProvider.GetRequiredService<ConfigurationService>();

        var guild = context.Guild!;
        if (!configurationService.TryGetGuildConfiguration(guild, out var guildConfiguration))
        {
            return ArraySegment<DiscordAutoCompleteChoice>.Empty;
        }

        var notes = context.Member!.GetPermissionLevel(guildConfiguration) < PermissionLevel.Moderator
            ? noteService.GetNotesAsync(guild, MemberNoteType.Guru)
            : noteService.GetNotesAsync(guild);

        var choices = new List<DiscordAutoCompleteChoice>();

        await foreach (var note in notes)
        {
            if (choices.Count == 10)
            {
                break;
            }

            var content = note.Content;
            if (content.Length > 10)
            {
                content = content[..10] + "...";
            }

            var text = $"#{note.Id} (User {note.UserId}) - {content}";
            choices.Add(new DiscordAutoCompleteChoice(text, note.Id));
        }

        return choices;
    }
}
