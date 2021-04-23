using System;
using System.Collections.Generic;
using Octokit.GraphQL.Model;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserPullRequestResult
    {
        public long RepositoryId { get; set; }
        public string RepositoryName { get; set; }
        public int PullRequestNumber { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public string HeadBranchName { get; set; }
        public int ChangedFiles { get; set; }

        public bool Closed { get; set; }


        public PullRequestState State { get; set; }

        public IEnumerable<string> Collaborators { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DescriptionMarkdown { get; set; }
    }
}