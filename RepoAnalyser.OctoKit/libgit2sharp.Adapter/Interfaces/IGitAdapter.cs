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

        RepoDirectoryResult.RepoDirectory GetRepoDirectory(string repoName, string branchName = null);

        bool IsDotNetProject(string repoName);

    }
}