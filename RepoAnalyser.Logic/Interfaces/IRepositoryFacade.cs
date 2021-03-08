using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IRepositoryFacade
    {
        IEnumerable<Commit> GetCommitsForRepo(RepositoryCommitsRequest request, string token);
        Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption);
    }
}