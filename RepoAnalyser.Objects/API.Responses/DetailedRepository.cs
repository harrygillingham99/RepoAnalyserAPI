using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class DetailedRepository
    {
        public UserRepositoryResult Repository { get; set; }
        public IEnumerable<GitHubCommit> Commits { get; set; }
        public RepoStatistics Statistics { get; set; }
    }
}