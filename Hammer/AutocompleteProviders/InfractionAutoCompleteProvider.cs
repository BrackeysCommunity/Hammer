using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;

namespace Hammer.AutocompleteProviders;

/// <summary>
///     Provides autocomplete suggestions for infractions.
/// </summary>
internal sealed class InfractionAutoCompleteProvider : IAutoCompleteProvider
{
    /// <inheritdoc />
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
    {
        var infractionService = context.Services.GetRequiredService<InfractionService>();
        IEnumerable<Infraction> infractions = infractionService.EnumerateInfractions(context.Guild);

        return Task.FromResult(infractions.OrderByDescending(i => i.IssuedAt).Take(10).Select(infraction =>
        {
            string summary = GetInfractionSummary(context.Client, infraction);
            return new DiscordAutoCompleteChoice(summary, infraction.Id);
        }));
    }

    private static string GetInfractionSummary(DiscordClient client, Infraction infraction)
    {
        string userString = $"User {infraction.UserId}";
        try
        {
            DiscordUser? user = client.GetUserAsync(infraction.UserId).GetAwaiter().GetResult();
            userString = user.GetUsernameWithDiscriminator();
        }
        catch (NotFoundException)
        {
            // ignored
        }

        return $"#{infraction.Id} - {infraction.Reason} ({userString})";
    }
}
