using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using RepoAnalyser.Logic.AnalysisHelpers;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
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
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);

            var urlResult = await _octoKitAuthServiceAgent.GetLoginRedirectUrl();

            if(user is null) throw new NullReferenceException("User was null");

            if(urlResult is null) throw new NullReferenceException("Url was null");

            return new UserInfoResult
            {
                User = user,
                LoginRedirectUrl = urlResult.AbsoluteUri
            };
        }

        public Task<Dictionary<string, int>> GetComplexityForAssemblies(string pathToAssembly)
        {
            var x = CecilHelper.ReadAssemblies(new List<string> {pathToAssembly});
            var y = x.Select(definition => new List<AssemblyDefinition> { definition.Assembly }.ScanForMethods(new List<string> { "Get" })).ToList().FirstOrDefault();
            var z = y.ToDictionary(key => key.Name, val => val.GetCyclomaticComplexity());
            return Task.FromResult(z);
        }
    }
}
