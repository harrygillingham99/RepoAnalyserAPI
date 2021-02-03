using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.API.Helpers
{
    public static class HeaderMetadataHelper
    {
        public static ClientMetadata GetMetadataFromRequestHeaders(this HttpRequest request) =>
            JsonConvert.DeserializeObject<ClientMetadata>(request.Headers["Metadata"]);
    }
}
