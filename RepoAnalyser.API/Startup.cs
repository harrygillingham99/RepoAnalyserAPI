using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;
using RepoAnalyser.API.NSwag;
using Serilog;

namespace RepoAnalyser.API
{
    public class Startup
    {
        private const string CorsKey = "Policy";
        private static readonly string[] AllowedOrigins =  { "https://192.168.0.69:4433", "http://localhost:3000", "https://server.local:4433"};
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsKey,
                    builder =>
                    {
                        builder.WithOrigins(AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "RepoAnalyser API";
                configure.DocumentName = "API";
                configure.Description = "An API interface.";
                configure.OperationProcessors.Add(new HeaderParameterOperationProcessor());
                configure.DocumentProcessors.Add(new SchemaExtenderDocumentProcessor());
                configure.AddSecurity("GitHub Token", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "GitHub Bearer Token"
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("GitHub Token"));
            });

            ServiceRegistry.AddConfigs(services, Configuration);

            ServiceRegistry.AddBackgroundTaskQueue(services);

            ServiceRegistry.ScanForAllRemainingRegistrations(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                //home server specific config here
            }

            app.UseOpenApi();

            app.UseSwaggerUi3();

            app.UseReDoc(options =>
            {
                options.Path = "/api-docs";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(CorsKey);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
