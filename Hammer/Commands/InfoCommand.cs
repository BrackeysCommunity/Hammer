using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Hammer.Configuration;
using Hammer.Extensions;
using Hammer.Services;
using Humanizer;
using Humanizer.Localisation;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>info</c> command.
/// </summary>
internal sealed class InfoCommand
{
    private readonly BotService _botService;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoCommand" /> class.
    /// </summary>
    /// <param name="botService">The bot service.</param>
    /// <param name="configurationService">The configuration service.</param>
    public InfoCommand(BotService botService, ConfigurationService configurationService)
    {
        _botService = botService;
        _configurationService = configurationService;
    }

    [Command("info")]
    [Description("Displays information about the bot.")]
    [RequireGuild]
    public async Task InfoAsync(SlashCommandContext context)
    {
        DiscordGuild guild = context.Guild;
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
        {
            configuration = new GuildConfiguration();
        }

        DiscordClient client = context.Client;
        DiscordMember member = (await client.CurrentUser.GetAsMemberOfAsync(guild))!;
        string hammerVersion = _botService.Version;
        DiscordColor embedColor = member.Color;
        if (embedColor.Value == 0)
        {
            embedColor = configuration.PrimaryColor;
        }
        
        TimeSpan latency = client.GetConnectionLatency(guild.Id);
        string ping = latency.Humanize(minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Second);

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(member);
        embed.WithColor(embedColor);
        embed.WithThumbnail(member.AvatarUrl);
        embed.WithTitle($"Hammer v{hammerVersion}");
        embed.AddField("Ping", $"{ping}", true);
        embed.AddField("Uptime", (DateTimeOffset.UtcNow - _botService.StartedAt).Humanize(), true);
        embed.AddField("Source", "[View on GitHub](https://github.com/BrackeysCommunity/Hammer)", true);

        var builder = new StringBuilder();
        builder.AppendLine($"Hammer: {hammerVersion}");
        builder.AppendLine($"D#+: {client.VersionString}");
        builder.AppendLine($"CLR: {Environment.Version.ToString(3)}");
        builder.AppendLine($"Host: {Environment.OSVersion}");

        embed.AddField("Version", Formatter.BlockCode(builder.ToString()));

        await context.RespondAsync(embed, true);
    }
}
