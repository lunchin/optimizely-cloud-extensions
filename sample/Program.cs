using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace sample;

public static class Program
{
    public static void Main(string[] args)
    {
        var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                 .AddJsonFile("appsettings.local.json", true, true)
                .AddEnvironmentVariables();
        IConfiguration Configuration = configBuilder.Build();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == Environments.Development || Configuration.GetValue<bool>("RunAsDevelopment");
        if (isDevelopment)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("EPiServer", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Optimizely", Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("App_Data/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
            .WriteTo.Console()
            .CreateLogger();
        }

        Log.Information($"Starting application in environment {environment}.");
        Log.Information($"Loading Configuration from JSON files: {string.Join(", ", configBuilder.Sources.Where(s => s is FileConfigurationSource).Select(s => (s as FileConfigurationSource).Path))}");

        try
        {
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            if (isDevelopment)
            {
                loggerFactory.AddSerilog(Log.Logger);
            }

            CreateHostBuilder(args, Configuration, isDevelopment, loggerFactory.CreateLogger<Startup>()).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Error("Begin Startup Failed Message");
            Log.Fatal(ex, "End Startup Failed Message");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration, bool isDevelopment, ILogger<Startup> logger)
    {
        Log.Information("Create Host Builder");
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration));

        if (isDevelopment)
        {
            Log.Information("Using Development host builder configuration");
            return hostBuilder
                .UseSerilog()
                .ConfigureCmsDefaults()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup(context => new Startup(context.HostingEnvironment, configuration)));
        }

        return hostBuilder
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup(context => new Startup(context.HostingEnvironment, configuration)));
    }
}
