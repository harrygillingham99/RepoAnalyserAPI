namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoSummaryResponse
    {
        public double OwnershipPercentage { get; set; }
        public int LocContributed { get; set; }
        public int LocRemoved { get; set; }
        public double AverageCyclomaticComplexity { get; set; }
        public int TotalIssues { get; set; }
        public int IssuesRaised { get; set; }
        public int IssuesSolved { get; set; }
        public int AnalysisIssues { get; set; }
    }
}
