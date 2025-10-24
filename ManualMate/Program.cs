using ManualMate.Controllers.Responses;
using ManualMate.Interfaces;
using ManualMate.Jobs;
using ManualMate.Presistence;
using ManualMate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace ManualMate
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ManualMateDbContext>(optionsAction =>
            {
                optionsAction.UseSqlServer(builder.Configuration.GetConnectionString("ManualMateDbContext"));
            });
            

            builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
            {
                return ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = string.Join("; ", context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return new BadRequestObjectResult(Result<object>.Fail(errors));
                };
            });


            builder.Services.AddQuartz(q =>
            {
                var jobKey1 = new JobKey("ProductCacheJob"); 

                q.AddJob<ProductCacheJob>(options => options.WithIdentity(jobKey1)); 
                q.AddTrigger(options => options
                    .ForJob(jobKey1)
                    .WithIdentity("ProductCacheJob-trigger")
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(60)
                        .RepeatForever()
                    ).StartNow());

                var jobKey2 = new JobKey("EmbeddingCacheJob");

                q.AddJob<EmbeddingCacheJob>(options => options.WithIdentity(jobKey2));
                q.AddTrigger(options => options
                    .ForJob(jobKey2)
                    .WithIdentity("EmbeddingCacheJob-trigger")
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(60)
                        .RepeatForever()
                    ).StartNow());
            });

            // to wait for any currently excecuting jobs to complete before shutting down program
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true); 


            builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<FileUploadService>();
            builder.Services.AddScoped<RedisService>();
            builder.Services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();
            builder.Services.AddScoped<IManualProcessingService, ManualProcessingService>();
            builder.Services.AddScoped<ILlmService, GeminiLlmService>();
            builder.Services.AddScoped<IManualQaService, ManualQaService>();                


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ManualMateDbContext>();
                context.Database.Migrate();
                await DbInitializer.seedAsync(context);
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            app.Run();
        }
    }
}
