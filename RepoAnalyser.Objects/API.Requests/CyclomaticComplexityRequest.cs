using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Requests
{
    public class CyclomaticComplexityRequest
    {
        public IEnumerable<string> FilesToSearch { get; set; }
        public long RepoId { get; set; }
        public int? PullRequestNumber { get; set; }
    }
}
