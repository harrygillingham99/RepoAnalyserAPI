using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class PullRequestFacade : IPullRequestFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;

        public PullRequestFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent, IOctoKitServiceAgent octoKitServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetPullRequests(token, filterOption);
        }

        public async Task<DetailedPullRequest> GetDetailedPullRequest(string token, long repoId, int pullNumber)
        {
            var pullRequest = await _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var commits = _octoKitServiceAgent.GetCommitsForPullRequest(repoId, pullNumber, token,
                pullRequest.UpdatedAt?.DateTime ?? DateTime.Now);

            return new DetailedPullRequest
            {
                PullRequest = pullRequest,
                Commits = await commits
            };
        }
    }
}