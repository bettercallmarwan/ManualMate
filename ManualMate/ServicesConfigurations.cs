using ManualMate.Interfaces;
using ManualMate.Mapping;
using ManualMate.Presistence;
using ManualMate.Repositories;
using ManualMate.Services;
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
                optionsAction.UseSqlServer(configuration.GetConnectionString("ManualMateDbContext"));
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

        public static IServiceCollection AddQuartz(this IServiceCollection services)
        {
            //builder.Services.AddQuartz(q =>
            //{
            //    var jobKey1 = new JobKey("ProductCacheJob"); 

            //    q.AddJob<ProductCacheJob>(options => options.WithIdentity(jobKey1)); 
            //    q.AddTrigger(options => options
            //        .ForJob(jobKey1)
            //        .WithIdentity("ProductCacheJob-trigger")
            //        .WithSimpleSchedule(s => s
            //            .WithIntervalInMinutes(60)
            //            .RepeatForever()
            //        ).StartNow());

            //    var jobKey2 = new JobKey("EmbeddingCacheJob");

            //    q.AddJob<EmbeddingCacheJob>(options => options.WithIdentity(jobKey2));
            //    q.AddTrigger(options => options
            //        .ForJob(jobKey2)
            //        .WithIdentity("EmbeddingCacheJob-trigger")
            //        .WithSimpleSchedule(s => s
            //            .WithIntervalInMinutes(60)
            //            .RepeatForever()
            //        ).StartNow());
            //});

            // to wait for any currently excecuting jobs to complete before shutting down program
            //builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true); 

            return services;
        }
    }
}
 