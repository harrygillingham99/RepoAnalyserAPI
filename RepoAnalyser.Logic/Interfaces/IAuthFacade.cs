using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IAuthFacade
    {
        Task<TokenUserResponse> GetOAuthTokenWithUserInfo(string code, string state);
        Task<string> GetLoginRedirectUrl();

        Task<UserInfoResult> GetUserInformation(string token);
        Task<Dictionary<string, int>> GetComplexityForAssemblies(string pathToAssembly);
    }
}