using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

/// <summary>
/// Applies migrations and seeds sample data before the MCP transport starts accepting requests.
/// Registered ahead of the MCP server, so it runs first.
/// </summary>
internal sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<PersistenceOptions> options,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        if (options.Value.ApplyMigrationsOnStartup)
        {
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("PostgreSQL schema is up to date");
        }

        if (options.Value.SeedSampleData && !await db.Policies.AnyAsync(cancellationToken))
        {
            var policies = PolicySeedData.Create();
            db.Policies.AddRange(policies);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded PostgreSQL policy store with {Count} policies", policies.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
