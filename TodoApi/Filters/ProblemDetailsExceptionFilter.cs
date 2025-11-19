using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace TodoApi.Filters
{
    public class ProblemDetailsExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment env;
        private readonly ILogger<ProblemDetailsExceptionFilter> logger;

        public ProblemDetailsExceptionFilter(IHostEnvironment env, ILogger<ProblemDetailsExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var (status, title) = MapException(context.Exception);

            // Log
            logger.LogError(context.Exception, "Unhandled exception converted to ProblemDetails (Status {Status})", status);

            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = env.IsDevelopment() ? context.Exception.Message : null,
                Instance = context.HttpContext.Request.Path
            };

            // Extra metadata
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            if (env.IsDevelopment())
            {
                problem.Extensions["exception"] = context.Exception.GetType().FullName;
                problem.Extensions["stack"] = context.Exception.StackTrace;
            }

            context.Result = new ObjectResult(problem)
            {
                StatusCode = status,
                ContentTypes = { "application/problem+json" }
            };
            context.ExceptionHandled = true;
        }

        private static (int status, string title) MapException(Exception ex) =>
            ex switch
            {
                ArgumentNullException => (400, "Missing required data"),
                ArgumentException => (400, "Invalid argument"),
                KeyNotFoundException => (404, "Resource not found"),
                UnauthorizedAccessException => (403, "Forbidden"),
                NotImplementedException => (501, "Not implemented"),
                TimeoutException => (504, "Operation timed out"),
                _ => (500, "An unexpected error occurred")
            };
    }
}
