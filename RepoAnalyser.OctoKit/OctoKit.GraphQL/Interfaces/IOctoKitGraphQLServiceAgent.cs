using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.Services.OctoKit.GraphQL.Interfaces
{
    public interface IOctoKitGraphQlServiceAgent
    {
        Task<IEnumerable<Repo>> GetRepositories(string token, RepoFilterOptions option);
    }
}