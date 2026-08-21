using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace Hammer.Data;

/// <summary>
///     Provides methods for configuring the <see cref="HammerContext" /> database context.
/// </summary>
internal static class HammerContextConfig
{
    /// <summary>
    ///     Configures the <see cref="HammerContext" /> database context with the specified connection string.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder" /> to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseNpgsql(connectionString, options =>
        {
            options.MapEnum<InfractionType>("infraction_type", "hammer", new NpgsqlSnakeCaseNameTranslator());
        });
        builder.UseSnakeCaseNamingConvention();
    }
}
