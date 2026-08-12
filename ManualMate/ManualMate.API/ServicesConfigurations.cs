using System.Net;
using System.Reflection.Metadata;
using FluentValidation;
using FluentValidation.AspNetCore;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Mapping;
using ManualMate.Application.Services;
using ManualMate.Application.Validators;
using ManualMate.Infrastructure.Consumers;
using ManualMate.Infrastructure.Presistence;
using ManualMate.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ManualMate
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
            // services.AddScoped<IApplicationDbContext, ManualMateDbContext>();
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<ManualMateDbContext>());

            services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateItemDtoValidator>();

            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();
            services.AddScoped<IFileProcessingService, FileProcessingService>();
            services.AddScoped<ILlmService, GeminiLlmService>();
            services.AddScoped<IQaService, QaService>();
            services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
            services.AddScoped<ITextChunker, TextChunker>();
            services.AddScoped<IIntegrationEventBus, MassTransitEventBus>();
            
            
            
            services.AddAutoMapper(cfg => {}, typeof(MappingProfile));
            
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ItemCreatedConsumer>();

                x.AddEntityFrameworkOutbox<ManualMateDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    var conf = context.GetRequiredService<IConfiguration>();
        
                    cfg.Host(conf["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(conf["RabbitMQ:Username"]!);
                        h.Password(conf["RabbitMQ:Password"]!);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Handle<TimeoutException>();
                        r.Handle<HttpRequestException>(e => e.StatusCode is null or >= HttpStatusCode.InternalServerError);        
                        r.Ignore<HttpRequestException>(e => e.StatusCode < HttpStatusCode.BadRequest);
                        r.Interval(3, TimeSpan.FromSeconds(5));
                    });
                    
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
   