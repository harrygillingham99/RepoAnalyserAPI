using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Attributes;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("repositories")]
    public class RepositoryController : BaseController
    {
        private readonly IRepositoryFacade _repositoryFacade;
        public RepositoryController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IRepositoryFacade repositoryFacade) : base(auditRepository, backgroundTaskQueue, options)
        {
            _repositoryFacade = repositoryFacade;
        }

        [HttpGet("{filterOption}")]
        [ProducesResponseType(typeof(IEnumerable<UserRepositoryResult>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> Repositories([FromRoute] RepoFilterOptions filterOption = RepoFilterOptions.All)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetRepositories(token, filterOption);
            });
        }

        [HttpGet("detailed/{repoId}")]
        [ProducesResponseType(typeof(DetailedRepository), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetDetailedRepository([FromRoute] long repoId)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetDetailedRepository(repoId, token);
            });
        }

        [RequireConnectionId]
        [HttpGet("code-owners/{repoId}")]
        [ProducesResponseType(typeof(IDictionary<string,string>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetCodeOwnersForRepo([FromRoute] long repoId)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var (connectionId, token) = (HttpContext.Request.GetConnectionId(), HttpContext.Request.GetAuthorizationToken());
                return _repositoryFacade.GetRepositoryCodeOwners(repoId, connectionId, token);
            });
        }

        [HttpGet("file-info/{repoId}/{fileName}/{extension}")]
        [ProducesResponseType(typeof(IEnumerable<GitHubCommit>), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetFileInformation([FromRoute] long repoId, [FromRoute] string fileName, [FromRoute] string extension)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetFileInformation(repoId, token, $"{fileName}.{extension}");
            });
        }
    }
}