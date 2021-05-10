using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Config;
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

        [HttpGet("user/{page}/{pageSize}")]
        [ProducesResponseType(typeof(UserActivity), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetUserStatistics([FromRoute] int page, [FromRoute] int pageSize)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _userFacade.GetUserStatistics(token, new PaginationOptions
                {
                    PageSize = pageSize,
                    Page = page
                });
            });
        }

        [HttpGet("landing")]
        [ProducesResponseType(typeof(UserLandingPageStatistics), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetLandingPageStatistics()
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _userFacade.GetLandingPageStatistics(token);
            });
        }
    }
}