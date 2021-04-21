using System;
using System.Collections.Generic;
using System.Text;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserLandingPageStatistics
    {
        public IEnumerable<Activity> Events { get; set; }
        public IDictionary<string, CommitActivity> TopRepoActivity { get; set; }

    }
}
