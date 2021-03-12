using System;
using System.Collections.Generic;
using Octokit.GraphQL.Model;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserPullRequestResult
    {
        public long RepositoryId { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        public bool Closed { get; set; }

        public PullRequestState State { get; set; }

        public IEnumerable<string> Collaborators { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}