using System.ComponentModel;
using DSharpPlus.Commands;
using Hammer.Services;

namespace Hammer.Commands.Infractions;

/// <summary>
///     Represents a module which implements infraction commands.
/// </summary>
[Command("infraction")]
[Description("Manage infractions.")]
internal sealed partial class InfractionCommand
{
    private readonly ConfigurationService _configurationService;
    private readonly InfractionService _infractionService;
    private readonly InfractionStatisticsService _infractionStatisticsService;
    private readonly DiscordLogService _logService;
    private readonly RuleService _ruleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfractionCommand" /> class.
    /// </summary>
    public InfractionCommand(
        ConfigurationService configurationService,
        DiscordLogService logService,
        InfractionService infractionService,
        InfractionStatisticsService infractionStatisticsService,
        RuleService ruleService
    )
    {
        _configurationService = configurationService;
        _infractionService = infractionService;
        _infractionStatisticsService = infractionStatisticsService;
        _logService = logService;
        _ruleService = ruleService;
    }
}
