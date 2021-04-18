using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class DetailedPullRequest
    {
        public UserPullRequestResult PullRequest { get; set; }
        public IEnumerable<GitHubCommit> Commits { get; set; }
        public IEnumerable<string> ModifiedFilePaths { get; set; }
    }
}