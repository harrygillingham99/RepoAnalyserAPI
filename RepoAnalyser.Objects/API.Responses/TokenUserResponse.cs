using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class TokenUserResponse
    {
        public string AccessToken { get; set; }

        public User User { get; set; }
    }
}
