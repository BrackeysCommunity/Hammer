using Hammer.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Represents a class which defines the database configuration for <see cref="TrackedMessage" />.
/// </summary>
internal sealed class TrackedMessageConfiguration : IEntityTypeConfiguration<TrackedMessage>
{
    private readonly bool _isMySql;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TrackedMessageConfiguration" /> class.
    /// </summary>
    /// <param name="isMySql">
    ///     <see langword="true" /> if this configuration should use MySQL configuration, otherwise <see langword="false" />.
    /// </param>
    public TrackedMessageConfiguration(bool isMySql)
    {
        _isMySql = isMySql;
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TrackedMessage> builder)
    {
        builder.ToTable("TrackedMessages");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnOrder(1);
        builder.Property(e => e.GuildId).HasColumnOrder(2);
        builder.Property(e => e.ChannelId).HasColumnOrder(3);
        builder.Property(e => e.AuthorId).HasColumnOrder(4);
        builder.Property(e => e.IsDeleted).HasColumnOrder(5);
        builder.Property(e => e.Content).HasColumnOrder(6).HasMaxLength(4000);
        builder.Property(e => e.Attachments).HasColumnOrder(7).HasConversion<UriListToBytesConverter>();

        if (_isMySql)
        {
            builder.Property(e => e.CreationTimestamp).HasColumnOrder(8).HasColumnType("DATETIME(6)");
            builder.Property(e => e.DeletionTimestamp).HasColumnOrder(9).HasColumnType("DATETIME(6)");
        }
        else
        {
            builder.Property(e => e.CreationTimestamp).HasColumnOrder(8).HasConversion<DateTimeOffsetToBytesConverter>();
            builder.Property(e => e.DeletionTimestamp).HasColumnOrder(9).HasConversion<DateTimeOffsetToBytesConverter>();
        }
    }
}
