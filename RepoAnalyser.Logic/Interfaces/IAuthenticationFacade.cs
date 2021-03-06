using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IAuthenticationFacade
    {
        Task<TokenUserResponse> GetOAuthTokenWithUserInfo(string code, string state);
        Task<string> GetLoginRedirectUrl();

        Task<UserInfoResult> GetUserInformation(string token);
    }
}