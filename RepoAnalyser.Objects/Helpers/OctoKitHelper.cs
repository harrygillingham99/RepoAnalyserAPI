using System.Collections.Generic;
using Octokit;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.Exceptions;
using Connection = Octokit.GraphQL.Connection;
using ProductHeaderValue = Octokit.GraphQL.ProductHeaderValue;
using RepositoryAffiliation = Octokit.GraphQL.Model.RepositoryAffiliation;

namespace RepoAnalyser.Objects.Helpers
{
    public static class OctoKitHelper
    {
        public static Credentials GetCredentials(string token)
        {
            return new Credentials(token);
        }

        public static GitHubClient BuildRestClient(string appName)
        {
            return new GitHubClient(new Octokit.ProductHeaderValue(appName));
        }

        public static Connection BuildConnection(ProductHeaderValue product, string token)
        {
            return new Connection(product, token);
        }

        public static Arg<IEnumerable<RepositoryAffiliation?>> BuildRepositoryScopes(RepoFilterOptions options)
        {
            List<RepositoryAffiliation?> scopes = new List<RepositoryAffiliation?>();
            switch (options)
            {
                case RepoFilterOptions.All:
                    scopes.AddRange(new List<RepositoryAffiliation?>{RepositoryAffiliation.Owner, RepositoryAffiliation.Collaborator});
                    break;
                case RepoFilterOptions.Owned:
                    scopes.Add(RepositoryAffiliation.Owner);
                    break;
                case RepoFilterOptions.ContributedNotOwned:
                    scopes.Add(RepositoryAffiliation.Collaborator);
                    break;
                default:
                    throw new BadRequestException("Supplied filtering option is invalid");
            }
            return new Arg<IEnumerable<RepositoryAffiliation?>>(scopes);
        }

        public static Arg<IEnumerable<PullRequestState>>? BuildPullRequestScopes(PullRequestFilterOption options)
        {
            List<PullRequestState> scopes = new List<PullRequestState>();
            switch (options)
            {
                case PullRequestFilterOption.All:
                    scopes.AddRange(new List<PullRequestState> {PullRequestState.Closed, PullRequestState.Merged, PullRequestState.Open});
                    break;
                case PullRequestFilterOption.Closed:
                    scopes.Add(PullRequestState.Closed);
                    break;
                case PullRequestFilterOption.Merged:
                    scopes.Add(PullRequestState.Merged);
                    break;
                case PullRequestFilterOption.Open:
                    scopes.Add(PullRequestState.Open);
                    break;
                default:
                    throw new BadRequestException("Supplied filtering option is invalid");
            }
            return new Arg<IEnumerable<PullRequestState>>(scopes);
        }
    }
}