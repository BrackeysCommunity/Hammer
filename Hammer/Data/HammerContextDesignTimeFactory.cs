using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hammer.Data;

[UsedImplicitly]
internal sealed class HammerContextDesignTimeFactory : IDesignTimeDbContextFactory<HammerContext>
{
    public HammerContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HammerContext>();
        var serverVersion = new MariaDbServerVersion(ServerVersion.Parse("10.5.3-mariadb"));

        const string connectionString = "server=localhost;database=hammer;user=root;password=your_password";
        options.UseMySql(connectionString, serverVersion, builder => builder.DisableBackslashEscaping());

        return new HammerContext(options.Options);
    }
}
