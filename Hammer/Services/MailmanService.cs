using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.Data;
using Hammer.Extensions;
using Humanizer;
using SmartFormat;
using X10D.Text;

namespace Hammer.Services;

/// <summary>
///     Represents a service which handles sending direct messages to members for a variety of purposes.
/// </summary>
public sealed class MailmanService
{
    private readonly DiscordClient _discordClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MailmanService" /> class.
    /// </summary>
    public MailmanService(DiscordClient discordClient)
    {
        _discordClient = discordClient;
    }

    /// <summary>
    ///     Sends an infraction notice to the applicable member, if possible.
    /// </summary>
    /// <param name="infraction">The infraction to notify.</param>
    /// <param name="infractionCount">The infraction count to display on the embed.</param>
    /// <param name="options">The infraction options.</param>
    /// <returns>The message which was sent to the member, or <see langword="null" /> if the message could not be sent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="infraction" /> is <see langword="null" />.</exception>
    public async Task<DiscordMessage?> SendInfractionAsync(Infraction infraction, int infractionCount, InfractionOptions options)
    {
        if (infraction is null)
        {
            throw new ArgumentNullException(nameof(infraction));
        }

        if (!_discordClient.Guilds.TryGetValue(infraction.GuildId, out var guild))
        {
            return null;
        }

        var member = await guild.GetMemberOrNullAsync(infraction.UserId);
        if (member is null)
        {
            return null; // bots can only DM members
        }

        try
        {
            var embed = CreatePrivateEmbed(infraction, infractionCount, options, member);
            if (embed is not null)
            {
                return await member.SendMessageAsync(embed);
            }

            // user does not exist, or guild is invalid
            return null;
        }
        catch (UnauthorizedException)
        {
            // bot is blocked or DMs disabled
            return null;
        }
    }

    private DiscordEmbed? CreatePrivateEmbed(Infraction infraction, int count, InfractionOptions options, DiscordMember? member)
    {
        if (member is null)
        {
            return null;
        }

        if (!_discordClient.Guilds.TryGetValue(infraction.GuildId, out var guild))
        {
            return null;
        }

        var description = infraction.Type.GetEmbedMessage();
        var reason = infraction.Reason.WithWhiteSpaceAlternative(Formatter.Italic("No reason given."));
        var embed = new DiscordEmbedBuilder();
        var iconUrl = guild.GetIconUrl(MediaFormat.Png) ?? guild.IconUrl;

        embed.WithColor(0xFF0000);
        embed.WithTitle(infraction.Type.Humanize());
        if (!string.IsNullOrWhiteSpace(description))
        {
            embed.WithDescription(description.FormatSmart(new { user = member, guild }));
        }

        embed.WithThumbnail(iconUrl);
        embed.WithFooter(guild.Name, iconUrl);
        embed.AddField("Reason", reason);
        embed.AddFieldIf(infraction.RuleId.HasValue, "Rule Broken", () => $"{infraction.RuleId} - {infraction.RuleText}", true);

        switch (infraction.Type)
        {
            case InfractionType.Warning:
                embed.AddField("Punishment", "**WARNING**", true);
                break;
            case InfractionType.Kick:
                embed.AddField("Punishment", "**KICK**", true);
                break;
            case InfractionType.Mute or InfractionType.TemporaryMute:
                embed.AddField("Punishment", $"**MUTE**\n{options.ReadableDuration}", true);
                break;
            case InfractionType.Ban or InfractionType.TemporaryBan:
                embed.AddField("Punishment", $"**BAN**\n{options.ReadableDuration}", true);
                break;
        }

        embed.AddField("Total Infractions", count, true);

        if (infraction.Type is not InfractionType.Ban and not InfractionType.TemporaryBan)
        {
            embed.AddModMailNotice();
        }

        return embed;
    }
}
