using BuildLogic45.Insurance.Core.Policies;
using BuildLogic45.Insurance.Infrastructure.Persistence;
using BuildLogic45.Insurance.Infrastructure.Persistence.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLogic45.Insurance.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the persistence backend selected by the <c>Persistence:Provider</c> setting.</summary>
    public static IServiceCollection AddInsuranceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(PersistenceOptions.SectionName);
        services.Configure<PersistenceOptions>(section);

        var options = section.Get<PersistenceOptions>() ?? new PersistenceOptions();
        switch (options.Provider)
        {
            case PersistenceProvider.InMemory:
                // Singleton so data survives across MCP requests/sessions for the life of the process.
                services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
                break;
            default:
                throw new NotSupportedException($"Persistence provider '{options.Provider}' is not supported.");
        }

        return services;
    }
}
