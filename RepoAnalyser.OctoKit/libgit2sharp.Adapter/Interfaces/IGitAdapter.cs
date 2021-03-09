using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces
{
    public interface IGitAdapter
    {
        string CloneOrPullLatestRepository(GitActionRequest request);
    }
}