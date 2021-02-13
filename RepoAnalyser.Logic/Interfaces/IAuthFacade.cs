﻿using System.Threading.Tasks;
using Octokit;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IAuthFacade
    {
        Task<TokenUserResponse> GetOAuthTokenWithUserInfo(string code, string state);
        Task<string> GetLoginRedirectUrl();

        Task<User> GetUserInformation(string token);
    }
}