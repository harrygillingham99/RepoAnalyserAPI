using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
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

        public Task<IEnumerable<Repo>> GetRepositories(string token, RepoFilterOptions option)
        {
            var query = new Query().Viewer
                .Repositories(100, affiliations: BuildRepositoryScopes(option)).Nodes
                .Select(x => new Repo
                {
                    Description = x.Description,
                    Name = x.Name,

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

    public class Repo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}