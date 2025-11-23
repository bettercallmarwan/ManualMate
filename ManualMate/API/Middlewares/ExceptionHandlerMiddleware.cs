using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Exceptions;
using System.Net;

namespace ManualMate.API.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger; 
        private readonly IWebHostEnvironment _environment; 

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext); 

            }
            catch (Exception ex)
            {
                if (_environment.IsDevelopment())
                {
                    _logger.LogError(ex, ex.Message);
                }
                else
                {
                }
                await HanldeExceptionAsync(httpContext, ex);
            }
            }

        private async Task HanldeExceptionAsync(HttpContext httpContext, Exception ex)
        {
            switch (ex)
            {
                case NotFoundException:
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsJsonAsync(Result<NotFoundException>.Fail(ex.Message.ToString()));
                    break;
                default:
                    httpContext.Response.StatusCode = 500;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsJsonAsync(Result<object>.Fail(ex.Message));
                    break;
            }
        }
    }
}