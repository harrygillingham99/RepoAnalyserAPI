using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.Exceptions;

namespace RepoAnalyser.API.Helpers
{
    public static class HttpRequestHelper
    {
        private const string MetadataKey = "metadata";
        private const string AuthorizationKey = "Authorization";

        public static ClientMetadata GetMetadataFromRequestHeaders(this HttpRequest request)
        {
            var header = TryGetHeaderValue<ClientMetadata>(MetadataKey, true, request);
            return header;
        }

        public static string GetAuthorizationToken(this HttpRequest request)
        {
            var header = TryGetHeaderValue<string>(AuthorizationKey, false, request);
            return header ?? throw new UnauthorizedRequestException("No Authorization token provided.");
        }

        private static T TryGetHeaderValue<T>(string key, bool isJson, HttpRequest request)
        {
            var hasHeader = request.Headers.TryGetValue(key, out var headerValues);

            if (hasHeader && headerValues.Any())
                return isJson
                    ? JsonConvert.DeserializeObject<T>(headerValues.First())
                    : (T) Convert.ChangeType(headerValues.First(), typeof(T));

            return default;
        }
    }
}