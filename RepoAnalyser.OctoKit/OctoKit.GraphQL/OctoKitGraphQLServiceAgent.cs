using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using RepoAnalyser.Objects;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;

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
            var query = new Query().Viewer.Repositories().AllPages()
                .Select(x => new Repo
                {
                    Description = x.Description, Name = x.Name

                }).Compile();
            return BuildConnectionAndQuery(token, query);

        }

        private Task<T> BuildConnectionAndQuery<T>(string token, ICompiledQuery<T> query)
        {
            var connection = new Connection(_productHeaderValue, token);
            return connection.Run(query);
        } 
    }

    public class Repo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
