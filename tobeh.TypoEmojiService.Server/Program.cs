using System.Globalization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using tobeh.TypoEmojiService.Server.Grpc;

namespace tobeh.TypoEmojiService.Server;

class Program
{
    public static void Main(string[] args)
    {
        SetupCulture();
        var builder = SetupApp(args);
        var app = builder.Build();
        SetupRoutes(app);
       
        app.Run();
    }

    private static void SetupCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    private static WebApplicationBuilder SetupApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // configure kestrel
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Setup a HTTP/2 endpoint without TLS.
            options.ListenAnyIP(builder.Configuration.GetRequiredSection("Grpc").GetValue<int>("HostPort"), o => o.Protocols = HttpProtocols.Http2);
        });

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddHttpClient();
        builder.Services.AddLogging();
        
        //builder.Services.AddScoped<servicename>();
        //builder.Services.Configure<configname>(builder.Configuration.GetSection("Git"));
        
        return builder;
    }

    private static void SetupRoutes(WebApplication app)
    {
        // Configure the HTTP request pipeline
        app.MapGrpcService<EmojisGrpcService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    }
}