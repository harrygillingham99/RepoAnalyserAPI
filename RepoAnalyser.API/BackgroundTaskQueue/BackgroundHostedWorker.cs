﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RepoAnalyser.Objects.Attributes;
using Serilog;

namespace RepoAnalyser.API.BackgroundTaskQueue
{
    [ScrutorIgnore]
    public class BackgroundHostedWorker : BackgroundService
    {

        public BackgroundHostedWorker(IBackgroundTaskQueue taskQueue)
        {
            TaskQueue = taskQueue;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information(
                "Background Hosted Service is running.");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex,
                        $"Error occurred executing {workItem.GetType().Name}.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("Background Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}