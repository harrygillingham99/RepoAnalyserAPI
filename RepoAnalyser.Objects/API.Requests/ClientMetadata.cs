using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.Objects.API.Requests
{
    [NSwagInclude]
    public class ClientMetadata
    {
        public string Page { get; set; }
        public string Referrer { get; set; }
        public string BrowserName { get; set; }
        public string BrowserEngine { get; set; }
        public string BrowserLanguage { get; set; }
        public bool CookiesEnabled { get; set; }

    }
}
