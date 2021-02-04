using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.API.Helpers
{
    public static class HeaderMetadataHelper
    {
        public static ClientMetadata GetMetadataFromRequestHeaders(this HttpRequest request) =>
            JsonConvert.DeserializeObject<ClientMetadata>(request.Headers["Metadata"]);

        public static string GetAuthorizationToken(this HttpRequest request) =>
            request.Headers.ContainsKey("Authorization") ? 
                request.Headers["Authorization"].First(): 
                throw new UnauthorizedAccessException("No auth token provided");
    }
}
