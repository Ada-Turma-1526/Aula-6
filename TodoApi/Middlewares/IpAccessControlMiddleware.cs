using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using TodoApi.Options;

namespace TodoApi.Middlewares
{
    public class IpAccessControlMiddleware
    {
        private readonly RequestDelegate next;
        private HashSet<string> allowedIps;

        // Uma Classe Middleware precisa satisfazer 2 condições:
        // 1 - Construtor que recece um RequestDelegate
        // 2 - Método chamado InvokeAsync que recebe um parametros do tipo HttpContext

        public IpAccessControlMiddleware(RequestDelegate next, IOptions<IpAllowListOptions> ipAlloListOptions)
        {
            this.next = next;
            this.allowedIps = new(ipAlloListOptions.Value?.AllowedIps ?? new());
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            if (!allowedIps.Contains(ip))
            {
                var problem = new ProblemDetails
                {
                    Type = $"https://httpstatuses.com/503",
                    Title = "Access Denied",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "IP Not Allowed",
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                // Headers da Resposta
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = $"{MediaTypeNames.Application.ProblemJson}; charset={Encoding.UTF8.WebName}";

                var json = System.Text.Json.JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(json);
                return;
            }

            await next(context);
        }
    }
}
