using System;
using System.Collections;
using System.Collections.Generic;
using Octokit;
using Octokit.GraphQL.Core;
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
    }
}