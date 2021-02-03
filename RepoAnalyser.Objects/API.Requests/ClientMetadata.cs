using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.Objects.API.Requests
{
    [NSwagInclude]
    public class ClientMetadata
    {
        public string UserAgent { get; set; }
    }
}
