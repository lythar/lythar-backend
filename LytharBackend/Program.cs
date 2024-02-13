namespace LytharBackend;

using LytharBackend.Exceptons;
using LytharBackend.Ldap;
using LytharBackend.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOpenApiDocument(options =>
            {
                options.Title = "Lythar";
            });
        }
        
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddMvc()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseContext")));
        builder.Services.AddSingleton<LdapService>();
        builder.Services.AddSingleton<ISessionService, JwtSessionsService>();

        var app = builder.Build();

        app.UseExceptionHandler(options =>
        {
            options.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>();

                BaseHttpException httpException;

                if (exception?.Error is BaseHttpException)
                {
                    httpException = (BaseHttpException)exception.Error;
                }
                else
                {
                    httpException = new BaseHttpException("InternalServerError", "Internal server error.", System.Net.HttpStatusCode.InternalServerError);
                }

                context.Response.StatusCode = httpException.Options.StatusCode;
                await context.Response.WriteAsJsonAsync(httpException.Options);
            });
        });
        app.UseRouting();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.MapControllers();

        app.Run();
    }
}