using ManualMate.API.Controllers.Responses;
using ManualMate.Infrastructure.Presistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Serilog.Events;
using System.Net.Http.Headers;

namespace ManualMate.API
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
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS
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
                string MODEL_URL = builder.Configuration["Gemini:ModelUrl"]!;
                string API_TOKEN = builder.Configuration["Gemini:GeminiToken"]!;

                client.BaseAddress = new Uri($"{MODEL_URL}{API_TOKEN}");
            });

            builder.Services.AddHttpClient("HuggingFaceClient", client =>
            {
                string MODEL_URL = builder.Configuration["HuggingFace:ModelUrl"]!;
                string API_TOKEN = builder.Configuration["HuggingFace:ApiToken"]!;

                client.BaseAddress = new Uri(MODEL_URL);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", API_TOKEN);
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
                await DbInitializer.seedAsync(context);
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
