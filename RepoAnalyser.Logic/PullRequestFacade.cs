using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;

namespace RepoAnalyser.Logic
{
    public class PullRequestFacade : IPullRequestFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;

        public PullRequestFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetPullRequests(token, filterOption);
        }

        public Task<DetailedPullRequest> GetDetailedPullRequest(string token, in long repoId, in int pullNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}
