﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.OctoKit.Interfaces
{
    public interface IOctoKitServiceAgent
    {
        Task<IEnumerable<GitHubCommit>> GetCommitsForRepo(long repoId, DateTime repoLastUpdated, string token);
        Task<RepoStatistics> GetStatisticsForRepository(long repoId, DateTime repoLastUpdated, string token);
        Task<UserActivity> GetDetailedUserActivity(string token);
        Task<IDictionary<string, string>> GetFileCodeOwners(string token, IEnumerable<string> filePaths, long repoId,
            DateTime repoLastUpdated);
        Task<IEnumerable<PullRequestCommit>> GetCommitsForPullRequest(long repoId, int pullNumber, string token,
            DateTime pullLastUpdated);
    }
}