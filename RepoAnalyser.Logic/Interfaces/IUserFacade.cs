using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IUserFacade
    {
        Task<UserActivity> GetUserStatistics(string token, PaginationOptions pageOptions);
    }
}