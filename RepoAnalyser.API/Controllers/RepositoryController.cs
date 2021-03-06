using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
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
        private readonly IRepositoryFacade _repositoryFacade;
        public RepositoryController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IOctoKitGraphQlServiceAgent octoKitServiceAgent, IRepositoryFacade repositoryFacade) : base(auditRepository, backgroundTaskQueue, options)
        {
            _octoKitServiceAgent = octoKitServiceAgent;
            _repositoryFacade = repositoryFacade;
        }

        [HttpGet("{filterOption}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<UserRepositoryResult>), Description = "Success getting repos")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting repos")]
        public Task<IActionResult> Repositories([FromRoute] RepoFilterOptions filterOption = RepoFilterOptions.All )
        {
            return ExecuteAndMapToActionResult(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _octoKitServiceAgent.GetRepositories(token, filterOption);
            });
        }

        [HttpPost("commits")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Commit>), Description = "Success getting commits for repo")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Repo not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting commits for repo")]
        public Task<IActionResult> GetCommitsForRepository([FromBody] RepositoryCommitsRequest request)
        {
            return ExecuteAndMapToActionResult(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetCommitsForRepo(request, token);
            });
        }

    }
}