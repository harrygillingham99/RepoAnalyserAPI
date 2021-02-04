using System;
using System.Threading.Tasks;
using Octokit;

namespace RepoAnalyser.Services.Interfaces
{
    public interface IOctoKitAuthServiceAgent
    {
        Task<Uri> GetLoginRedirectUrl();
        Task<OauthToken> GetOAuthToken(string code, string state);

    }
}