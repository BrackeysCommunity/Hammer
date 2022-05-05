using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrackeysBot.API;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.API.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Hammer.API;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Data.Infractions;
using Hammer.Resources;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using SmartFormat;
using X10D.Text;
using TimestampFormat = DSharpPlus.TimestampFormat;
using CoreGuildConfiguration = BrackeysBot.Core.API.Configuration.GuildConfiguration;

namespace Hammer.Services;

/// <summary>
///     Represents a service which handles and manipulates infractions.
/// </summary>
/// <seealso cref="BanService" />
/// <seealso cref="MuteService" />
internal sealed class InfractionService : BackgroundService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<ulong, List<Infraction>> _infractionCache = new();
    private readonly ConfigurationService _configurationService;
    private readonly MailmanService _mailmanService;
    private readonly RuleService _ruleService;
    private readonly ICorePlugin _corePlugin;
    private readonly DiscordClient _discordClient;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfractionService" /> class.
    /// </summary>
    public InfractionService(IServiceScopeFactory scopeFactory, ICorePlugin corePlugin, DiscordClient discordClient,
        ConfigurationService configurationService, MailmanService mailmanService, RuleService ruleService)
    {
        _scopeFactory = scopeFactory;
        _corePlugin = corePlugin;
        _discordClient = discordClient;
        _configurationService = configurationService;
        _mailmanService = mailmanService;
        _ruleService = ruleService;
    }

    /// <summary>
    ///     Adds an infraction to the database.
    /// </summary>
    /// <param name="infraction">The infraction to add.</param>
    /// <param name="guild">
    ///     The guild to which this infraction belongs. If <see langword="null" /> is passed, this method will attempt to retrieve
    ///     the guild specified by the infraction from the client.
    /// </param>
    /// <returns>The infraction entity.</returns>
    /// <remarks>
    ///     Do NOT use this method to issue infractions to users. Use an appropriate user-targeted method from another service.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The infraction belongs to a guild that this client cannot access.</exception>
    /// <seealso cref="CreateInfractionAsync" />
    public async Task<Infraction> AddInfractionAsync(Infraction infraction, DiscordGuild? guild = null)
    {
        if (guild is not null) guild = await guild.NormalizeClientAsync(_discordClient).ConfigureAwait(false);
        try
        {
            guild ??= await _discordClient.GetGuildAsync(infraction.GuildId).ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            throw new InvalidOperationException(ExceptionMessages.InvalidGuild);
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        infraction = (await context.AddAsync(infraction).ConfigureAwait(false)).Entity;
        await context.SaveChangesAsync();

        if (!_infractionCache.TryGetValue(guild.Id, out List<Infraction>? infractions))
        {
            infractions = new List<Infraction>();
            _infractionCache.Add(guild.Id, infractions);
        }

        infractions.Add(infraction);
        return infraction;
    }

    /// <summary>
    ///     Adds infractions to the database.
    /// </summary>
    /// <param name="infractions">The infractions to add.</param>
    /// <remarks>Do NOT use this method to issue infractions to users. Use an appropriate user-targeted method.</remarks>
    public async Task AddInfractionsAsync(IEnumerable<Infraction> infractions)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        await context.AddRangeAsync(infractions).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     <para>Creates an infraction from the specified options, notifying the target user if:</para>
    ///     <para>1) the user is currently in the same guild as the staff member issuing the infraction</para>
    ///     -and-
    ///     <para>2) the infraction type is not <see cref="InfractionType.Gag" />.</para>
    ///     -and-
    ///     <para>
    ///         3) <see cref="InfractionOptions.NotifyUser" /> as defined in <paramref name="options" /> is
    ///         <see langword="true" />.
    ///     </para>
    /// </summary>
    /// <param name="type">The infraction type.</param>
    /// <param name="user">The user receiving this infraction.</param>
    /// <param name="staffMember">The staff member issuing the infraction.</param>
    /// <param name="options">
    ///     An instance of <see cref="InfractionOptions" /> containing additional information regarding the infraction.
    /// </param>
    /// <returns>The newly-created infraction.</returns>
    public async Task<Infraction> CreateInfractionAsync(InfractionType type, DiscordUser user, DiscordMember staffMember,
        InfractionOptions options)
    {
        user = await user.NormalizeClientAsync(_discordClient).ConfigureAwait(false);
        staffMember = await staffMember.NormalizeClientAsync(_discordClient).ConfigureAwait(false);

        string? reason = options.Reason.AsNullIfWhiteSpace();

        DiscordGuild guild = staffMember.Guild;
        DateTimeOffset? expirationTime = options.ExpirationTime;

        if (type == InfractionType.Gag)
        {
            GuildConfiguration guildConfiguration = _configurationService.GetGuildConfiguration(guild);
            expirationTime = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(guildConfiguration.MuteConfiguration.GagDuration);
        }

        var builder = new InfractionBuilder();
        builder.WithType(type);
        builder.WithTargetUser(user).WithGuild(guild);
        builder.WithReason(reason).WithStaffMember(staffMember);
        builder.WithRule(options.RuleBroken);

        Infraction infraction = await AddInfractionAsync(builder.Build(), guild).ConfigureAwait(false);

        var logMessageBuilder = new StringBuilder();
        logMessageBuilder.Append($"{type.ToString("G")} issued to {user} by {staffMember} in {guild}. ");
        logMessageBuilder.Append($"Reason: {reason ?? "<none>"}. ");
        logMessageBuilder.Append($"Rule broken: {options.RuleBroken?.Id.ToString() ?? "<none>"}. ");
        logMessageBuilder.Append($"Expires: {expirationTime?.ToString() ?? "never"}");
        Logger.Info(logMessageBuilder);

        if (type != InfractionType.Gag && options.NotifyUser)
            await _mailmanService.SendInfractionAsync(infraction).ConfigureAwait(false);

        return infraction;
    }

    /// <summary>
    ///     Creates an infraction embed to send to the staff log channel.
    /// </summary>
    /// <param name="infraction">The infraction to log.</param>
    public async Task<DiscordEmbed> CreateInfractionEmbedAsync(Infraction infraction)
    {
        DiscordUser? user;
        try
        {
            user = await _discordClient.GetUserAsync(infraction.UserId).ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            user = null;
        }

        int infractionCount = GetInfractionCount(infraction.UserId, infraction.GuildId);

        string reason = string.IsNullOrWhiteSpace(infraction.Reason)
            ? Formatter.Italic("<none>")
            : infraction.Reason;

        var embedBuilder = new DiscordEmbedBuilder();
        embedBuilder.WithColor(0xFF0000);

        if (user is null) embedBuilder.WithAuthor($"User {infraction.UserId}");
        else embedBuilder.WithAuthor(user);

        Rule? rule = null;
        if (infraction.RuleId is { } ruleId)
            rule = _ruleService.GetRuleById(infraction.GuildId, ruleId);

        embedBuilder.WithTitle(infraction.Type.Humanize());
        embedBuilder.AddField(EmbedFieldNames.InfractionID, infraction.Id, true);
        embedBuilder.AddField(EmbedFieldNames.User, MentionUtility.MentionUser(infraction.UserId), true);
        embedBuilder.AddField(EmbedFieldNames.UserID, infraction.UserId.ToString(), true);
        embedBuilder.AddField(EmbedFieldNames.StaffMember, MentionUtility.MentionUser(infraction.StaffMemberId), true);
        embedBuilder.AddField(EmbedFieldNames.TotalUserInfractions, infractionCount, true);
        embedBuilder.AddField(EmbedFieldNames.RuleBroken, reason);
        embedBuilder.AddField(EmbedFieldNames.Reason, reason);

        return embedBuilder.Build();
    }

    public DiscordEmbedBuilder BuildInfractionHistoryEmbed(DiscordUser user, DiscordGuild guild,
        bool staffRequested, int page = 0)
    {
        const int infractionsPerPage = 10;

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed();
        IReadOnlyList<Infraction> infractions = GetInfractions(user, guild);

        if (_corePlugin.TryGetGuildConfiguration(guild, out CoreGuildConfiguration? guildConfiguration))
            embed.WithColor(guildConfiguration.PrimaryColor);
        else
            embed.WithColor(DiscordColor.Orange);

        embed.WithAuthor(user);

        string underlinedFieldName = Formatter.Underline("Infraction Record");
        if (infractions.Count > 0)
        {
            IEnumerable<Infraction> infractionList = infractions.Skip(page * infractionsPerPage).Take(infractionsPerPage);
            embed.AddField(underlinedFieldName, string.Join("\n\n", infractionList.Select(BuildInfractionString)));
        }
        else
            embed.AddField(underlinedFieldName, "✅ No infractions on record");

        return embed;

        string BuildInfractionString(Infraction infraction)
        {
            var builder = new StringBuilder();

            builder.Append(Formatter.Bold($"ID: {infraction.Id}")).Append(" \u2022 ");
            builder.AppendLine($"Issued at {Formatter.Timestamp(infraction.IssuedAt, TimestampFormat.ShortDate)}");
            builder.Append($"Punishment: {infraction.Type.Humanize()}");

            if (staffRequested)
                builder.Append($" by {MentionUtility.MentionUser(infraction.StaffMemberId)}");

            builder.AppendLine().AppendLine(infraction.Reason);

            return builder.ToString().Trim();
        }
    }

    /// <inheritdoc cref="IHammerPlugin.EnumerateInfractions(DiscordGuild)" />
    public IEnumerable<Infraction> EnumerateInfractions(DiscordGuild guild)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        if (!_infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache))
            yield break;

        foreach (Infraction infraction in cache)
            yield return infraction;
    }

    /// <inheritdoc cref="IHammerPlugin.EnumerateInfractions(DiscordUser, DiscordGuild)" />
    public IEnumerable<Infraction> EnumerateInfractions(DiscordUser user, DiscordGuild guild)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        if (!_infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache))
            yield break;

        foreach (Infraction infraction in cache.Where(i => i.UserId == user.Id))
            yield return infraction;
    }

    /// <summary>
    ///     Issues a gag infraction to a user.
    /// </summary>
    /// <param name="user">The user to warn.</param>
    /// <param name="staffMember">The staff member responsible for the warning.</param>
    /// <param name="sourceMessage">The message to which the staff member reacted.</param>
    /// <returns>The newly-created infraction.</returns>
    public async Task<Infraction> GagAsync(DiscordUser user, DiscordMember staffMember, DiscordMessage? sourceMessage = null)
    {
        user = await user.NormalizeClientAsync(_discordClient).ConfigureAwait(false);
        staffMember = await staffMember.NormalizeClientAsync(_discordClient).ConfigureAwait(false);

        DiscordGuild guild = staffMember.Guild;
        GuildConfiguration guildConfiguration = _configurationService.GetGuildConfiguration(guild);
        long gagDurationMilliseconds = guildConfiguration.MuteConfiguration.GagDuration;
        TimeSpan gagDuration = TimeSpan.FromMilliseconds(gagDurationMilliseconds);
        DateTimeOffset gagUntil = DateTimeOffset.UtcNow + gagDuration;

        try
        {
            DiscordMember member = await guild.GetMemberAsync(user.Id).ConfigureAwait(false);
            await member.TimeoutAsync(gagUntil, AuditLogReasons.GaggedUser.FormatSmart(new {staffMember})).ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            // user is not in the guild. we can safely ignore this
        }

        DiscordEmbedBuilder embed = guild.CreateDefaultEmbed(false);
        embed.WithAuthor(user);
        embed.WithColor(DiscordColor.Orange);
        embed.WithTitle(EmbedTitles.UserGagged);
        embed.AddField(EmbedFieldNames.User, user.Mention, true);
        embed.AddField(EmbedFieldNames.StaffMember, staffMember.Mention, true);
        embed.AddField(EmbedFieldNames.Duration, gagDuration.Humanize(), true);

        if (sourceMessage is not null)
        {
            bool hasContent = !string.IsNullOrWhiteSpace(sourceMessage.Content);
            bool hasAttachments = sourceMessage.Attachments.Count > 0;

            string? content = hasContent ? Formatter.BlockCode(Formatter.Sanitize(sourceMessage.Content)) : null;
            string? attachments = hasAttachments ? string.Join('\n', sourceMessage.Attachments.Select(a => a.Url)) : null;
            string messageLink = Formatter.MaskedUrl(sourceMessage.Id.ToString(), sourceMessage.JumpLink);
            string timestamp = Formatter.Timestamp(sourceMessage.CreationTimestamp, TimestampFormat.ShortDateTime);

            embed.AddField(EmbedFieldNames.MessageID, messageLink, true);
            embed.AddField(EmbedFieldNames.MessageTime, timestamp, true);
            embed.AddFieldIf(hasContent, EmbedFieldNames.Content, content);
            embed.AddFieldIf(hasAttachments, EmbedFieldNames.Attachments, attachments);
        }

        await _corePlugin.LogAsync(guild, embed).ConfigureAwait(false);

        return await CreateInfractionAsync(InfractionType.Gag, user, staffMember, new InfractionOptions {NotifyUser = false});
    }

    /// <summary>
    ///     Gets the infraction with the specified ID.
    /// </summary>
    /// <param name="infractionId">The ID of the infraction to get.</param>
    /// <returns>The infraction with the specified ID, or <see langword="null" /> if no such infraction exists.</returns>
    public Infraction? GetInfraction(long infractionId)
    {
        return _infractionCache.Values.SelectMany(i => i).FirstOrDefault(i => i.Id == infractionId);
    }

    /// <inheritdoc cref="IHammerPlugin.GetInfractionCount(DiscordGuild)" />
    public int GetInfractionCount(DiscordGuild guild)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return _infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache)
            ? cache.Count
            : 0;
    }

    /// <inheritdoc cref="IHammerPlugin.GetInfractionCount(DiscordUser, DiscordGuild)" />
    public int GetInfractionCount(DiscordUser user, DiscordGuild guild)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return GetInfractionCount(user.Id, guild.Id);
    }

    /// <inheritdoc cref="IHammerPlugin.GetInfractionCount(DiscordUser, DiscordGuild)" />
    public int GetInfractionCount(ulong userId, ulong guildId)
    {
        return _infractionCache.TryGetValue(guildId, out List<Infraction>? cache)
            ? cache.Count(i => i.UserId == userId)
            : 0;
    }

    /// <inheritdoc cref="IHammerPlugin.GetInfractions(DiscordGuild)" />
    public IReadOnlyList<Infraction> GetInfractions(DiscordGuild guild)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return _infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache)
            ? cache.ToArray()
            : ArraySegment<Infraction>.Empty;
    }

    /// <inheritdoc cref="IHammerPlugin.GetInfractions(DiscordUser, DiscordGuild)" />
    public IReadOnlyList<Infraction> GetInfractions(DiscordUser user, DiscordGuild guild)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return _infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache)
            ? cache.Where(i => i.UserId == user.Id).ToArray()
            : ArraySegment<Infraction>.Empty;
    }

    /// <summary>
    ///     Logs an infraction to the staff log channel.
    /// </summary>
    /// <param name="guild">The guild in which to log.</param>
    /// <param name="infraction">The infraction to log.</param>
    /// <param name="notificationOptions">
    ///     Optional. The staff notification options. Defaults to <see cref="StaffNotificationOptions.None" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="guild" /> is <see langword="null" />.</para>
    ///     or
    ///     <para><paramref name="infraction" /> is <see langword="null" />.</para>
    /// </exception>
    public async Task LogInfractionAsync(DiscordGuild guild, Infraction infraction,
        StaffNotificationOptions notificationOptions = StaffNotificationOptions.None)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (infraction is null) throw new ArgumentNullException(nameof(infraction));

        guild = await guild.NormalizeClientAsync(_discordClient).ConfigureAwait(false);
        DiscordEmbed embed = await CreateInfractionEmbedAsync(infraction).ConfigureAwait(false);
        await _corePlugin.LogAsync(guild, embed, notificationOptions).ConfigureAwait(false);
    }

    /// <summary>
    ///     Modifies an infraction.
    /// </summary>
    /// <param name="infraction">The infraction to modify.</param>
    /// <param name="action">The delegate to invoke for the infraction.</param>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="infraction" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="action" /> is <see langword="null" />.</para>
    /// </exception>
    public async Task ModifyInfractionAsync(Infraction infraction, Action<Infraction> action)
    {
        ArgumentNullException.ThrowIfNull(infraction);
        ArgumentNullException.ThrowIfNull(action);

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        infraction = context.Entry(infraction).Entity;
        action(infraction);
        context.Update(infraction);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Redacts an infraction.
    /// </summary>
    /// <param name="infraction">The infraction to redact.</param>
    /// <exception cref="ArgumentNullException"><paramref name="infraction" /> is <see langword="null" />.</exception>
    public async Task RemoveInfractionAsync(Infraction infraction)
    {
        if (infraction is null) throw new ArgumentNullException(nameof(infraction));

        _infractionCache[infraction.GuildId].Remove(infraction);

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        context.Remove(infraction);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Redacts a collection of infractions.
    /// </summary>
    /// <param name="infractions">The infractions to redact.</param>
    /// <exception cref="ArgumentNullException"><paramref name="infractions" /> is <see langword="null" />.</exception>
    public async Task RemoveInfractionsAsync(IEnumerable<Infraction> infractions)
    {
        if (infractions is null) throw new ArgumentNullException(nameof(infractions));

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();

        foreach (IGrouping<ulong, Infraction> group in infractions.GroupBy(i => i.GuildId))
        {
            List<Infraction> list = _infractionCache[group.Key];
            foreach (Infraction infraction in group)
            {
                context.Remove(infraction);
                list.Remove(infraction);
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.GuildAvailable += DiscordClientOnGuildAvailable;
        return Task.CompletedTask;
    }

    private async Task LoadGuildInfractions(DiscordGuild guild)
    {
        if (!_infractionCache.TryGetValue(guild.Id, out List<Infraction>? cache))
        {
            cache = new List<Infraction>();
            _infractionCache.Add(guild.Id, cache);
        }

        cache.Clear();

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<HammerContext>();
        cache.AddRange(context.Infractions.Where(i => i.GuildId == guild.Id));

        Logger.Info($"Retrieved {cache.Count} infractions for {guild}");
    }

    private Task DiscordClientOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        return LoadGuildInfractions(e.Guild);
    }
}
