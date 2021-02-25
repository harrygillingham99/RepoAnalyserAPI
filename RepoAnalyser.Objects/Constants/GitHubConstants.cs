using System.Collections.Generic;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;

namespace RepoAnalyser.Objects.Constants
{
    public static class GitHubConstants
    {
        public static Arg<IEnumerable<RepositoryAffiliation?>> RepositoryScopes = new Arg<IEnumerable<RepositoryAffiliation?>>(new List<RepositoryAffiliation?> { RepositoryAffiliation.Owner, RepositoryAffiliation.Collaborator });
    }
}
