using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Defines configuration for <see cref="Infraction" />.
/// </summary>
internal sealed class InfractionConfiguration : IEntityTypeConfiguration<Infraction>
{
    private readonly bool _isMySql;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfractionConfiguration" /> class.
    /// </summary>
    /// <param name="isMySql">
    ///     <see langword="true" /> if this configuration should use MySQL configuration, otherwise <see langword="false" />.
    /// </param>
    public InfractionConfiguration(bool isMySql)
    {
        _isMySql = isMySql;
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Infraction> builder)
    {
        builder.ToTable("Infraction");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnOrder(1);
        builder.Property(e => e.GuildId).HasColumnOrder(2);
        builder.Property(e => e.UserId).HasColumnOrder(3);
        builder.Property(e => e.StaffMemberId).HasColumnOrder(4);
        builder.Property(e => e.Type).HasColumnOrder(5);

        if (_isMySql)
        {
            builder.Property(e => e.IssuedAt).HasColumnOrder(6).HasColumnType("DATETIME(6)");
        }
        else
        {
            builder.Property(e => e.IssuedAt).HasColumnOrder(7).HasConversion<DateTimeOffsetToBytesConverter>();
        }

        builder.Property(e => e.Reason).HasColumnOrder(8).HasMaxLength(255);
        builder.Property(e => e.AdditionalInformation).HasColumnOrder(9).HasMaxLength(255);
        builder.Property(e => e.RuleId).HasColumnOrder(10);
        builder.Property(e => e.RuleText).HasColumnOrder(11).HasMaxLength(255);
    }
}
