namespace RepoAnalyser.CSharp.Client
{
    public class BaseClient
    {
        protected readonly string BaseUrl;

        protected BaseClient(ApiClientConfig configuration)
        {
            BaseUrl = configuration.BaseUrl;
        }
    }
}
