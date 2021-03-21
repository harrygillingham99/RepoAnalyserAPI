using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoDirectoryResult
    {
        public RepoDirectory DefaultRemoteDir { get; set; }
        public IDictionary<string, RepoDirectory> BranchNameDirMap { get; set; }

        public struct RepoDirectory
        {
            public string Directory { get; set; }
            public string DotNetBuildDirectory { get; set; }
        }
    }
}