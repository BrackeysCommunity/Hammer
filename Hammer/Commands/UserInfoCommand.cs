using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Extensions;
using Hammer.Services;
using Humanizer;
using JetBrains.Annotations;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>userinfo</c> command.
/// </summary>
internal sealed class UserInfoCommand
{
    private readonly ConfigurationService _configurationService;
    private readonly AltAccountService _altAccountService;
    private readonly InfractionService _infractionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserInfoCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="altAccountService">The alt account service.</param>
    /// <param name="infractionService">The infraction service.</param>
    public UserInfoCommand(ConfigurationService configurationService, AltAccountService altAccountService,
        InfractionService infractionService)
    {
        _configurationService = configurationService;
        _altAccountService = altAccountService;
        _infractionService = infractionService;
    }

    [Command("userinfo")]
    [Description("Displays information about a user.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task UserInfoAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user whose information to view.")] DiscordUser user)
    {
        DiscordGuild guild = context.Guild!;
        GuildConfiguration? configuration = _configurationService.GetGuildConfiguration(guild);
        if (configuration is null)
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        bool staffRequested = context.Member!.IsStaffMember(configuration);
        DiscordMember? member = await user.GetAsMemberOfAsync(guild);
        DiscordEmbed embed = CreateUserInfoEmbed(user, member, staffRequested, guild);

        await context.RespondAsync(embed);
    }

    [Command("User Information")]
    [SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu)]
    [RequireGuild]
    [UsedImplicitly]
    public async Task UserInfoContextMenuAsync(SlashCommandContext context, DiscordUser user)
    {
        DiscordGuild guild = context.Guild!;
        GuildConfiguration? configuration = _configurationService.GetGuildConfiguration(guild);
        if (configuration is null)
        {
            await context.RespondAsync("This guild is not configured.", true);
            return;
        }

        bool staffRequested = context.Member!.IsStaffMember(configuration);
        var member = await user.GetAsMemberOfAsync(guild);
        DiscordEmbed embed = CreateUserInfoEmbed(user, member, staffRequested, guild);
        await context.RespondAsync(embed, true);
    }

    private DiscordEmbed CreateUserInfoEmbed(DiscordUser user, DiscordMember? member, bool staffRequested, DiscordGuild guild)
    {
        var embed = new DiscordEmbedBuilder();
        GuildConfiguration? configuration = _configurationService.GetGuildConfiguration(guild);

        if (member is null && !staffRequested)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("User not found");
            embed.WithDescription("The specified user is not a member of this guild.");
            return embed;
        }

        embed.WithAuthor(user.GetUsernameWithDiscriminator(), iconUrl: user.GetAvatarUrl(MediaFormat.Png));
        embed.WithColor(member?.Color.PrimaryColor ?? DiscordColor.Gray);
        embed.WithTitle("User Information");
        embed.WithThumbnail(user.AvatarUrl);

        embed.AddField("Username", user.GetUsernameWithDiscriminator(), true);
        embed.AddField("ID", user.Id, true);
        embed.AddFieldIf(!string.IsNullOrWhiteSpace(member?.Nickname), "Nickname", () => member!.Nickname, true);
        embed.AddField("Created", Formatter.Timestamp(user.CreationTimestamp), true);

        embed.AddFieldIf(member is not null, "Joined", () => Formatter.Timestamp(member!.JoinedAt), true);
        embed.AddFieldIf(member is not null && configuration is not null, "Permission Level",
            () => member!.GetPermissionLevel(configuration!), true);

        if (staffRequested)
        {
            IReadOnlyCollection<ulong> altAccounts = _altAccountService.GetAltsFor(user.Id);

            int infractionCount = _infractionService.GetInfractionCount(user, guild);
            int altInfractions = altAccounts.SelectMany(alt => _infractionService.GetInfractions(alt, guild.Id)).Count();
            embed.AddFieldIf(infractionCount > 0, "Infractions", $"{infractionCount} (+ {altInfractions})", true);

            int altCount = altAccounts.Count;
            embed.AddFieldIf(altCount > 0, "Alt Account".ToQuantity(altCount), () =>
            {
                ulong firstAlt = altAccounts.First();
                return altCount switch
                {
                    1 => $"{MentionUtility.MentionUser(firstAlt)} ({firstAlt})",
                    <= 5 => string.Join("\n", altAccounts.Select(id => $"• {MentionUtility.MentionUser(id)} ({id})")),
                    _ => $"Use `/alt view user:{user.Id}` to view."
                };
            });
        }

        if (member is null)
        {
            embed.WithFooter("⚠️ This user is not currently in the server.");
        }

        return embed;
    }
}
