using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        private readonly IGitAdapter _gitAdapter;

        public RepositoryFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent, IGitAdapter gitAdapter, IOctoKitAuthServiceAgent octoKitAuthServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
            _gitAdapter = gitAdapter;
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
        }

        public async Task<DetailedRepository> GetDetailedRepository(long repoId, string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var commits = _octoKitServiceAgent.GetCommitsForRepo(repoId,repository.LastUpdated, token);
            var repoStats = _octoKitServiceAgent.GetStatisticsForRepository(repoId,repository.LastUpdated ,token);

            return new DetailedRepository
            {
                Repository = repository,
                Commits = await commits,
                Statistics = await repoStats
            };
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetRepositories(token, filterOption);
        }

        public async Task<IDictionary<string,string>> GetRepositoryCodeOwners(long repoId, string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            _ = _gitAdapter.CloneOrPullLatestRepository(new GitActionRequest
            {
                BranchName = null,
                Email = user.Email ?? "unknown@RepoAnalyser.test",
                RepoName = repository.Name,
                RepoUrl = repository.PullUrl,
                Token = token,
                Username = user.Login
            });
            var filesInRepo = _gitAdapter.GetRelativeFilePathsForRepository(repository.Name);

            /* example of invoking a build for a .NET project
             var repoDir = _gitAdapter.GetRepoDirectory(repository.Name);
            _buildRunner.Build(repoDir.Directory, repoDir.DotNetBuildDirectory);*/

            return await _octoKitServiceAgent.GetFileCodeOwners(token, filesInRepo, repository.Id, repository.LastUpdated);
        }
    }
}