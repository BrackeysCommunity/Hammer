using Hammer.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Represents a class which defines the database configuration for <see cref="ReportedMessage" />.
/// </summary>
internal class ReportedMessageConfiguration : IEntityTypeConfiguration<ReportedMessage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReportedMessageConfiguration" /> class.
    /// </summary>
    public ReportedMessageConfiguration()
    {
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ReportedMessage> builder)
    {
        builder.ToTable("ReportedMessage");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnOrder(1);
        builder.Property(e => e.GuildId).HasColumnOrder(2);
        builder.Property(e => e.ChannelId).HasColumnOrder(3);
        builder.Property(e => e.MessageId).HasColumnOrder(4);
        builder.Property(e => e.AuthorId).HasColumnOrder(5);
        builder.Property(e => e.ReporterId).HasColumnOrder(6);
        builder.Property(e => e.Content).HasColumnOrder(7);
        builder.Property(e => e.Attachments).HasColumnOrder(8).HasConversion<UriListToBytesConverter>();
    }
}
