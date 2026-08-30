using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Hammer.AutocompleteProviders;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using Humanizer;
using JetBrains.Annotations;
using X10D.Text;
using X10D.Time;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>mute</c> command.
/// </summary>
internal sealed class MuteCommand
{
    private readonly ILogger<MuteCommand> _logger;
    private readonly ConfigurationService _configurationService;
    private readonly InfractionCooldownService _cooldownService;
    private readonly InfractionService _infractionService;
    private readonly MuteService _muteService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MuteCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="cooldownService">The cooldown service.</param>
    /// <param name="infractionService">The infraction service.</param>
    /// <param name="muteService">The mute service.</param>
    /// <param name="ruleService">The rule service.</param>
    public MuteCommand(
        ILogger<MuteCommand> logger,
        ConfigurationService configurationService,
        InfractionCooldownService cooldownService,
        InfractionService infractionService,
        MuteService muteService,
        RuleService ruleService
    )
    {
        _logger = logger;
        _configurationService = configurationService;
        _cooldownService = cooldownService;
        _infractionService = infractionService;
        _muteService = muteService;
        _ruleService = ruleService;
    }

    [Command("mute")]
    [Description("Temporarily or permanently mutes a user.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task MuteAsync(SlashCommandContext context,
        [Parameter("user"), Description("The user to mute.")] DiscordUser user,
        [Parameter("reason"), Description("The reason for the mute.")] string? reason = null,
        [Parameter("duration"), Description("The duration of the mute.")] string? durationRaw = null,
        [Parameter("rule"), Description("The rule which was broken."), SlashAutoCompleteProvider<RuleAutoCompleteProvider>]
        string? ruleSearch = null)
    {
        await context.DeferResponseAsync(true);

        DiscordMember member = context.Member!;
        if (_cooldownService.IsCooldownActive(user, member) &&
            _cooldownService.TryGetInfraction(user, out Infraction? infraction))
        {
            _logger.LogInformation("{User} is on cooldown. Prompting for confirmation", user);
            DiscordEmbed embed = await _infractionService.CreateInfractionEmbedAsync(infraction);
            bool result = await InfractionCooldownService.ShowConfirmationAsync(context, user, infraction, embed);
            if (!result)
            {
                return;
            }
        }

        DiscordGuild guild = context.Guild!;
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            DiscordWebhookBuilder responseBuilder = new DiscordWebhookBuilder().WithContent("This guild is not configured.");
            await context.EditResponseAsync(responseBuilder);
            return;
        }

        TimeSpan? duration = null;
        if (!string.IsNullOrWhiteSpace(durationRaw))
        {
            if (TimeSpanParser.TryParse(durationRaw, out TimeSpan timeSpan))
            {
                duration = timeSpan;
            }
            else
            {
                var responseBuilder = new DiscordWebhookBuilder();
                var embed = new DiscordEmbedBuilder();
                embed.WithColor(DiscordColor.Red);
                embed.WithTitle("⚠️ Error parsing duration");
                embed.WithDescription($"The duration `{durationRaw}` is not a valid duration. " +
                                      "Accepted format is `#y #mo #w #d #h #m #s #ms`");
                await context.EditResponseAsync(responseBuilder.AddEmbed(embed));
                return;
            }
        }

        var message = new DiscordWebhookBuilder();
        var importantNotes = new List<string>();

        var hasSearch = !string.IsNullOrWhiteSpace(ruleSearch);
        Rule? rule = hasSearch ? _ruleService.SearchForRule(guild, ruleSearch!) : null;
        if (hasSearch && rule is null)
        {
            importantNotes.Add($"The rule search \"{ruleSearch}\" did not match any rules in this guild.");
        }

        ValueTask<InfractionResult> infractionTask;
        PermissionLevel permissionLevel = member.GetPermissionLevel(guildConfiguration);
        var shouldClampDuration = false;

        if (guildConfiguration.Mute.MaxModeratorMuteDuration is { } maxModeratorMuteDuration and > 0)
        {
            shouldClampDuration = permissionLevel == PermissionLevel.Moderator;
        }
        else
            // pattern match does not initialize to 0 on failure. explicit = 0 is required here, else the compiler complains
        {
            maxModeratorMuteDuration = 0;
        }

        if (duration is null)
        {
            if (shouldClampDuration)
            {
                duration = TimeSpan.FromMilliseconds(maxModeratorMuteDuration);
                infractionTask = _muteService.TemporaryMuteAsync(user, member, reason, duration.Value, rule);
            }
            else
            {
                infractionTask = _muteService.MuteAsync(user, member, reason, rule);
            }
        }
        else
        {
            if (shouldClampDuration && duration.Value.TotalMilliseconds > maxModeratorMuteDuration)
            {
                duration = TimeSpan.FromMilliseconds(maxModeratorMuteDuration);
            }

            infractionTask = _muteService.TemporaryMuteAsync(user, member, reason, duration.Value, rule);
        }

        var builder = new DiscordEmbedBuilder();

        try
        {
            InfractionResult result = await infractionTask;

            if (!result.DirectMessageSuccess)
            {
                importantNotes.Add("The mute was successfully issued, but the user could not be DM'd.");
            }

            builder.WithAuthor(user);
            builder.WithColor(DiscordColor.Red);
            if (!string.IsNullOrWhiteSpace(reason))
            {
                builder.WithDescription(reason);
            }

            builder.WithFooter($"Infraction {result.Infraction.Id} \u2022 User {user.Id}");
            reason = reason.WithWhiteSpaceAlternative("None");

            if (result.Infraction.Type == InfractionType.Mute)
            {
                builder.WithTitle("Muted user");
                _logger.LogInformation("{StaffMember} muted {User}. Reason: {Reason}", member, user, reason);
            }
            else if (result.Infraction.Type == InfractionType.TemporaryMute)
            {
                builder.WithTitle("Temporarily muted user");
                builder.AddField("Duration", duration!.Value.Humanize());
                _logger.LogInformation("{StaffMember} temporarily muted {User} for {Duration}. Reason: {Reason}",
                    member, user, duration.Value.Humanize(), reason);
            }

            if (importantNotes.Count > 0)
            {
                builder.AddField("⚠️ Important Notes", string.Join("\n", importantNotes.Select(n => $"• {n}")));
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not issue mute to {User}", user);

            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing mute");
            builder.WithDescription($"{exception.GetType().Name} was thrown while issuing the mute.");
            builder.WithFooter("See log for further details.");
        }

        message.AddEmbed(builder);
        await context.EditResponseAsync(message);
    }
}
