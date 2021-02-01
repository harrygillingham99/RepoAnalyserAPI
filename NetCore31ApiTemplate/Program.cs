using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace NetCore31ApiTemplate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    new JsonFormatter(renderMessage: true),
                    Path.Combine(AppContext.BaseDirectory, "logs//Serilog.json"),
                    shared: true,
                    fileSizeLimitBytes: 20_971_520,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 10)
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
                }).UseSerilog();
    }
}
