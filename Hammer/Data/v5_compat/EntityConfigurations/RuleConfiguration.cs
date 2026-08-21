using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.v5_compat.EntityConfigurations;

/// <summary>
///     Defines configuration for <see cref="Data.Rule" />.
/// </summary>
internal sealed class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable("Rule");
        builder.HasKey(e => new {e.Id, e.GuildId});

        builder.Property(e => e.Id);
        builder.Property(e => e.GuildId);
        builder.Property(e => e.Brief);
        builder.Property(e => e.Description);
    }
}
