using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System;
using System.IO;
using System.Reflection;
using TodoApi.Filters;
using TodoApi.Middlewares;
using TodoApi.Options;
using TodoApi.Repositories;

namespace TodoApi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string CORS_NAME = "PoliticaPermissiva";

            var builder = WebApplication.CreateBuilder();

            builder.Services.Configure<MaintenantanceOptions>(builder.Configuration.GetSection(MaintenantanceOptions.SectionName));
            builder.Services.Configure<IpAllowListOptions>(builder.Configuration.GetSection(IpAllowListOptions.SectionName));

            // Controllers + global exception filter
            builder.Services.AddControllers(opt =>
            {
                opt.Filters.Add<ProblemDetailsExceptionFilter>();
            });

            builder.Services.AddScoped<ITodoRepository, TodoRepository>();
            builder.Services.AddDbContext<TodoDbContext>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Todo API",
                    Version = "v1",
                    Description = "A simple Todo API with CRUD endpoints."
                });
                c.EnableAnnotations();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath, true);
            });

            builder.Services.AddCors(option => option.AddPolicy(CORS_NAME, policy => {
                policy
                .WithOrigins("http://localhost:3000")
                .WithMethods("GET", "OPTIONS", "PUT", "DELETE")
                .AllowAnyHeader();
            }));
                
            var app = builder.Build();
            app.UseMiddleware<IpAccessControlMiddleware>();
            app.UseMiddleware<MaintenanceMiddleware>();

            app.UseCors(CORS_NAME);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
            });
            app.MapControllers();

            // Configurando Middlewares
            // UseXXXXX
            // MapXXXXX 
            app.Run();
        }
    }
}
