using System;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects.Config;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.Services.OctoKit.Interfaces;
using OauthLoginRequest = Octokit.OauthLoginRequest;
using static RepoAnalyser.Objects.Helpers.OctoKitHelper;

namespace RepoAnalyser.Services.OctoKit
{
    public class OctoKitAuthServiceAgent : IOctoKitAuthServiceAgent
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _frontEndRedirectUrl;
        private readonly GitHubClient _client;
        private readonly IAppCache _cache;

        public OctoKitAuthServiceAgent(IOptions<GitHubSettings> options, IAppCache cache)
        {
            _cache = cache;
            _clientId = options.Value.ClientId;
            _clientSecret = options.Value.ClientSecret;
            _frontEndRedirectUrl = options.Value.FrontEndRedirectUrl;
            _client = BuildRestClient(options.Value.AppName);
        }

        public Uri GetLoginRedirectUrl()
        {
            var request = new OauthLoginRequest(_clientId)
            {
                RedirectUri = new Uri(_frontEndRedirectUrl),
                //why use a read only collection here? Means this prop can't be initialized from config
                Scopes = { "read:user", "repo", "security_events", "gist", "notifications" }
            };

            return _client.Oauth.GetGitHubLoginUrl(request);
        }

        public Task<OauthToken> GetOAuthToken(string code, string state)
        {
            if (string.IsNullOrEmpty(code)) throw new NullReferenceException("code parameter was null");

            var request = new OauthTokenRequest(_clientId, _clientSecret, code);

            return _client.Oauth.CreateAccessToken(request);
        }

        public Task<User> GetUserInformation(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new UnauthorizedRequestException("no token provided");

            _client.Connection.Credentials = GetCredentials(token);

            return _cache.GetOrAddAsync($"{token}-user", () => _client.User.Current(), CacheConstants.DefaultSlidingCacheExpiry);
        }
    }
}
