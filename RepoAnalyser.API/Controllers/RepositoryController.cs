using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("repo")]
    public class RepositoryController : BaseController
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitServiceAgent;
        private readonly IAuthFacade _authFacade;
        public RepositoryController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IOctoKitGraphQlServiceAgent octoKitServiceAgent, IAuthFacade authFacade) : base(auditRepository, backgroundTaskQueue, options)
        {
            _octoKitServiceAgent = octoKitServiceAgent;
            _authFacade = authFacade;
        }

        [HttpGet("{filterOption}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Repo>), Description = "Success getting repos")]
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

        [HttpPost("TestCyclomaticComplexity")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Dictionary<string, int>), Description = "Success getting repos")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting repos")]
        public Task<IActionResult> GetComplexityForMethodsInAssembly([FromBody] string pathToAssembly)
        {
            return ExecuteAndMapToActionResult(() =>
                _authFacade.GetComplexityForAssemblies(pathToAssembly)
            );
        }

    }
}