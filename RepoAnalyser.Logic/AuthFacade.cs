using RepoAnalyser.Services;
using System.Threading.Tasks;

namespace RepoAnalyser.Logic
{
    //TODO: hook up to DAL and store these tokens
    public class AuthFacade : IAuthFacade
    {
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;

        public AuthFacade(IOctoKitAuthServiceAgent octoKitAuthServiceAgent)
        {
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
        }


        public Task<string> GetOAuthToken(string code, string state) =>
            _octoKitAuthServiceAgent.GetOAuthToken(code, state);

        public Task<string> GetLoginRedirectUrl() => _octoKitAuthServiceAgent.GetLoginRedirectUrl();
    }
}
