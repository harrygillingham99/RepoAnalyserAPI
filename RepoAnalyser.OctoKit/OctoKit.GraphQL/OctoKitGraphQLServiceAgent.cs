using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using static RepoAnalyser.Objects.Helpers.OctoKitHelper;

namespace RepoAnalyser.Services.OctoKit.GraphQL
{
    public class OctoKitGraphQlServiceAgent : IOctoKitGraphQlServiceAgent
    {
        private readonly ProductHeaderValue _productHeaderValue;

        public OctoKitGraphQlServiceAgent(IOptions<GitHubSettings> options)
        {
            _productHeaderValue = new ProductHeaderValue(options.Value.AppName);
        }

        public async Task<UserRepositoryResult> GetRepository(string token, long repoId) =>
            (await GetRepositories(token, RepoFilterOptions.All)).FirstOrDefault(x => x.Id == repoId);
      
        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions option)
        {
            var query = new Query().Viewer
                .Repositories(100, affiliations: BuildRepositoryScopes(option)).Nodes
                .Select(repository => new UserRepositoryResult
                {
                    Id = repository.DatabaseId.Value,
                    Description = repository.Description,
                    Name = repository.Name,
                    PullUrl = repository.Url,
                    Private = repository.IsPrivate,
                    Template = repository.IsTemplate,
                    Collaborators = repository.Collaborators(null, null, null, null, null, null)
                        .Select(conn => conn.Nodes)
                        .Select(user => new Collaborator(user.Name, user.AvatarUrl(128))).ToList(),
                    LastUpdated = repository.UpdatedAt.DateTime
                }).Compile();
            return BuildConnectionExecuteQuery(token, query);
        }

        private Task<T> BuildConnectionExecuteQuery<T>(string token, ICompiledQuery<T> query,
            Dictionary<string, object> variables = null)
        {
            var connection = BuildConnection(_productHeaderValue, token);
            return connection.Run(query, variables);
        }
    }
}