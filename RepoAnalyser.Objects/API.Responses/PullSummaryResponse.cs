namespace RepoAnalyser.Objects.API.Responses
{
    public class PullSummaryResponse
    {
        public bool IsReviewer { get; set; }
        public int LocAdded { get; set; }
        public int LocRemoved { get; set; }
    }
}
