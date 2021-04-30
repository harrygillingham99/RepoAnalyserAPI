using System;
using System.Collections.Generic;
using System.Text;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoIssuesResponse
    {
        public IEnumerable<Issue> Issues { get; set; }
    }
}
