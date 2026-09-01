using DSharpPlus.Entities;
using Hammer.Data;
using Hammer.Extensions;
using X10D.Text;

namespace Hammer.Services;

/// <inheritdoc />
internal sealed class WarningService : IWarningService
{
    private readonly InfractionService _infractionService;
    private readonly DiscordLogService _logService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WarningService" /> class.
    /// </summary>
    public WarningService(DiscordLogService logService, InfractionService infractionService, RuleService ruleService)
    {
        _logService = logService;
        _infractionService = infractionService;
        _ruleService = ruleService;
    }

    /// <inheritdoc />
    public async ValueTask<InfractionResult> WarnAsync(WarningOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var opt = new InfractionOptions
        {
            NotifyUser = true,
            Reason = options.Reason.AsNullIfWhiteSpace(),
            RuleBroken = options.RuleBroken,
            AdditionalInformation = options.AdditionalInfo.AsNullIfWhiteSpace()
        };

        var result = await _infractionService.CreateInfractionAsync(InfractionType.Warning, options.User, options.Issuer, opt);
        var infractionCount = _infractionService.GetInfractionCount(options.User, options.Issuer.Guild);

        Rule? rule = null;
        if (result.Infraction.RuleId is { } ruleId && _ruleService.GuildHasRule(result.Infraction.GuildId, ruleId))
        {
            var ruleResult = _ruleService.GetRuleById(result.Infraction.GuildId, ruleId);
            rule = ruleResult.IsSuccess ? ruleResult.Value : null;
        }

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.Orange);
        embed.WithAuthor(options.User);
        embed.WithTitle("User warned");
        embed.AddField("User", options.User.Mention, true);
        embed.AddField("User ID", options.User.Id, true);
        embed.AddField("Staff Member", options.Issuer.Mention, true);
        embed.AddFieldIf(infractionCount > 0, "Total User Infractions", infractionCount, true);
        embed.AddFieldIf(rule is not null, "Rule Broken", () => $"{rule!.Id} - {rule.Brief ?? rule.Description}", true);
        embed.AddFieldIf(!string.IsNullOrWhiteSpace(opt.Reason), "Reason", opt.Reason);
        embed.AddFieldIf(!string.IsNullOrWhiteSpace(opt.AdditionalInformation), "Additional Information",
            opt.AdditionalInformation);
        embed.WithFooter($"Infraction {result.Infraction.Id}");

        await _logService.LogAsync(options.Issuer.Guild, embed);
        return result;
    }
}
