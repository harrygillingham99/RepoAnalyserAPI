using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.OctoKit.GraphQL.Interfaces
{
    public interface IOctoKitGraphQlServiceAgent
    {
        Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions option);
    }
}