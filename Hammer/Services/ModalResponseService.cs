using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Extensions;
using X10D.Text;

namespace Hammer.Services;

/// <summary>
///     Represents a service which handles modal responses.
/// </summary>
internal sealed class ModalResponseService : IEventHandler<ModalSubmittedEventArgs>
{
    private readonly Dictionary<string, Func<ModalSubmittedEventArgs, Task>> _modalHandlers = [];
    private readonly ILogger<ModalResponseService> _logger;
    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;
    private readonly MessageDeletionService _messageDeletionService;
    private readonly MessageService _messageService;
    private readonly RuleService _ruleService;
    private readonly WarningService _warningService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModalResponseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="messageDeletionService">The message deletion service.</param>
    /// <param name="messageService">The message service.</param>
    /// <param name="ruleService">The rule service.</param>
    /// <param name="warningService">The warning service.</param>
    public ModalResponseService(ILogger<ModalResponseService> logger,
        DiscordClient discordClient,
        ConfigurationService configurationService,
        MessageDeletionService messageDeletionService,
        MessageService messageService,
        RuleService ruleService,
        WarningService warningService)
    {
        _logger = logger;
        _discordClient = discordClient;
        _configurationService = configurationService;
        _messageDeletionService = messageDeletionService;
        _messageService = messageService;
        _ruleService = ruleService;
        _warningService = warningService;

        _modalHandlers[CustomIds.BadMessageWarning] = BadMessageAsync;
        _modalHandlers[CustomIds.MessageMember] = MessageAsync;
        _modalHandlers[CustomIds.AddRule] = AddRuleAsync;
        _modalHandlers[CustomIds.EditRule] = EditRuleAsync;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, ModalSubmittedEventArgs e)
    {
        foreach ((string prefix, Func<ModalSubmittedEventArgs, Task> handler) in _modalHandlers)
        {
            if (!e.Id.StartsWith(prefix))
            {
                continue;
            }

            await handler(e);
            return;
        }
    }

    private async Task AddRuleAsync(ModalSubmittedEventArgs e)
    {
        DiscordGuild guild = e.Interaction.Guild!;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            var builder = new DiscordInteractionResponseBuilder();
            builder.AsEphemeral();
            builder.WithContent("This guild is not configured.");
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, builder);
            return;
        }

        string? brief = (e.Values["brief"] as TextInputModalSubmission)?.Value;
        string description = (e.Values["description"] as TextInputModalSubmission)?.Value ?? string.Empty;

        Rule rule = _ruleService.AddRule(guild, description, brief);

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(guildConfiguration, false);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle($"Rule #{rule.Id} added");

        if (string.IsNullOrWhiteSpace(brief))
        {
            embed.WithDescription(rule.Description);
        }
        else
        {
            embed.AddField(rule.Brief!, rule.Description);
        }

        await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }

    private async Task EditRuleAsync(ModalSubmittedEventArgs e)
    {
        DiscordGuild guild = e.Interaction.Guild!;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
        {
            var builder = new DiscordInteractionResponseBuilder();
            builder.AsEphemeral();
            builder.WithContent("This guild is not configured.");
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, builder);
            return;
        }

        var ruleId = 0;

        if (!CustomIdBuilder.TryParse(e.Id, out string? _, out IReadOnlyDictionary<string, string>? parameters) ||
            !parameters.TryGetValue("rule", out string? ruleIdString) ||
            !int.TryParse(ruleIdString, out ruleId) ||
            !_ruleService.GuildHasRule(guild, ruleId))
        {
            DiscordEmbed responseEmbed = RuleService.CreateRuleNotFoundEmbed(ruleId);
            var response = new DiscordFollowupMessageBuilder();
            response.AsEphemeral();
            response.AddEmbed(responseEmbed);
            await e.Interaction.CreateFollowupMessageAsync(response);
            return;
        }

        var ruleResult = _ruleService.GetRuleById(guild, ruleId);
        Rule? rule = ruleResult.IsSuccess ? ruleResult.Value : null;
        string? oldBrief = rule?.Brief?.AsNullIfWhiteSpace();
        string oldDescription = rule?.Description.AsNullIfWhiteSpace() ?? string.Empty;

        string? brief = (e.Values["brief"] as TextInputModalSubmission)?.Value;
        string? description = (e.Values["description"] as TextInputModalSubmission)?.Value;

        string? newBrief = brief?.AsNullIfWhiteSpace();
        string? newDescription = description.AsNullIfWhiteSpace();
        var changed = false;

        if (!string.Equals(oldBrief, newBrief) && (changed = true))
        {
            _ruleService.SetRuleBrief(rule, newBrief);
        }

        if (!string.Equals(oldDescription, newDescription) && (changed = true))
        {
            _ruleService.SetRuleContent(rule, newDescription!);
        }

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(guildConfiguration, false);

        if (changed)
        {
            embed.WithColor(DiscordColor.Green);
            embed.WithTitle($"Rule #{rule.Id} updated");
        }
        else
        {
            embed.WithColor(DiscordColor.Orange);
            embed.WithTitle($"Rule #{rule.Id} unchanged");
            embed.WithDescription("No changes were made to the rule.");
        }

        if (string.IsNullOrWhiteSpace(brief))
        {
            embed.WithDescription(rule.Description);
        }
        else
        {
            embed.AddField(rule.Brief!, rule.Description);
        }

        var webhook = new DiscordWebhookBuilder();
        webhook.AddEmbed(embed);
        await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
    }

    private async Task BadMessageAsync(ModalSubmittedEventArgs e)
    {
        DiscordGuild guild = e.Interaction.Guild!;

        if (e.Values["rule-id"] is not TextInputModalSubmission ruleInput)
        {
            throw new InvalidOperationException("Rule ID input is missing.");
        }

        if (e.Values["reason"] is not TextInputModalSubmission reasonInput)
        {
            throw new InvalidOperationException("Reason input is missing.");
        }

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? configuration))
        {
            configuration = new GuildConfiguration();
        }

        string defaultReason = configuration.DefaultBadMessageWarning;

        var importantNotes = new List<string>();
        DiscordMember staffMember = (await e.Interaction.User.GetAsMemberOfAsync(guild))!;
        DiscordMessage message = e.Interaction.Message!;
        DiscordUser user = message.Author!;

        if (!TryGetRule(guild, ruleInput.Value, out Rule? rule))
        {
            importantNotes.Add("The specified rule does not exist - it will be omitted from the infraction.");
        }

        string reason = MentionUtility.ReplaceChannelMentions(guild, reasonInput.Value.WithWhiteSpaceAlternative(defaultReason));
        await _messageDeletionService.DeleteMessageAsync(message, staffMember);

        DiscordChannel channel = message.Channel!;
        var additionalInfo = $"Message {message.Id} in {channel.Mention} (#{channel.Name})";
        var options = new WarningOptions(user, staffMember, reason, rule, additionalInfo);
        InfractionResult result = await _warningService.WarnAsync(options);

        if (!result.DirectMessageSuccess)
        {
            importantNotes.Add("The warning was successfully issued, but the user could not be DM'd.");
        }

        var builder = new DiscordEmbedBuilder();

        if (importantNotes.Count > 0)
        {
            builder.AddField("⚠️ Important Notes", string.Join("\n", importantNotes.Select(n => $"• {n}")));
        }

        builder.WithAuthor(user);
        builder.WithColor(DiscordColor.Orange);
        builder.WithTitle("Warned user");
        builder.WithDescription(reason);
        builder.WithFooter($"Infraction {result.Infraction.Id} \u2022 User {user.Id}");

        _logger.LogInformation("{StaffMember} warned {User}. Reason: {Reason}", staffMember, user, reason);

        var response = new DiscordFollowupMessageBuilder();
        response.AsEphemeral();
        response.AddEmbed(builder);
        await e.Interaction.CreateFollowupMessageAsync(response);
    }

    private async Task MessageAsync(ModalSubmittedEventArgs e)
    {
        DiscordGuild guild = e.Interaction.Guild!;
        if (!CustomIdBuilder.TryParse(e.Id, out string? _, out IReadOnlyDictionary<string, string>? parameters))
        {
            return;
        }

        ulong userId = ulong.Parse(parameters["user"]);
        DiscordUser user = await _discordClient.GetUserAsync(userId);
        DiscordMember member = (await user.GetAsMemberOfAsync(guild))!;

        if (e.Values["message"] is not TextInputModalSubmission message)
        {
            throw new InvalidOperationException("Message input is missing.");
        }

        string content = MentionUtility.ReplaceChannelMentions(guild, message.Value.Trim());
        var builder = new DiscordFollowupMessageBuilder();
        builder.AsEphemeral();

        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(content))
        {
            embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(user);
            embed.WithTitle("Message not sent");
            embed.WithDescription($"An empty message cannot be sent to {user.Mention}");
            await e.Interaction.CreateFollowupMessageAsync(builder.AddEmbed(embed));
            return;
        }

        DiscordMember staffMember = (await e.Interaction.User.GetAsMemberOfAsync(guild))!;
        bool success = await _messageService.MessageMemberAsync(member, staffMember, content);

        if (success)
        {
            embed.WithColor(DiscordColor.Green);
            embed.WithAuthor(user);
            embed.WithTitle("Message Sent");
            embed.AddField("Content", content);
            await e.Interaction.CreateFollowupMessageAsync(builder.AddEmbed(embed));
        }
        else
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(user);
            embed.WithTitle("Failed to send message");
            embed.WithDescription($"The message could not be sent to {user.Mention}. " +
                                  "This is likely due to DMs being disabled for this user.");
            embed.AddField("Content", content);
            await e.Interaction.CreateFollowupMessageAsync(builder.AddEmbed(embed));
        }

        embed.AddField("Content", content);
        await e.Interaction.CreateFollowupMessageAsync(builder.AddEmbed(embed));
    }

    private bool TryGetRule(DiscordGuild guild, string? query, out Rule? rule)
    {
        rule = null;
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        if (int.TryParse(query, out int ruleId))
        {
            if (_ruleService.GuildHasRule(guild, ruleId))
            {
                var ruleResult = _ruleService.GetRuleById(guild, ruleId);
                if (ruleResult.IsSuccess)
                {
                    rule = ruleResult.Value;
                    return true;
                }
            }
        }
        else
        {
            rule = _ruleService.SearchForRule(guild, query);
            if (rule is not null)
            {
                return true;
            }
        }

        return false;
    }
}
