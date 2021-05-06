using System.Collections.Generic;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        IEnumerable<string> GetRelativeFilePathsForRepository(GitActionRequest request,
            bool ignoreGitFiles = true);

        RepoDirectoryResult GetAllDirectoriesForRepo(GitActionRequest request);

        RepoDirectoryResult.RepoDirectory GetRepoDirectory(GitActionRequest request);

        bool IsDotNetProject(GitActionRequest request);

        string GetSlnName(string repoName, string branchName = null);

        IDictionary<string, AddedRemoved> GetFileLocMetrics(GitActionRequest request);
        IEnumerable<string> GetBuiltAssembliesForRepo(string repoName, string branchName = null);
    }
}