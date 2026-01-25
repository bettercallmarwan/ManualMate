using ManualMate.Application.Interfaces;
using ManualMate.Application.Mapping;
using ManualMate.Application.Services;
using ManualMate.Infrastructure.Presistence;
using ManualMate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ManualMate.API
{
    public static class ServicesConfigurations
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ManualMateDbContext>(optionsAction =>
            {
                optionsAction.UseNpgsql(
                    configuration.GetConnectionString("ManualMateDbContext"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.UseVector();
                    });
            });
            services.AddSingleton<IConnectionMultiplexer>(options =>
            {
                return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
            });
            services.AddScoped<ProductService>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<FileUploadService>();
            services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();
            services.AddScoped<IManualProcessingService, ManualProcessingService>();
            services.AddScoped<ILlmService, GeminiLlmService>();
            services.AddScoped<IManualQaService, ManualQaService>();
            services.AddScoped<ProductRepository>();
            services.AddScoped<ManualEmbeddingRepository>();
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
   