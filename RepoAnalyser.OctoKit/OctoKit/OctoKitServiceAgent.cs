using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly GitHubClient _client;

        public OctoKitServiceAgent(IOptions<GitHubSettings> options)
        {
            _client = BuildRestClient(options.Value.AppName);
        }

        public async Task<IEnumerable<GitHubCommit>> GetCommitsForRepo(long repoId, string token)
        {
            List<Task<GitHubCommit>> commits = new List<Task<GitHubCommit>>();

            _client.Connection.Credentials = GetCredentials(token);

            var result = await _client.Repository.Commit.GetAll(repoId);

            foreach (var commit in result)
            {
                commits.Add(_client.Repository.Commit.Get(repoId, commit.Sha));
            }

            return await Task.WhenAll(commits);
        }

        public async Task<RepoStatistics> GetStatisticsForRepository(long repoId, string token)
        {
            _client.Connection.Credentials = GetCredentials(token);

            var codeFrequency = _client.Repository.Statistics.GetCodeFrequency(repoId);
            var commitActivity= _client.Repository.Statistics.GetCommitActivity(repoId);
            var participation= _client.Repository.Statistics.GetParticipation(repoId);
            var commitPunchCard= _client.Repository.Statistics.GetPunchCard(repoId);

            return new RepoStatistics
            {
                CommitActivity = await commitActivity,
                CodeFrequency = await codeFrequency,
                Participation = await participation,
                CommitPunchCard = await commitPunchCard
            };
        }

        public async Task<UserActivity> GetDetailedUserActivity(string token)
        {
            _client.Connection.Credentials = GetCredentials(token);

            var userLogin = (await _client.User.Current()).Login;

            var notifications = _client.Activity.Notifications.GetAllForCurrent();
            var events = _client.Activity.Events.GetAllUserPerformed(userLogin);

            return new UserActivity
            {
                Notifications = await notifications,
                Events = await events
            };
        }
    }
}
