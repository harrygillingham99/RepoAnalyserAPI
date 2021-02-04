using System.CodeDom.Compiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoAnalyser.Logic;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.Attributes;
using RepoAnalyser.Services;
using RepoAnalyser.SqlServer.DAL;
using Scrutor;

namespace RepoAnalyser.API
{
    public static class ServiceRegistry
    {
        public static void ScanForAllRemainingRegistrations(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(Startup), typeof(OctoKitAuthServiceAgent), typeof(RepoAnalyserRepository), typeof(AuthFacade))
                .AddClasses(x => x.WithoutAttribute(typeof(GeneratedCodeAttribute)).WithoutAttribute(typeof(ScrutorIgnoreAttribute)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        public static void AddConfigs(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(option => { configuration.GetSection(nameof(AppSettings)).Bind(option); });
            services.Configure<GitHubSettings>(option => { configuration.GetSection(nameof(GitHubSettings)).Bind(option); });
        }
    }
}
