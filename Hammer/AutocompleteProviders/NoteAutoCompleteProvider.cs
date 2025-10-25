using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for notes.
/// </summary>
internal sealed class NoteAutoCompleteProvider : IAutoCompleteProvider
{
    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        IServiceProvider serviceProvider = context.ServiceProvider;
        var noteService = serviceProvider.GetRequiredService<MemberNoteService>();
        var configurationService = serviceProvider.GetRequiredService<ConfigurationService>();

        if (!configurationService.TryGetGuildConfiguration(context.Guild, out GuildConfiguration? guildConfiguration))
        {
            return ArraySegment<DiscordAutoCompleteChoice>.Empty;
        }

        IAsyncEnumerable<MemberNote> notes = context.Member.GetPermissionLevel(guildConfiguration) < PermissionLevel.Moderator
            ? noteService.GetNotesAsync(context.Guild, MemberNoteType.Guru)
            : noteService.GetNotesAsync(context.Guild);

        var choices = new List<DiscordAutoCompleteChoice>();

        await foreach (MemberNote note in notes)
        {
            if (choices.Count == 10)
            {
                break;
            }

            string content = note.Content;
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
