using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IPullRequestFacade
    {
        Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption filterOption);
    }
}