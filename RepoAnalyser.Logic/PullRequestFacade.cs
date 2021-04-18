using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class PullRequestFacade : IPullRequestFacade
    {
        private readonly IGitAdapter _gitAdapter;
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;

        public PullRequestFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent, IGitAdapter gitAdapter,
            IOctoKitAuthServiceAgent octoKitAuthServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
            _gitAdapter = gitAdapter;
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token,
            PullRequestFilterOption filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetPullRequests(token, filterOption);
        }

        public async Task<DetailedPullRequest> GetDetailedPullRequest(string token, long repoId, int pullNumber)
        {
            var detailedCommits = new List<GitHubCommit>();
            var pullRequest = await _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var repo = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var repoRelativePaths = _gitAdapter.GetRelativeFilePathsForRepository(new GitActionRequest
            {
                RepoUrl = repo.PullUrl,
                RepoName = repo.Name,
                Token = token,
                Username = user.Login,
                Email = user.Email ?? "test@RepoAnalyser.com"
            });
            var commits = (await _octoKitServiceAgent.GetCommitsForPullRequest(repoId, pullNumber, token,
                pullRequest.UpdatedAt?.DateTime ?? DateTime.Now)).ToList();

            foreach (var commit in commits)
                detailedCommits.Add(await _octoKitServiceAgent.GetDetailedCommit(token, repoId, commit.Sha));

            var filesModifiedInPull = detailedCommits
                .SelectMany(commit => commit?.Files ?? new List<GitHubCommitFile>())
                .Select(file => file.Filename ?? "Untracked File").Distinct();

            return new DetailedPullRequest
            {
                PullRequest = pullRequest,
                Commits = detailedCommits,
                ModifiedFilePaths = repoRelativePaths.Where(path => filesModifiedInPull.Any(path.Contains))
            };
        }
    }
}