using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.SignalR.Objects
{
    [NSwagInclude]
    public enum SignalRNotificationType
    {
        RepoAnalysisProgressUpdate = 1,
        PullRequestAnalysisProgressUpdate = 2,
        RepoAnalysisDone = 3,
        PullRequestAnalysisDone = 4
    }
}