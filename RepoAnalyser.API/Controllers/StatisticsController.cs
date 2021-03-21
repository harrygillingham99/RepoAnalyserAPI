using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("statistics")]
    public class StatisticsController : BaseController
    {
        private readonly IUserFacade _userFacade;

        public StatisticsController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options, IUserFacade userFacade) : base(
            auditRepository, backgroundTaskQueue, options)
        {
            _userFacade = userFacade;
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(UserActivity), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetUserStatistics()
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _userFacade.GetUserStatistics(token);
            });
        }
    }
}