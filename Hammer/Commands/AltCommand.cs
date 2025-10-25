using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Extensions;
using Hammer.Services;

namespace Hammer.Commands;

[Command("alt")]
[Description("Commands for managing alt accounts.")]
internal sealed class AltCommand
{
    private readonly AltAccountService _altAccountService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AltCommand" /> class.
    /// </summary>
    /// <param name="altAccountService">The alt account service.</param>
    public AltCommand(AltAccountService altAccountService)
    {
        _altAccountService = altAccountService;
    }

    [Command("add")]
    [Description("Adds an alt account to a user.")]
    [RequireGuild]
    public async Task AddAltAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user to add an alt account to.")] DiscordUser user,
        [Parameter("alt"), Description("The alt account to add.")] DiscordUser alt)
    {
        await context.DeferResponseAsync();
        _altAccountService.AddAlt(user, alt, context.Member);

        DiscordUser olderAccount = user.CreationTimestamp > alt.CreationTimestamp ? alt : user;

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user.GetUsernameWithDiscriminator(), iconUrl: user.GetAvatarUrl(MediaFormat.Png));
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Alt account registered");
        embed.WithDescription("The following users have been registered as alts of each other.");
        embed.AddField("Main Account", user.Mention + (olderAccount == user ? " (older)" : " (newer)"), true);
        embed.AddField("Alt Account", alt.Mention + (olderAccount == alt ? " (older)" : " (newer)"), true);

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }

    [Command("remove")]
    [Description("Removes an alt account from a user.")]
    [RequireGuild]
    public async Task RemoveAltAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user to remove an alt account from.")] DiscordUser user,
        [Parameter("alt"), Description("The alt account to remove.")] DiscordUser alt)
    {
        await context.DeferResponseAsync();
        _altAccountService.RemoveAlt(user, alt, context.Member);

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user.GetUsernameWithDiscriminator(), iconUrl: user.GetAvatarUrl(MediaFormat.Png));
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle("Alt account unregistered");
        embed.WithDescription("The following users have been registered as alts of each other.");
        embed.AddField("Main Account", user.Mention, true);
        embed.AddField("Alt Account", alt.Mention, true);

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }

    [Command("view")]
    [Description("Views the alt accounts for a user.")]
    [RequireGuild]
    public async Task ViewAltsAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user to add an alt account to.")] DiscordUser user)
    {
        await context.DeferResponseAsync();
        IReadOnlyCollection<ulong> altAccounts = _altAccountService.GetAltsFor(user.Id);

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(user.GetUsernameWithDiscriminator(), iconUrl: user.GetAvatarUrl(MediaFormat.Png));
        embed.WithColor(DiscordColor.Blurple);
        embed.WithTitle("Known alt accounts");

        if (altAccounts.Count > 0)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"The following users are known alt accounts of {user.Mention}:");

            foreach (ulong altAccount in altAccounts)
            {
                builder.AppendLine($"• {MentionUtility.MentionUser(altAccount)} ({altAccount})");
            }

            embed.WithDescription(builder.ToString());
        }
        else
        {
            embed.WithDescription("✅ No alt accounts are known for this user.");
        }

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }
}
