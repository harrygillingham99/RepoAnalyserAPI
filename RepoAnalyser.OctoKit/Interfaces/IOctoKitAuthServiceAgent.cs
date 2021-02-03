using System.Threading.Tasks;

namespace RepoAnalyser.Services
{
    public interface IOctoKitAuthServiceAgent
    {
        Task<string> GetLoginRedirectUrl();
        Task<string> GetOAuthToken(string code, string state);

    }
}