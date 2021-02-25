using Octokit;
using Connection = Octokit.GraphQL.Connection;

namespace RepoAnalyser.Objects.Helpers
{
    public static class OctoKitHelper
    {
        public static Credentials GetCredentials(string token) => new Credentials(token);

        public static GitHubClient BuildRestClient(string appName) => new GitHubClient(new ProductHeaderValue(appName));

        public static Connection BuildConnection(Octokit.GraphQL.ProductHeaderValue product, string token) => new Connection(product, token);
    }
}
