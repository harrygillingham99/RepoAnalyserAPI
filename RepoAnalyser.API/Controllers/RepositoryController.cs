using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.Analysis.Interfaces;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("repositories")]
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

        [HttpGet("code-owners/{repoId}/{connectionId}")]
        [ProducesResponseType(typeof(IDictionary<string,string>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetCodeOwnersForRepo([FromRoute] long repoId, [FromRoute] string connectionId)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _repositoryFacade.GetRepositoryCodeOwners(repoId, connectionId, token);
            });
        }
    }
}