using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using FluentResults;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using Hammer.Services;
using JetBrains.Annotations;

namespace Hammer.Commands;

/// <summary>
///     Represents a class which implements the <c>Misplaced Ad</c> context menu command.
/// </summary>
internal sealed class AdvertiseWarnCommand
{
    private const string WarningFormat = "Advertising outside of <#{0}>";

    private readonly ConfigurationService _configurationService;
    private readonly MessageDeletionService _deletionService;
    private readonly RuleService _ruleService;
    private readonly WarningService _warningService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AdvertiseWarnCommand" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="deletionService">The message deletion service.</param>
    /// <param name="ruleService">The rule service.</param>
    /// <param name="warningService">The warning service.</param>
    public AdvertiseWarnCommand(ConfigurationService configurationService,
        MessageDeletionService deletionService,
        RuleService ruleService,
        WarningService warningService)
    {
        _configurationService = configurationService;
        _deletionService = deletionService;
        _ruleService = ruleService;
        _warningService = warningService;
    }

    [Command("Misplaced Ad")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    [RequireGuild]
    [UsedImplicitly]
    public async Task AdvertiseWarnAsync(SlashCommandContext context, DiscordMessage message)
    {
        await context.DeferResponseAsync(true);

        var staffMember = context.Member!;
        var builder = new DiscordWebhookBuilder();
        var embed = new DiscordEmbedBuilder();
        var guild = context.Guild!;
        var configurationResult = ValidateConfiguration(guild);

        if (configurationResult is { IsSuccess: false, Errors.Count: > 0 })
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("⚠️ Error issuing warning");
            embed.WithDescription(configurationResult.Errors[0].Message);
            builder.AddEmbed(embed);
            await context.EditResponseAsync(builder);
            return;
        }

        try
        {
            await _deletionService.DeleteMessageAsync(message, staffMember);
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().ToString());
            embed.WithTitle("Deletion failed");
            embed.WithDescription(exception.Message);
            builder.AddEmbed(embed);
            await context.EditResponseAsync(builder);
            return;
        }

        var author = message.Author!;
        var (configuration, rule) = configurationResult.Value;
        var reason = string.Format(WarningFormat, configuration.JamLinksChannel);
        var importantNotes = new List<string>();
        var options = new WarningOptions(author, staffMember, reason, rule);
        var result = await _warningService.WarnAsync(options);

        embed.AddField("Rule Broken", $"#{rule.Id}: {rule.Brief}");

        if (!result.DirectMessageSuccess)
        {
            importantNotes.Add("The warning was successfully issued, but the user could not be DM'd.");
        }

        if (importantNotes.Count > 0)
        {
            embed.AddField("⚠️ Important Notes", string.Join("\n", importantNotes.Select(n => $"• {n}")));
        }

        embed.WithAuthor(author);
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle("Warned user");
        embed.WithDescription(reason);
        embed.WithFooter($"Infraction {result.Infraction.Id} \u2022 User {author.Id}");

        builder.AddEmbed(embed);
        await context.EditResponseAsync(builder);
    }

    private Result<(GuildConfiguration, Rule)> ValidateConfiguration(DiscordGuild guild)
    {
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
        {
            return Result.Fail($"The configuration for guild {guild.Id} could not be found.");
        }

        if (configuration.AdvertiseChannel == 0)
        {
            return Result.Fail("The advertise channel is not configured.");
        }

        if (configuration.AdvertiseRule == 0)
        {
            return Result.Fail("The advertise rule is not configured.");
        }

        var ruleResult = _ruleService.GetRuleById(guild, configuration.AdvertiseRule);
        if (!ruleResult.IsSuccess)
        {
            return Result.Fail($"The advertise rule with ID {configuration.AdvertiseRule} could not be found.");
        }

        return Result.Ok((configuration, ruleResult.Value));
    }
}
