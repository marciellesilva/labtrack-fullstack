using LabTrack.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LabTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISampleService, SampleService>();
        return services;
    }
}
