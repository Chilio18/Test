using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Domain.Interfaces;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;
using UnameIT.RevenueIntelligence.Infrastructure.AI.Mock;
using UnameIT.RevenueIntelligence.Infrastructure.AI.OpenAI;
using UnameIT.RevenueIntelligence.Infrastructure.Caching;
using UnameIT.RevenueIntelligence.Infrastructure.CRM.Mock;
using UnameIT.RevenueIntelligence.Infrastructure.Data;
using UnameIT.RevenueIntelligence.Infrastructure.Identity;
using UnameIT.RevenueIntelligence.Infrastructure.Speech.Mock;
using UnameIT.RevenueIntelligence.Infrastructure.Storage;

namespace UnameIT.RevenueIntelligence.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(3)));

        // Redis
        var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Storage
        services.Configure<S3StorageOptions>(configuration.GetSection("Storage:S3"));
        services.AddScoped<IStorageService, S3StorageService>();

        // Encryption
        services.Configure<EncryptionOptions>(configuration.GetSection("Encryption"));
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Identity / Tenant
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, CurrentTenant>();

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AI Provider — default Mock for dev, swap via config
        var aiProvider = configuration["AI:Provider"] ?? "Mock";
        if (aiProvider == "OpenAI")
        {
            services.Configure<OpenAIProviderOptions>(configuration.GetSection("AI:OpenAI"));
            services.AddScoped<IAIProvider, OpenAIProvider>();
        }
        else
        {
            services.AddScoped<IAIProvider, MockAIProvider>();
        }

        // Speech Provider
        services.AddScoped<ISpeechProvider, MockSpeechProvider>();

        // CRM Provider
        services.AddScoped<ICrmProvider, MockCrmProvider>();

        // Hangfire background jobs
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlClientFactory(configuration.GetConnectionString("DefaultConnection")!)));
        services.AddHangfireServer();

        return services;
    }
}
