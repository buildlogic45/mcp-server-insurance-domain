using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

/// <summary>
/// Used only by <c>dotnet ef</c> at design time. Set <c>ConnectionStrings__Insurance</c> to target
/// a specific database; adding a migration does not need a connection.
/// </summary>
internal sealed class InsuranceDbContextFactory : IDesignTimeDbContextFactory<InsuranceDbContext>
{
    public InsuranceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{PersistenceOptions.ConnectionStringName}")
            ?? "Host=localhost;Database=insurance;Username=postgres";

        var options = new DbContextOptionsBuilder<InsuranceDbContext>().UseNpgsql(connectionString).Options;
        return new InsuranceDbContext(options);
    }
}
