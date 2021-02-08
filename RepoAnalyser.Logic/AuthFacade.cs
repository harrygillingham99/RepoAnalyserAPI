﻿using System;
using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.Services.Interfaces;

namespace RepoAnalyser.Logic
{
    //TODO: hook up to DAL and store these tokens
    public class AuthFacade : IAuthFacade
    {
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;

        public AuthFacade(IOctoKitAuthServiceAgent octoKitAuthServiceAgent)
        {
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
        }


        public async Task<TokenUserResponse> GetOAuthTokenWithUserInfo(string code, string state)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new BadRequestException("No code provided");

            var token = await _octoKitAuthServiceAgent.GetOAuthToken(code, state);

            if(token == null) throw new NullReferenceException("Token generated was null");

            return new TokenUserResponse
            {
                AccessToken = token.AccessToken,
                User = await _octoKitAuthServiceAgent.GetUserInformation(token.AccessToken)
            };
        }

        public async Task<string> GetLoginRedirectUrl()
        {
            var urlResult = await _octoKitAuthServiceAgent.GetLoginRedirectUrl();

            if (urlResult == null) throw new NullReferenceException("Returned URL is null");

            return urlResult.AbsoluteUri;
        }
    }
}
