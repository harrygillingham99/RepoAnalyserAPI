using System.Collections.Generic;
using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class PullDiscussionResult
    {
        public IEnumerable<PullRequestReviewComment> Discussion { get; set; }
    }
}