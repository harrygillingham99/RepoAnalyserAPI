﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.Objects;
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
        public Task<string> GetLoginRedirectUrl()
        {
            var request = new OauthLoginRequest(_clientId)
            {
                RedirectUri = new Uri(_frontEndRedirectUrl),
            };
            return Task.FromResult(_client.Oauth.GetGitHubLoginUrl(request).AbsoluteUri);
        }

        public async Task<string> GetOAuthToken(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var request = new OauthTokenRequest(_clientId, _clientSecret, code);

            return (await _client.Oauth.CreateAccessToken(request)).AccessToken;

        }

    }
}
