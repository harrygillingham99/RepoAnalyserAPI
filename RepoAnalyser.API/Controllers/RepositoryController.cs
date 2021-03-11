using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.Analysis.Interfaces;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("repo")]
    public class RepositoryController : BaseController
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly IMsBuildRunner _msBuildRunner;
        public RepositoryController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IRepositoryFacade repositoryFacade, IMsBuildRunner msBuildRunner) : base(auditRepository, backgroundTaskQueue, options)
        {
            _repositoryFacade = repositoryFacade;
            _msBuildRunner = msBuildRunner;
        }

        [HttpGet("{filterOption}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<UserRepositoryResult>), Description = "Success getting repos")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting repos")]
        public Task<IActionResult> Repositories([FromRoute] RepoFilterOptions filterOption = RepoFilterOptions.All )
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetRepositories(token, filterOption);
            });
        }

        [HttpGet("detailed/{repoId}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DetailedRepository), Description = "Success getting detailed repo")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Repo not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting detailed repo information")]
        public Task<IActionResult> GetDetailedRepository([FromRoute] long repoId)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetDetailedRepository(repoId, token);
            });
        }

        [HttpGet("code-owners/{repoId}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Dictionary<string,string>), Description = "Success getting codeowners")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting code owners")]
        public Task<IActionResult> GetCodeOwnersForRepo([FromRoute] long repoId)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetRepositoryCodeOwners(repoId, token);
            });
        }
    }
}