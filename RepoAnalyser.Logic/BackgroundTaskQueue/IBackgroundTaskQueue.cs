﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace RepoAnalyser.Logic.BackgroundTaskQueue
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(
            Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);

    }
}