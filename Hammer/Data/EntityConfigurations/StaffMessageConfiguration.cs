using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Defines configuration for <see cref="StaffMessage" />.
/// </summary>
internal sealed class StaffMessageConfiguration : IEntityTypeConfiguration<StaffMessage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StaffMessageConfiguration" /> class.
    /// </summary>
    public StaffMessageConfiguration()
    {
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StaffMessage> builder)
    {
        builder.ToTable("StaffMessage");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnOrder(1);
        builder.Property(e => e.GuildId).HasColumnOrder(2);
        builder.Property(e => e.StaffMemberId).HasColumnOrder(3);
        builder.Property(e => e.RecipientId).HasColumnOrder(4);
        builder.Property(e => e.Content).HasColumnOrder(5);
    }
}
