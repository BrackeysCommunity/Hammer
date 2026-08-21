using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Defines configuration for <see cref="Infraction" />.
/// </summary>
internal sealed class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Infraction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.GuildId);
        builder.Property(e => e.UserId);
        builder.Property(e => e.StaffMemberId);
        builder.Property(e => e.Type);
        builder.Property(e => e.IssuedAt);
        builder.Property(e => e.Reason).HasMaxLength(255);
        builder.Property(e => e.AdditionalInformation).HasMaxLength(255);
        builder.Property(e => e.RuleId);
        builder.Property(e => e.RuleText).HasMaxLength(255);
    }
}
