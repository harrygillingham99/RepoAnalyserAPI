using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;

        public RepositoryFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
        }

        public async Task<DetailedRepository> GetDetailedRepository(long repoId, string token)
        {
            var repository = _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var commits = _octoKitServiceAgent.GetCommitsForRepo(repoId, token);
            var repoStats = _octoKitServiceAgent.GetStatisticsForRepository(repoId, token);

            return new DetailedRepository
            {
                Repository = await repository,
                Commits = await commits,
                Statistics = await repoStats
            };
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetRepositories(token, filterOption);
        }
    }
}