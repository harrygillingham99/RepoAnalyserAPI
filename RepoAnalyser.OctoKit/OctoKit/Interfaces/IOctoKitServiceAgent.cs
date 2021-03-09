using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.OctoKit.Interfaces
{
    public interface IOctoKitServiceAgent
    {
        Task<IEnumerable<GitHubCommit>> GetCommitsForRepo(long repoId, string token);
        Task<RepoStatistics> GetStatisticsForRepository(long repoId, string token);
        Task<UserActivity> GetDetailedUserActivity(string token);
    }
}