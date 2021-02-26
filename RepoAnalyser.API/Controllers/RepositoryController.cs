using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("repo")]
    public class RepositoryController : BaseController
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitServiceAgent;

        public RepositoryController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IOctoKitGraphQlServiceAgent octoKitServiceAgent) : base(auditRepository, backgroundTaskQueue, options)
        {
            _octoKitServiceAgent = octoKitServiceAgent;
        }

        [HttpGet("")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Repo>), Description = "Success getting repos")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting repos")]
        public Task<IActionResult> Repositories()
        {
            return ExecuteAndMapToActionResult(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _octoKitServiceAgent.GetRepositories(token);
            });
        }
    }
}