using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace RepoAnalyser.API
{
    public static class Program
    {
        private static readonly LogEventLevel[] InfoEvents =
            {LogEventLevel.Information, LogEventLevel.Warning, LogEventLevel.Debug};

        private static readonly LogEventLevel[] ErrorEvents = {LogEventLevel.Error, LogEventLevel.Fatal};

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithMemoryUsage()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(ev => ErrorEvents.Contains(ev.Level))
                    .WriteTo.JsonFile("logs")
                    .WriteTo.Console())
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(ev => InfoEvents.Contains(ev.Level))
                    .WriteTo.JsonFile("info")
                    .WriteTo.Console())
                .CreateLogger();

            try
            {
                Log.Information("***Starting Application***");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "***Application failed to start***");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static LoggerConfiguration JsonFile(this LoggerSinkConfiguration configuration, string fileName)
        {
            return configuration.File(new JsonFormatter(renderMessage: true),
                Path.Combine(AppContext.BaseDirectory, $"logs//{fileName}.json"),
                shared: true,
                fileSizeLimitBytes: 20_971_520, // roughly 20 MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 10);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); }).UseSerilog(Log.Logger);
        }
    }
}