using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit;
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

        public Task<UserActivity> GetUserStatistics(string token)
        {
            return _octoKitServiceAgent.GetDetailedUserActivity(token);
        }
    }

    public interface IUserFacade
    {
        Task<UserActivity> GetUserStatistics(string token);
    }
}
