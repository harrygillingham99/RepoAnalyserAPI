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
        private const string ConnectionIdKey = "ConnectionId";

        public static ClientMetadata GetMetadataFromRequestHeaders(this HttpRequest request)
        {
            return request.GetHeaderValueOrDefault<ClientMetadata>(MetadataKey, true);
        }


        public static string GetAuthorizationToken(this HttpRequest request)
        {
            var header = request.GetHeaderValueOrDefault<string>(AuthorizationKey);
            return string.IsNullOrWhiteSpace(header)
                ? throw new UnauthorizedRequestException("No Authorization token provided.")
                : header;
        }

        public static string GetConnectionId(this HttpRequest request)
        {
            var header = request.GetHeaderValueOrDefault<string>(ConnectionIdKey);
            return string.IsNullOrWhiteSpace(header)
                ? throw new UnauthorizedRequestException("No Authorization token provided.")
                : header;
        }

        private static T GetHeaderValueOrDefault<T>(this HttpRequest request, string key, bool isJson = false)
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