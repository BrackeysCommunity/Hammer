using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Hammer.AutocompleteProviders;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using JetBrains.Annotations;
using X10D.Text;

namespace Hammer.Commands;

/// <summary>
///     Represents a module which implements the <c>kick</c> command.
/// </summary>
internal sealed class KickCommand
{
    private readonly ILogger<KickCommand> _logger;
    private readonly BanService _banService;
    private readonly InfractionCooldownService _cooldownService;
    private readonly InfractionService _infractionService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KickCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="banService">The ban service.</param>
    /// <param name="cooldownService">The cooldown service.</param>
    /// <param name="infractionService">The infraction service.</param>
    /// <param name="ruleService">The rule service.</param>
    public KickCommand(
        ILogger<KickCommand> logger,
        BanService banService,
        InfractionCooldownService cooldownService,
        InfractionService infractionService,
        RuleService ruleService
    )
    {
        _logger = logger;
        _banService = banService;
        _cooldownService = cooldownService;
        _infractionService = infractionService;
        _ruleService = ruleService;
    }

    [Command("kick")]
    [Description("Kicks a member")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task KickAsync(SlashCommandContext context,
        [Parameter("member"), Description("The member to kick.")] DiscordUser user,
        [Parameter("reason"), Description("The reason for the kick.")] string? reason = null,
        [Parameter("rule"), Description("The rule which was broken."), SlashAutoCompleteProvider<RuleAutoCompleteProvider>]
        string? ruleSearch = null,
        [Parameter("clearMessageHistory"), Description("Clear the user's recent messages in text channels.")]
        bool clearMessageHistory = false)
    {
        await context.DeferResponseAsync(true);

        if (_cooldownService.IsCooldownActive(user, context.Member!) &&
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

        var builder = new DiscordEmbedBuilder();
        var message = new DiscordWebhookBuilder();
        var importantNotes = new List<string>();
        DiscordMember member;

        DiscordGuild guild = context.Guild!;
        try
        {
            member = await guild.GetMemberAsync(user.Id);
        }
        catch (NotFoundException)
        {
            builder.WithAuthor(user);
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Not in guild");
            builder.WithDescription($"The user {user.Mention} is not in this guild.");
            message.AddEmbed(builder);
            await context.EditResponseAsync(message);

            _logger.LogInformation("{StaffMember} attempted to kick non-member {User}", context.Member, user);
            return;
        }

        try
        {
            var hasSearch = !string.IsNullOrWhiteSpace(ruleSearch);
            Rule? rule = hasSearch ? _ruleService.SearchForRule(guild, ruleSearch!) : null;
            if (hasSearch && rule is null)
            {
                importantNotes.Add($"The rule search \"{ruleSearch}\" did not match any rules in this guild.");
            }

            InfractionResult result = await _banService.KickAsync(member, context.Member!, reason, rule, clearMessageHistory);

            if (!result.DirectMessageSuccess)
            {
                importantNotes.Add("The kick was successfully issued, but the user could not be DM'd.");
            }

            if (importantNotes.Count > 0)
            {
                builder.AddField("⚠️ Important Notes", string.Join("\n", importantNotes.Select(n => $"• {n}")));
            }

            builder.WithAuthor(member);
            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("Kicked user");
            if (reason is not null)
            {
                builder.WithDescription(reason);
            }

            builder.WithFooter($"Infraction {result.Infraction.Id} \u2022 User {member.Id}");

            reason = reason.WithWhiteSpaceAlternative("None");
            _logger.LogInformation("{StaffMember} kicked {User}. Reason: {Reason}", context.Member, member, reason);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not issue kick to {Member}", member);

            builder.WithColor(DiscordColor.Red);
            builder.WithTitle("⚠️ Error issuing kick");
            builder.WithDescription($"{exception.GetType().Name} was thrown while issuing the kick.");
            builder.WithFooter("See log for further details.");
        }

        message.AddEmbed(builder);
        await context.EditResponseAsync(message);
    }
}
