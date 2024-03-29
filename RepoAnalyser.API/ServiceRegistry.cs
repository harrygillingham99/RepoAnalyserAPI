﻿using System.CodeDom.Compiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoAnalyser.Logic;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Objects.Attributes;
using RepoAnalyser.Objects.Config;
using RepoAnalyser.Services.OctoKit;
using RepoAnalyser.Services.ProcessUtility;
using RepoAnalyser.SignalR.Hubs;
using RepoAnalyser.SqlServer.DAL;
using Scrutor;

namespace RepoAnalyser.API
{
    public static class ServiceRegistry
    {
        public static void ScanForAllRemainingRegistrations(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(Startup), typeof(OctoKitAuthServiceAgent), typeof(AppHub), typeof(RepoAnalyserAuditRepository), typeof(AuthenticationFacade))
                .AddClasses(x => x.WithoutAttribute(typeof(GeneratedCodeAttribute)).WithoutAttribute(typeof(ScrutorIgnoreAttribute)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        public static void AddBackgroundTaskQueue(IServiceCollection services)
        {
            services.AddHostedService<BackgroundHostedWorker>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }

        public static void AddConfigs(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(option => configuration.GetSection(nameof(AppSettings)).Bind(option));
            services.Configure<GitHubSettings>(option => configuration.GetSection(nameof(GitHubSettings)).Bind(option));
        }

        public static void AddSingletons(IServiceCollection services)
        {
            services.AddSingleton<IWinProcessUtil, WinProcessUtil>();
        }
    }
}
