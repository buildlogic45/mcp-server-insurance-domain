namespace BuildLogic45.Insurance.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>Name of the entry under <c>ConnectionStrings</c> used by the PostgreSQL provider.</summary>
    public const string ConnectionStringName = "Insurance";

    /// <summary>Which storage backend to use.</summary>
    public PersistenceProvider Provider { get; set; } = PersistenceProvider.InMemory;

    /// <summary>
    /// Populate the store with sample data on startup. For PostgreSQL this only happens when the
    /// policies table is empty, so existing data is never overwritten.
    /// </summary>
    public bool SeedSampleData { get; set; } = true;

    /// <summary>PostgreSQL only: apply pending EF Core migrations on startup.</summary>
    public bool ApplyMigrationsOnStartup { get; set; } = true;
}

public enum PersistenceProvider
{
    InMemory,
    Postgres,
}
