using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using ManualMate.API;
using ManualMate.Application.Responses;
using ManualMate.Infrastructure.Consumers;
using ManualMate.Infrastructure.Presistence;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

namespace ManualMate
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
                configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });


            builder.Services.AddServices(builder.Configuration);// extension


            builder.Services.AddHttpClient("GeminiClient", client =>
            {
                string modelUrl = builder.Configuration["Gemini:ModelUrl"]!;
                string apiToken = builder.Configuration["Gemini:GeminiToken"]!;

                client.BaseAddress = new Uri($"{modelUrl}{apiToken}");
            });

            builder.Services.AddHttpClient("HuggingFaceClient", client =>
            {
                string modelUrl = builder.Configuration["HuggingFace:ModelUrl"]!;
                string apiToken = builder.Configuration["HuggingFace:ApiToken"]!;

                client.BaseAddress = new Uri(modelUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiToken);
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
                
            var app = builder.Build();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ManualMateDbContext>();
                context.Database.Migrate();
                await DbInitializer.SeedAsync(context);
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}