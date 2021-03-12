using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IRepositoryFacade
    {
        Task<DetailedRepository> GetDetailedRepository(long repoId, string token);
        Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption);
        Task<IDictionary<string, string>> GetRepositoryCodeOwners(long repoId, string token);
    }
}