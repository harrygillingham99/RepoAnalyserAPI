using System.Collections.Generic;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        string CloneOrPullLatestRepository(GitActionRequest request);

        IEnumerable<string> GetRelativeFilePathsForRepository(string repoDirectory, string repoName);

    }
}