using Octokit;

namespace RepoAnalyser.Objects.API.Requests
{
    public class GitActionRequest
    {
        public GitActionRequest(string repoUrl, string token, string username, string email, string repoName)
        {
            RepoUrl = repoUrl;
            Token = token;
            RepoName = repoName;
            Username = username;
            Email = email;
        }

        public GitActionRequest(string repoUrl, string token, Account user)
        {
            RepoUrl = repoUrl;
            Token = token;
            Email = user.Email;
            Username = user.Login;
        }

        public string RepoUrl { get; }
        public string RepoName { get; }
        public string Token { get; }
        public string Username { get; }
        public string Email { get; }
    }
}