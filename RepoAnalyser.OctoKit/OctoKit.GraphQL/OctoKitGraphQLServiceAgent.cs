﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
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
        private readonly IAppCache _cache;

        public OctoKitGraphQlServiceAgent(IOptions<GitHubSettings> options, IAppCache appCache)
        {
            _cache = appCache;
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

            return _cache.GetOrAddAsync($"{token}-{option}-repos", () => BuildConnectionExecuteQuery(token, query), DateTimeOffset.Now.AddHours(1));
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption option)
        {
            var query = new Query().Viewer.PullRequests(100, states: BuildPullRequestScopes(option)).Nodes.Select(pull => new UserPullRequestResult
            {
                RepositoryId = pull.Repository.DatabaseId.Value,
                ClosedAt = pull.ClosedAt,
                Closed = pull.Closed,
                Title = pull.Title,
                Description = pull.BodyText,
                State = pull.State,
                Collaborators = pull.Participants(100,null,null,null).Nodes.Select(x => x.Login).ToList()
            }).Compile();

            return _cache.GetOrAddAsync($"{token}-{option}-pulls", () => BuildConnectionExecuteQuery(token, query), DateTimeOffset.Now.AddHours(1));
        }

        private Task<T> BuildConnectionExecuteQuery<T>(string token, ICompiledQuery<T> query,
            Dictionary<string, object> variables = null)
        {
            var connection = BuildConnection(_productHeaderValue, token);
            return connection.Run(query, variables);
        }
    }

    public class UserPullRequestResult
    {
        public long RepositoryId { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        public bool Closed { get; set; }

        public PullRequestState State { get; set; }

        public IEnumerable<string> Collaborators { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}