using System.Collections.Generic;
using RepoAnalyser.Objects.API.Requests;
using Commit = LibGit2Sharp.Commit;

namespace RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        IEnumerable<Commit> GetCommits(GitActionRequest request);
        string CloneOrPullLatestRepository(GitActionRequest request);
    }
}