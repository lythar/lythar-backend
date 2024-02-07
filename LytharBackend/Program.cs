namespace LytharBackend;

using LytharBackend.Ldap;
using Microsoft.AspNetCore.Builder;
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
        builder.Services.AddScoped<LdapService>();

        var app = builder.Build();

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