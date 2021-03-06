namespace RepoAnalyser.Objects.API.Requests
{
    public class RepositoryCommitsRequest
    {
        public string RepositoryUrl { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}