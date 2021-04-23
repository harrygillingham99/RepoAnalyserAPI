using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects.API.Requests
{
    public class CyclomaticComplexityRequest
    {
        public IEnumerable<string> FilesToSearch { get; set; }
        public long RepoId { get; set; }
        public int? PullRequestNumber { get; set; }
    }
}
