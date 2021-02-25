using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class UserInfoResult
    {
        public User User { get; set; }
        public string LoginRedirectUrl { get; set; }
    }
}
