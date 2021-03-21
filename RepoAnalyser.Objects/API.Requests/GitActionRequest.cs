using Octokit;

namespace RepoAnalyser.Objects.API.Requests
{
    public class GitActionRequest
    {
        public string RepoUrl { get; set; }
        public string RepoName { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string BranchName { get; set; }
    }
}