using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IRepositoryFacade
    {
        Task<DetailedRepository> GetDetailedRepository(long repoId, string token);
        Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption);
        Task<IDictionary<string, string>> GetRepositoryCodeOwners(long repoId, string connectionId, string token);
        Task<IEnumerable<GitHubCommit>> GetFileInformation(long repoId, string token, string fileName);
        Task<IDictionary<string, int>> GetCyclomaticComplexities(string connectionId, string token, CyclomaticComplexityRequest request);
        Task<RepoIssuesResponse> GetRepoIssues(long repoId, string token);
        Task<RepoSummaryResponse> GetRepoSummary(long repoId, string token);
        Task<RepoContributionResponse> GetRepoContributionVolumes(long repoId, string token, string connectionId);
    }
}