using Octokit;

namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoStatistics
    {
        public CodeFrequency CodeFrequency { get; set; }
        public CommitActivity CommitActivity { get; set; }
        public Participation Participation { get; set; }
        public PunchCard CommitPunchCard { get; set; }
    }
}