using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Services.libgit2csharp.Adapter.Interfaces;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IGitAdapter _gitAdapter;

        public RepositoryFacade(IGitAdapter gitAdapter)
        {
            _gitAdapter = gitAdapter;
        }

        public Task<IEnumerable<Commit>> GetCommitsForRepo(RepositoryCommitsRequest request, string token)
        {
            return Task.FromResult(
                _gitAdapter.GetCommits(request.RepositoryUrl, token, request.Username, request.Email));
        }
    }

    public interface IRepositoryFacade
    {
        Task<IEnumerable<Commit>> GetCommitsForRepo(RepositoryCommitsRequest request, string token);
    }
}
