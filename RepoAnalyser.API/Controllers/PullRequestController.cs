﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("pull-requests")]
    public class PullRequestController : BaseController
    {
        private readonly IPullRequestFacade _pullRequestFacade;
        public PullRequestController(IRepoAnalyserAuditRepository auditRepository, IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options, IPullRequestFacade pullRequestFacade) : base(auditRepository, backgroundTaskQueue, options)
        {
            _pullRequestFacade = pullRequestFacade;
        }

        [HttpGet("{pullFilterOption}")]
        [ProducesResponseType(typeof(IEnumerable<UserPullRequestResult>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetPullRequests(
            [FromRoute] PullRequestFilterOption pullFilterOption =  PullRequestFilterOption.All)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetPullRequests(token, pullFilterOption);
            });
        }

        [HttpGet("detailed/{repoId}/{pullNumber}")]
        [ProducesResponseType(typeof(DetailedPullRequest), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetDetailedPullRequest([FromRoute] long repoId, [FromRoute] int pullNumber)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetDetailedPullRequest(token, repoId, pullNumber);
            });
        }

        [HttpGet("file-info/{repoId}/{pullNumber}/{fileName}/{extension}")]
        [ProducesResponseType(typeof(PullFileInfo), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetPullFileInformation([FromRoute] long repoId, [FromRoute] int pullNumber,
            [FromRoute] string fileName, [FromRoute] string extension)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetPullFileInformation(repoId, pullNumber, $"{fileName}.{extension}", token);
            });
        }

        [HttpGet("cyclomatic-complexity/{repoId}/{pullNumber}")]
        [ProducesResponseType(typeof(IDictionary<string, int>), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetCyclomaticComplexitiesForPull([FromRoute] long repoId, [FromRoute] int pullNumber)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var (token, connectionId) = (HttpContext.Request.GetAuthorizationToken(),
                    HttpContext.Request.GetConnectionId());
                return _pullRequestFacade.GetPullCyclomaticComplexity(repoId, pullNumber, token, connectionId);
            });
        }
    }
}
