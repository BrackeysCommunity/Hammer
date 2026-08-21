using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hammer.Data.EntityConfigurations;

/// <summary>
///     Represents a class which defines the database configuration for <see cref="Mute" />.
/// </summary>
internal sealed class MuteConfiguration : IEntityTypeConfiguration<Mute>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Mute> builder)
    {
        builder.HasKey(e => new { e.UserId, e.GuildId });

        builder.Property(e => e.GuildId);
        builder.Property(e => e.UserId);
        builder.Property(e => e.ExpiresAt);
    }
}
