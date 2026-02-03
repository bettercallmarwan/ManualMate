using FluentValidation;
using FluentValidation.AspNetCore;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Mapping;
using ManualMate.Application.Services;
using ManualMate.Application.Validators;
using ManualMate.Infrastructure.Presistence;
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
            services.AddScoped<IApplicationDbContext, ManualMateDbContext>();

            services.AddSingleton<IConnectionMultiplexer>(options =>
            {
                return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
            });

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateItemDtoValidator>();

            services.AddScoped<ItemService>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<FileUploadService>();
            services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();
            services.AddScoped<IFileProcessingService, FileProcessingService>();
            services.AddScoped<ILlmService, GeminiLlmService>();
            services.AddScoped<IQaService, QaService>();
            
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
   