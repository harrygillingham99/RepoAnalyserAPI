using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class PullDiscussionResult
    {
        public IEnumerable<IssueComment> Discussion { get; set; }
        public IEnumerable<User> AssignedReviewers { get; set; }
    }
}