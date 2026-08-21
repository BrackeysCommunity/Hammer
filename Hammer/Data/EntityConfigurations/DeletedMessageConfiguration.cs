using Hammer.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Represents a class which defines the configuration for the <see cref="DeletedMessage"/> entity.
/// </summary>
internal sealed class DeletedMessageConfiguration : IEntityTypeConfiguration<DeletedMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeletedMessage> builder)
    {
        builder.HasKey(e => e.MessageId);

        builder.Property(e => e.MessageId);
        builder.Property(e => e.GuildId);
        builder.Property(e => e.ChannelId);
        builder.Property(e => e.AuthorId);
        builder.Property(e => e.StaffMemberId);
        builder.Property(e => e.Content).HasMaxLength(1024);
        builder.Property(e => e.Attachments).HasConversion<UriListToBytesConverter>();
        builder.Property(e => e.CreationTimestamp);
        builder.Property(e => e.DeletionTimestamp);
        builder.Ignore(e => e.AddedByBot);
    }
}
