namespace BuildLogic45.Insurance.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>Name of the entry under <c>ConnectionStrings</c> used by the PostgreSQL provider.</summary>
    public const string ConnectionStringName = "Insurance";

    /// <summary>Which storage backend to use.</summary>
    public PersistenceProvider Provider { get; set; } = PersistenceProvider.InMemory;

    /// <summary>
    /// In-memory only: populate the store with sample data on startup. For PostgreSQL, run
    /// <c>database/postgres/003_seed_sample_data.sql</c> instead.
    /// </summary>
    public bool SeedSampleData { get; set; } = true;
}

public enum PersistenceProvider
{
    InMemory,
    Postgres,
}
