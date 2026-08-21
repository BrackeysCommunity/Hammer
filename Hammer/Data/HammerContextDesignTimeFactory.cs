using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hammer.Data;

[UsedImplicitly]
internal sealed class HammerContextDesignTimeFactory : IDesignTimeDbContextFactory<HammerContext>
{
    public HammerContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Username=root;Password=localdev;Database=postgres";

        var options = new DbContextOptionsBuilder<HammerContext>();
        HammerContextConfig.Configure(options, connectionString);

        return new HammerContext(options.Options);
    }
}
