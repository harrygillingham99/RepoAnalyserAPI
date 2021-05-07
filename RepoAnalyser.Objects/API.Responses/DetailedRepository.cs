using System;
using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class DetailedRepository
    {
        public UserRepositoryResult Repository { get; set; }
        public IEnumerable<GitHubCommit> Commits { get; set; }
        public RepoStatistics Statistics { get; set; }
        public IDictionary<string, string> CodeOwners { get; set; }
        public DateTime? CodeOwnersLastUpdated { get; set; }

        public bool IsDotNetProject { get; set; }
        public IDictionary<string, int> CyclomaticComplexities { get; set; }
        public DateTime? CyclomaticComplexitiesLastUpdated { get; set; }
        public DateTime? StaticAnalysisLastUpdated { get; set; }
        public string StaticAnalysisHtml { get; set; }
    }
}