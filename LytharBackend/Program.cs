namespace LytharBackend;

using LytharBackend.Ldap;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lythar", Version = "v1" });
            });
        }
        
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddMvc();

        builder.Services.AddScoped<LdapService>();

        var app = builder.Build();

        app.UseRouting();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.MapControllers();

        app.Run();
    }
}