using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Requests
{
    public class GendarmeAnalyisRequest
    {
        public string RepoName { get; set; }
        public IEnumerable<string> PathToAssemblies { get; set; }
        public string RepoBuildPath { get; set; }
    }
}