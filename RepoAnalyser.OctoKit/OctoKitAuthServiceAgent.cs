using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.Services.Interfaces;
using OauthLoginRequest = Octokit.OauthLoginRequest;

namespace RepoAnalyser.Services
{
    public class OctoKitAuthServiceAgent : IOctoKitAuthServiceAgent
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _frontEndRedirectUrl;
        private readonly GitHubClient _client;

        public OctoKitAuthServiceAgent(IOptions<GitHubSettings> options)
        {
            _clientId = options.Value.ClientId;
            _clientSecret = options.Value.ClientSecret;
            _frontEndRedirectUrl = options.Value.FrontEndRedirectUrl;
            _client = new GitHubClient(new ProductHeaderValue(options.Value.AppName));
        }
        //Everything else is actually async, just want to follow the task pattern
        public Task<Uri> GetLoginRedirectUrl()
        {
            var request = new OauthLoginRequest(_clientId)
            {

                RedirectUri = new Uri(_frontEndRedirectUrl)
            };
            return Task.FromResult(_client.Oauth.GetGitHubLoginUrl(request));
        }

        public async Task<OauthToken> GetOAuthToken(string code, string state)
        {
            if (string.IsNullOrEmpty(code)) throw new NullReferenceException("code parameter was null");

            var request = new OauthTokenRequest(_clientId, _clientSecret, code);

            return (await _client.Oauth.CreateAccessToken(request));
        }

        public async Task<User> GetUserInformation(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new UnauthorizedRequestException("no token provided");

            _client.Connection.Credentials = new Credentials(token);

            return await _client.User.Current();
        }
    }
}
