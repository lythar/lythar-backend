namespace LytharBackend;

using LytharBackend.Exceptons;
using LytharBackend.Ldap;
using LytharBackend.Session;
using LytharBackend.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
                    httpException = new InternalServerException();
                }

                context.Response.StatusCode = httpException.Options.StatusCode;
                await context.Response.WriteAsJsonAsync(httpException.Options);
            });
        });
        app.UseWebSockets();
        app.UseRouting();

        if (app.Environment.IsDevelopment())
        {
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials()
            );

            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.MapControllers();

        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var sessionService = context.RequestServices.GetService<ISessionService>();

                if (sessionService == null)
                {
                    throw new InternalServerException();
                }

                var token = await sessionService.VerifyRequest(context);

                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var client = new WebSocketClient(context, webSocket, token);

                await client.Listen();
            }
            else
            {
                await next(context);
            }

            // 404 handler, we do this so we don't write logs for 404s
            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                var notFound = new NotFoundException(context.Request.Path);

                context.Response.StatusCode = notFound.Options.StatusCode;
                await context.Response.WriteAsJsonAsync(notFound.Options);
            }
        });

        app.Run();
    }
}