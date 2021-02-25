using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using RepoAnalyser.Objects;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using static RepoAnalyser.Objects.Constants.GitHubConstants;

namespace RepoAnalyser.Services.OctoKit.GraphQL
{
    public class OctoKitGraphQLServiceAgent : IOctoKitGraphQLServiceAgent
    {
        private readonly ProductHeaderValue _productHeaderValue;

        public OctoKitGraphQLServiceAgent(IOptions<GitHubSettings> options)
        {
            _productHeaderValue = new ProductHeaderValue(options.Value.AppName);
        }

        public Task<IEnumerable<Repo>> GetRepositories(string token)
        {
            var query = new Query().Viewer
                .Repositories(100, affiliations: RepositoryScopes).Nodes
                .Select(x => new Repo
                {
                    Description = x.Description, Name = x.Name
                }).Compile();
            return BuildConnectionExecuteQuery(token, query);
        }

        private Task<T> BuildConnectionExecuteQuery<T>(string token, ICompiledQuery<T> query,
            Dictionary<string, object> variables = null)
        {
            var connection = new Connection(_productHeaderValue, token);
            return connection.Run(query, variables);
        }
    }

    public class Repo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}