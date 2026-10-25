using BuildLogic45.Insurance.Core.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildLogic45.Insurance.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddInsuranceCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IPolicyService, PolicyService>();
        return services;
    }
}
