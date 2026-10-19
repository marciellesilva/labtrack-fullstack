using Amazon.SQS;
using LabTrack.Application.Abstractions;
using LabTrack.Infrastructure.Messaging;
using LabTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LabTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LabTrackDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("LabTrack")));

        services.AddScoped<ISampleRepository, SampleRepository>();

        // Mensageria configurável: "Logging" (padrão) ou "Sqs".
        var messagingSection = configuration.GetSection(MessagingOptions.SectionName);
        services.Configure<MessagingOptions>(messagingSection);
        var provider = messagingSection.GetValue<string>(nameof(MessagingOptions.Provider));

        if (string.Equals(provider, "Sqs", StringComparison.OrdinalIgnoreCase))
        {
            // Credenciais e região vêm da cadeia padrão da AWS (variáveis de ambiente, perfil, IAM role).
            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
            services.AddScoped<IEventPublisher, SqsEventPublisher>();
        }
        else
        {
            services.AddScoped<IEventPublisher, LoggingEventPublisher>();
        }

        return services;
    }
}
