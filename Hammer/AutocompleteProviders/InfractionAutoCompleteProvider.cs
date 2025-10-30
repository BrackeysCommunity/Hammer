using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for infractions.
/// </summary>
internal sealed class InfractionAutoCompleteProvider : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var infractionService = context.ServiceProvider.GetRequiredService<InfractionService>();
        IEnumerable<Infraction> infractions = infractionService.EnumerateInfractions(context.Guild!);

        return ValueTask.FromResult(infractions.OrderByDescending(i => i.IssuedAt).Take(10).Select(infraction =>
        {
            string summary = GetInfractionSummary(context.Client, infraction);
            return new DiscordAutoCompleteChoice(summary, infraction.Id);
        }));
    }

    private static string GetInfractionSummary(DiscordClient client, Infraction infraction)
    {
        var userString = $"User {infraction.UserId}";
        try
        {
            DiscordUser user = client.GetUserAsync(infraction.UserId).GetAwaiter().GetResult();
            userString = user.GetUsernameWithDiscriminator();
        }
        catch (NotFoundException)
        {
            // ignored
        }

        return $"#{infraction.Id} - {infraction.Reason} ({userString})";
    }
}
