using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.API.Helpers
{
    public static class HttpRequestHelper
    {
        public static ClientMetadata GetMetadataFromRequestHeaders(this HttpRequest request)
        {
            var hasHeader = request.Headers.TryGetValue("metadata", out StringValues headerVal);

            if(hasHeader && headerVal.Any()) return JsonConvert.DeserializeObject<ClientMetadata>(headerVal.First());

            return null;
        }
           

        public static string GetAuthorizationToken(this HttpRequest request) =>
            request.Headers.ContainsKey("Authorization") ? 
                request.Headers["Authorization"].First(): 
                throw new UnauthorizedAccessException("No auth token provided");
    }
}
