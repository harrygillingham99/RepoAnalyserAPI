using System;
using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Services.OctoKit.Interfaces
{
    public interface IOctoKitAuthServiceAgent
    {
        Uri GetLoginRedirectUrl();
        Task<OauthToken> GetOAuthToken(string code, string state);

        Task<User> GetUserInformation(string token);
    }
}