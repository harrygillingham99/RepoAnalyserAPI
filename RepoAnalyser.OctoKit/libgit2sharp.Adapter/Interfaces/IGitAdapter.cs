using System.Collections.Generic;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        string CloneOrPullLatestRepository(GitActionRequest request);

        IEnumerable<string> GetRelativeFilePathsForRepository(string repoName, string branchName = null,
            bool ignoreGitFiles = true);

        RepoDirectoryResult GetAllDirectoriesForRepo(string repoName);

        RepoDirectoryResult.RepoDirectory GetRepoDirectory(string repoName, string branchName = null);

        bool IsDotNetProject(string repoName);

    }
}