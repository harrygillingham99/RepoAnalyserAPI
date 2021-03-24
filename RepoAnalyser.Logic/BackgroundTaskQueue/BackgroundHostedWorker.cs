using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RepoAnalyser.Objects.Attributes;
using Serilog;

namespace RepoAnalyser.Logic.BackgroundTaskQueue
{
    [ScrutorIgnore]
    public class BackgroundHostedWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;

        public BackgroundHostedWorker(IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Background Hosted Service is running.");

            return BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await _taskQueue.DequeueAsync(stoppingToken);

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

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("Background Hosted Service is stopping.");

            return base.StopAsync(stoppingToken);
        }
    }
}