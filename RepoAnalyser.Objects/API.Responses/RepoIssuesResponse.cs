using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoIssuesResponse
    {
        public IEnumerable<Issue> Issues { get; set; }
    }
}
