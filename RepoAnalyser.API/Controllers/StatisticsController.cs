using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic;
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
        [SwaggerResponse(HttpStatusCode.OK, typeof(UserActivity), Description = "Success getting user stats")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationResponse), Description = "Bad request getting user stats")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided when getting user stats")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Error getting user stats, user not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting user stats")]
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