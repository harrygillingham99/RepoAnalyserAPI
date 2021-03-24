using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace RepoAnalyser.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithMemoryUsage()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Error || ev.Level == LogEventLevel.Fatal)
                    .WriteTo.File(
                        new JsonFormatter(renderMessage: true),
                        Path.Combine(AppContext.BaseDirectory, "logs//Serilog.json"),
                        shared: true,
                        fileSizeLimitBytes: 20_971_520, // roughly 20 MB
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 10)
                    .WriteTo.Console())
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Information || ev.Level == LogEventLevel.Warning)
                    .WriteTo.File(
                        new JsonFormatter(renderMessage: true),
                        Path.Combine(AppContext.BaseDirectory, "logs//Info.json"),
                        shared: true,
                        fileSizeLimitBytes: 20_971_520, // roughly 20 MB
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 10)
                    .WriteTo.Console())
                .CreateLogger();

            try
            {
                Log.Information("Starting Application");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog(Log.Logger);
    }
}
