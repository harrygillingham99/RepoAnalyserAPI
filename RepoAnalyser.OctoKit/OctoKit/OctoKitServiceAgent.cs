using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Config;
using RepoAnalyser.Services.OctoKit.Interfaces;
using static RepoAnalyser.Objects.Constants.CacheConstants;
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
                var result = _client.Repository.Commit.GetAll(repoId);

                var commits = (await result).Select(commit => _client.Repository.Commit.Get(repoId, commit.Sha));

                return await Task.WhenAll(commits);
            }

            return _cache.GetOrAddAsync($"{repoLastUpdated}-{repoId}-commits", GetCommits,
                DefaultSlidingCacheExpiry);
        }

        public Task<UserLandingPageStatistics> GetLandingPageStatistics(string token)
        {
            _client.Connection.Credentials = GetCredentials(token);

            var statsForRepos = new Dictionary<string, CommitActivity>();
            var languages = new List<(string language, long bytes)>();

            async Task<UserLandingPageStatistics> GetStats()
            {
                var repos = await _client.Repository.GetAllForCurrent();

                var iterator = 0;

                foreach (var repo in repos.OrderByDescending(repository => repository.UpdatedAt.DateTime))
                {
                    if (iterator <= 2)
                        statsForRepos.Add(repo.Name, await _client.Repository.Statistics.GetCommitActivity(repo.Id));

                    var repoLanguages = _client.Repository.GetAllLanguages(repo.Id);

                    foreach (var lang in await repoLanguages)
                    {
                        var existingIndex = languages.FindIndex(langStat => langStat.language == lang.Name);

                        if (existingIndex >= 0)
                        {
                            var itemToUpdate = languages[existingIndex];
                            itemToUpdate.bytes += lang.NumberOfBytes;
                            languages[existingIndex] = itemToUpdate;
                        }
                        else
                        {
                            languages.Add((lang.Name, lang.NumberOfBytes));
                        }
                    }

                    iterator++;
                }

                var totalBytes = languages.Sum(x => x.bytes);

                long GetPercentageLanguageUsage(long langBytes)
                {
                    var result = (long) Math.Round((double) (100 * langBytes) / totalBytes);
                    if (result == 0) result = 1;
                    return result;
                }

                return new UserLandingPageStatistics
                {
                    TopRepoActivity = statsForRepos,
                    Languages = languages.ToDictionary(key => key.language,
                        val => GetPercentageLanguageUsage(val.bytes))
                };
            }

            return _cache.GetOrAddAsync($"{token}-landingStats", GetStats, DefaultSlidingCacheExpiry);
        }

        public Task<GitHubCommit> GetDetailedCommit(string token, long repoId, string sha)
        {
            _client.Connection.Credentials = GetCredentials(token);

            Task<GitHubCommit> DetailedCommit()
            {
                return _client.Repository.Commit.Get(repoId, sha);
            }

            return _cache.GetOrAddAsync($"commit-{sha}-{repoId}", DetailedCommit,
                DefaultSlidingCacheExpiry);
        }

        public Task<IEnumerable<PullRequestCommit>> GetCommitsForPullRequest(long repoId, int pullNumber, string token,
            DateTime pullLastUpdated)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<IEnumerable<PullRequestCommit>> GetCommits()
            {
                return (await _client.PullRequest.Commits(repoId, pullNumber)).ToList();
            }

            return _cache.GetOrAddAsync($"{repoId}-{pullNumber}-{pullLastUpdated}-pullCommits", GetCommits,
                DefaultSlidingCacheExpiry);
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

            return _cache.GetOrAddAsync($"{repoLastUpdated}-{repoId}-stats", GetStatistics,
                DefaultSlidingCacheExpiry);
        }

        public Task<UserActivity> GetDetailedUserActivity(string token, PaginationOptions pageOptions)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<UserActivity> GetUserStats()
            {
                var userAccount = await _client.User.Current();
                var notifications = _client.Activity.Notifications.GetAllForCurrent();
                var events = _client.Activity.Events.GetAllUserPerformed(userAccount.Login, new ApiOptions
                {
                    PageCount = 1,
                    PageSize = pageOptions.PageSize,
                    StartPage = pageOptions.Page
                });

                return new UserActivity {Notifications = await notifications, Events = await events};
            }

            return _cache.GetOrAddAsync($"{token}-{pageOptions.Page}-{pageOptions.PageSize}-stats", GetUserStats,
                DefaultSlidingCacheExpiry);
        }

        public Task<IDictionary<string, string>> GetFileCodeOwners(string token, IEnumerable<string> filePaths,
            long repoId, DateTime repoLastUpdated)
        {
            _client.Connection.Credentials = GetCredentials(token);

            var filePathsList = filePaths.ToList();

            async Task<IDictionary<string, string>> GetCodeOwners()
            {
                var commitsForFiles = new Dictionary<string, IReadOnlyList<GitHubCommit>>();

                foreach (var path in filePathsList)
                    commitsForFiles.Add(path,
                        (await _cache.GetOrAddAsync($"{repoId}-{path}-{repoLastUpdated}-fileCommits",
                            () => GetCommitsForFile(repoId, path), DefaultSlidingCacheExpiry)).ToList());

                return commitsForFiles.ToDictionary(key => key.Key,
                    value => value.Value.GroupBy(x => x?.Author?.Login ?? "Unknown")
                        .Select(x => new {x.Key, Count = x.Count()})
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault()
                        ?.Key);
            }

            return GetCodeOwners();
        }

        public Task<IEnumerable<GitHubCommit>> GetFileCommits(long repoId, string token, string filePath,
            DateTime repoLastUpdated)
        {
            _client.Connection.Credentials = GetCredentials(token);

            return _cache.GetOrAddAsync($"{repoId}-{filePath}-{repoLastUpdated}-fileCommits",
                () => GetCommitsForFile(repoId, filePath),
                DefaultSlidingCacheExpiry);
        }

        public Task<PullDiscussionResult> GetPullReviewInformation(string token, long repoId, int pullNumber,
            DateTime updatedAtDateTime)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<PullDiscussionResult> GetPullReviewInfo()
            {
                var reviewComments = await _client.Issue.Comment.GetAllForIssue(repoId, pullNumber);
                var reviewers = await _client.PullRequest.ReviewRequest.Get(repoId, pullNumber);
                return new PullDiscussionResult
                {
                    Discussion = reviewComments,
                    AssignedReviewers = reviewers.Users.ToList()
                };
            }

            return _cache.GetOrAddAsync($"{token}-{repoId}-{pullNumber}-{updatedAtDateTime}-pullDiscussion",
                GetPullReviewInfo, DefaultSlidingCacheExpiry);
        }

        public Task<IEnumerable<Issue>> GetIssuesForRepo(string token, long repoId)
        {
            _client.Connection.Credentials = GetCredentials(token);

            async Task<IEnumerable<Issue>> GetIssues()
            {
                return await _client.Issue.GetAllForRepository(repoId);
            }

            return _cache.GetOrAddAsync($"{token}-{repoId}-issues", GetIssues,
                DefaultSlidingCacheExpiry);
        }

        private async Task<IEnumerable<GitHubCommit>> GetCommitsForFile(long repoId, string filePath)
        {
            var commits = new List<Task<GitHubCommit>>();

            var result = _client.Repository.Commit.GetAll(repoId, new CommitRequest
            {
                Path = filePath
            });

            foreach (var commit in await result) commits.Add(_client.Repository.Commit.Get(repoId, commit.Sha));

            return await Task.WhenAll(commits);
        }
    }
}