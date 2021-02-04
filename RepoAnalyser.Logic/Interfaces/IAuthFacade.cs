using System.Threading.Tasks;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IAuthFacade
    {
        Task<string> GetOAuthToken(string code, string state);
        Task<string> GetLoginRedirectUrl();

    }
}