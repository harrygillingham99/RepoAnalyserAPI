using System;
using System.Threading.Tasks;
using Octokit;

namespace RepoAnalyser.Services.OctoKit.Interfaces
{
    public interface IOctoKitAuthServiceAgent
    {
        Uri GetLoginRedirectUrl();
        Task<OauthToken> GetOAuthToken(string code, string state);

        Task<User> GetUserInformation(string token);

    }
}