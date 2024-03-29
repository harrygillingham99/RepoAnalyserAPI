﻿using System.Collections.Generic;
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

        [HttpGet("discussion-issues/{repoId}/{pullNumber}")]
        [ProducesResponseType(typeof(PullDiscussionResult), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetPullIssuesAndDiscussion([FromRoute] long repoId, [FromRoute] int pullNumber)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetPullIssuesAndDiscussion(repoId, pullNumber, token);
            });
        }

        [HttpGet("summary/{repoId}/{pullNumber}")]
        [ProducesResponseType(typeof(PullSummaryResponse), (int) HttpStatusCode.OK)]
        public Task<IActionResult> GetPullRequestSummary([FromRoute] long repoId, [FromRoute] int pullNumber)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetPullRequestSummary(repoId, pullNumber, token);
            });
        }
    }
}
