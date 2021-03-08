using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IGitAdapter _gitAdapter;
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;

        public RepositoryFacade(IGitAdapter gitAdapter, IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent)
        {
            _gitAdapter = gitAdapter;
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
        }

        public IEnumerable<Commit> GetCommitsForRepo(RepositoryCommitsRequest request, string token)
        {
            return _gitAdapter.GetCommits(new GitActionRequest(request, token));
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetRepositories(token, filterOption);
        }
    }
}
