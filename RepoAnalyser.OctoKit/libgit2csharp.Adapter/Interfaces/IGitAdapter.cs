using System.Collections.Generic;
using Commit = LibGit2Sharp.Commit;

namespace RepoAnalyser.Services.libgit2csharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        IEnumerable<Commit> GetCommits(string repoUrl, string token, string username, string email);
        string GetOrCloneRepository(string repoUrl, string token, string username, string email);
    }
}