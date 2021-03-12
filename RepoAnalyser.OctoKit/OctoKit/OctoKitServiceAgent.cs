using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Constants;
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
            _client.Connection.Credentials = GetCredentials(token);

            async Task<IEnumerable<GitHubCommit>> GetCommits()
            {
                var commits = new List<Task<GitHubCommit>>();

                var result = _client.Repository.Commit.GetAll(repoId);

                foreach (var commit in await result) commits.Add(_client.Repository.Commit.Get(repoId, commit.Sha));

                return await Task.WhenAll(commits);
            }

            return _cache.GetOrAddAsync($"{repoLastUpdated}-{repoId}-commits", GetCommits);
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

            return _cache.GetOrAddAsync($"{repoLastUpdated}-{repoId}-stats", GetStatistics);
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

            return _cache.GetOrAddAsync($"{token}-stats", GetUserStats, CacheConstants.DefaultCacheExpiry);
        }

        public Task<IDictionary<string,string>> GetFileCodeOwners(string token, IEnumerable<string> filePaths, long repoId, DateTime repoLastUpdated)
        {
            _client.Connection.Credentials = GetCredentials(token);

            var filePathsList = filePaths.ToList();

            async Task<IReadOnlyList<GitHubCommit>> GetCommitsForFile (string path) =>
                await _client.Repository.Commit.GetAll(repoId, new CommitRequest
                {
                    Path = path,
                });

            async Task<IDictionary<string, string>> GetCodeOwners()
            {
                var commitsForFiles = new Dictionary<string, IReadOnlyList<GitHubCommit>>();
                foreach (var path in filePathsList)
                    commitsForFiles.Add(path, await _cache.GetOrAddAsync($"{repoId}-{path}-{repoLastUpdated}-fileCommits", () => GetCommitsForFile(path)));

                return commitsForFiles.ToDictionary(key => key.Key,
                    value => value.Value.GroupBy(x => x.Author.Login)
                        .Select(x => new {x.Key, Count = x.Count()})
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault()
                        ?.Key);
            }

            return GetCodeOwners();
        }
    }
}