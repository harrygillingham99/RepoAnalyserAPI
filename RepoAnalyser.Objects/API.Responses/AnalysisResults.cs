using System;

namespace RepoAnalyser.Objects.API.Responses
{
    public class AnalysisResults
    {
        public long RepoId { get; set; }
        public string RepoName { get; set; }
        public DateTime? CodeOwnersLastRunDate { get; set; }
        public DateTime? CyclomaticComplexitiesLastUpdated { get; set; }
        public DateTime? StaticAnalysisLastUpdated { get; set; }
    }
}