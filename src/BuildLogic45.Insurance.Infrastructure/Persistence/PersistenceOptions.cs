namespace BuildLogic45.Insurance.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>Which storage backend to use. Only <see cref="PersistenceProvider.InMemory"/> exists today.</summary>
    public PersistenceProvider Provider { get; set; } = PersistenceProvider.InMemory;

    /// <summary>Populate the store with sample data on startup.</summary>
    public bool SeedSampleData { get; set; } = true;
}

public enum PersistenceProvider
{
    InMemory,
}
