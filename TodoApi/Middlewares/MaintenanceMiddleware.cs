using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TodoApi.Options;

namespace TodoApi.Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<MaintenanceMiddleware> logger;
        private readonly MaintenantanceOptions options;

        public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger, IOptions<MaintenantanceOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new MaintenantanceOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (options.IsInMaintenantanceMode)
            {
                // Conteúdo da Resposta
                var problem = new ProblemDetails
                {
                    Type = $"https://httpstatuses.com/503",
                    Title = "API Indisponível",
                    Status = 503,
                    Detail = options.MaintenantanceMessage,
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;
                problem.Extensions["maintenance"] = true;

                // Headers da Resposta
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                context.Response.Headers["Retry-After"] = "120";

                var json = System.Text.Json.JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(json);
                return;
            }
            await next(context);
        }
    }
}
