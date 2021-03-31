using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using static RepoAnalyser.Objects.Helpers.OctoKitHelper;

namespace RepoAnalyser.Services.OctoKit.GraphQL
{
    public class OctoKitGraphQlServiceAgent : IOctoKitGraphQlServiceAgent
    {
        private readonly IAppCache _cache;
        private readonly ProductHeaderValue _productHeaderValue;

        public OctoKitGraphQlServiceAgent(IOptions<GitHubSettings> options, IAppCache appCache)
        {
            _cache = appCache;
            _productHeaderValue = new ProductHeaderValue(options.Value.AppName);
        }

        public async Task<UserRepositoryResult> GetRepository(string token, long repoId)
        {
            return (await GetRepositories(token, RepoFilterOptions.All)).FirstOrDefault(x => x.Id == repoId);
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions option)
        {
            var query = new Query().Viewer
                .Repositories(100, affiliations: BuildRepositoryScopes(option)).Nodes
                .Select(repository => new UserRepositoryResult
                {
                    Id = repository.DatabaseId.Value,
                    Description = repository.Description,
                    DescriptionHtml = repository.DescriptionHTML,
                    Name = repository.Name,
                    PullUrl = repository.Url,
                    Private = repository.IsPrivate,
                    Template = repository.IsTemplate,
                    Collaborators = repository.Collaborators(null, null, null, null, null, null)
                        .Select(conn => conn.Nodes)
                        .Select(user => new Collaborator(user.Name, user.AvatarUrl(128))).ToList(),
                    LastUpdated = repository.UpdatedAt.DateTime
                }).Compile();

            return _cache.GetOrAddAsync($"{token}-{option}-repos", () => BuildConnectionExecuteQuery(token, query),
                CacheConstants.DefaultSlidingCacheExpiry);
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption option)
        {
            var query = new Query().Viewer.PullRequests(100, states: BuildPullRequestScopes(option)).Nodes
                .Select(pull => new UserPullRequestResult
                {
                    RepositoryId = pull.Repository.DatabaseId.Value,
                    RepositoryName = pull.Repository.Name,
                    PullRequestNumber = pull.Number,
                    ClosedAt = pull.ClosedAt,
                    Closed = pull.Closed,
                    Title = pull.Title,
                    UpdatedAt = pull.UpdatedAt,
                    Additions = pull.Additions,
                    Deletions = pull.Deletions,
                    ChangedFiles = pull.ChangedFiles,
                    Description = pull.BodyText,
                    DescriptionMarkdown = pull.Body,
                    State = pull.State,
                    Collaborators = pull.Participants(100, null, null, null).Nodes
                        .Select(x => x.Login).ToList()
                }).Compile();

            return _cache.GetOrAddAsync($"{token}-{option}-pulls", () => BuildConnectionExecuteQuery(token, query),
                CacheConstants.DefaultSlidingCacheExpiry);
        }

        public async Task<UserPullRequestResult> GetPullRequest(string token, long repoId, int pullNumber)
        {
            bool MatchingPullRequest(UserPullRequestResult pull)
            {
                return pull.PullRequestNumber == pullNumber && pull.RepositoryId == repoId;
            }

            return (await GetPullRequests(token, PullRequestFilterOption.All)).FirstOrDefault(MatchingPullRequest);
        }

        private Task<T> BuildConnectionExecuteQuery<T>(string token, ICompiledQuery<T> query,
            Dictionary<string, object> variables = null)
        {
            var connection = BuildConnection(_productHeaderValue, token);
            return connection.Run(query, variables);
        }
    }
}