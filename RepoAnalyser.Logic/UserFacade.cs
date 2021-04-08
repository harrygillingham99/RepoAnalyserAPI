using System.Threading.Tasks;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.Interfaces;

namespace RepoAnalyser.Logic
{
    public class UserFacade : IUserFacade
    {
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;

        public UserFacade(IOctoKitServiceAgent octoKitServiceAgent)
        {
            _octoKitServiceAgent = octoKitServiceAgent;
        }

        public Task<UserActivity> GetUserStatistics(string token, PaginationOptions pageOptions)
        {
            return _octoKitServiceAgent.GetDetailedUserActivity(token, pageOptions);
        }
    }
}
