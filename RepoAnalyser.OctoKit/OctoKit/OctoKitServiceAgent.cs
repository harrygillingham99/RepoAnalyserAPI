using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.Interfaces;
using static RepoAnalyser.Objects.Helpers.OctoKitHelper;

namespace RepoAnalyser.Services.OctoKit
{
    public class OctoKitServiceAgent : IOctoKitServiceAgent
    {
        private readonly IAppCache _cache;
        private readonly GitHubClient _client;

        public OctoKitServiceAgent(IOptions<GitHubSettings> options, IAppCache cache)
        {
            _cache = cache;
            _client = BuildRestClient(options.Value.AppName);
        }

        public Task<IEnumerable<GitHubCommit>> GetCommitsForRepo(long repoId, DateTime repoLastUpdated, string token)
        {
            var commits = new List<Task<GitHubCommit>>();
            _client.Connection.Credentials = GetCredentials(token);

            async Task<IEnumerable<GitHubCommit>> GetCommits()
            {
                var result = _client.Repository.Commit.GetAll(repoId);

                foreach (var commit in await result) commits.Add(_client.Repository.Commit.Get(repoId, commit.Sha));

                return await Task.WhenAll(commits);
            }

            return _cache.GetOrAddAsync($"{repoLastUpdated.ToLongDateString()}-{repoId}-commits", GetCommits);
        }

        public Task<RepoStatistics> GetStatisticsForRepository(long repoId, DateTime repoLastUpdated, string token)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<RepoStatistics> GetStatistics()
            {
                var codeFrequency = _client.Repository.Statistics.GetCodeFrequency(repoId);
                var commitActivity = _client.Repository.Statistics.GetCommitActivity(repoId);
                var participation = _client.Repository.Statistics.GetParticipation(repoId);
                var commitPunchCard = _client.Repository.Statistics.GetPunchCard(repoId);

                return new RepoStatistics
                {
                    CommitActivity = await commitActivity, CodeFrequency = await codeFrequency,
                    Participation = await participation, CommitPunchCard = await commitPunchCard
                };
            }

            return _cache.GetOrAddAsync($"{repoLastUpdated.ToLongDateString()}-{repoId}-stats", GetStatistics);
        }

        public Task<UserActivity> GetDetailedUserActivity(string token)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<UserActivity> GetUserStats()
            {
                var userAccount = await _client.User.Current();
                var notifications = _client.Activity.Notifications.GetAllForCurrent();
                var events = _client.Activity.Events.GetAllUserPerformed(userAccount.Login);

                return new UserActivity {Notifications = await notifications, Events = await events};
            }

            return _cache.GetOrAddAsync($"{token}-stats", GetUserStats, DateTimeOffset.Now.AddDays(1));
        }
    }
}