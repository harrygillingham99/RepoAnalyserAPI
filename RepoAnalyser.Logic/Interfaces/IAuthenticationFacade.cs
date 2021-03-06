using System.Threading.Tasks;
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