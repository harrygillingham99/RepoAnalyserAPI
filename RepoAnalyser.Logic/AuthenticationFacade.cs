using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using RepoAnalyser.Logic.AnalysisHelpers;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.Services.libgit2csharp.Adapter;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class AuthenticationFacade : IAuthenticationFacade
    {
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        public AuthenticationFacade(IOctoKitAuthServiceAgent octoKitAuthServiceAgent, IGitAdapter gitAdapter)
        {
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
        }

        public async Task<TokenUserResponse> GetOAuthTokenWithUserInfo(string code, string state)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new BadRequestException("No code provided");

            var token = await _octoKitAuthServiceAgent.GetOAuthToken(code, state);

            if(token is null || string.IsNullOrWhiteSpace(token.AccessToken)) throw new NullReferenceException("Token generated was null");

            return new TokenUserResponse
            {
                AccessToken = token.AccessToken,
                User = await _octoKitAuthServiceAgent.GetUserInformation(token.AccessToken)
            };
        }

        public async Task<string> GetLoginRedirectUrl()
        {
            var urlResult = await _octoKitAuthServiceAgent.GetLoginRedirectUrl();

            if (urlResult is null) throw new NullReferenceException("Returned URL is null");

            return urlResult.AbsoluteUri;
        }

        public async Task<UserInfoResult> GetUserInformation(string token)
        {
            var user = _octoKitAuthServiceAgent.GetUserInformation(token);

            var urlResult =  _octoKitAuthServiceAgent.GetLoginRedirectUrl();

            return new UserInfoResult
            {
                User = await user ?? throw new NullReferenceException("User is null"),
                LoginRedirectUrl = (await urlResult)?.AbsoluteUri ?? throw new NullReferenceException("Url was null")
            };
        }
    }
}
