﻿using System.Collections.Generic;
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
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<UserPullRequestResult>), Description = "Success getting pull requests")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting pull requests")]
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
        [SwaggerResponse(HttpStatusCode.OK, typeof(DetailedPullRequest), Description = "Success getting pull requests")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting pull requests")]
        public Task<IActionResult> GetDetailedPullRequest([FromRoute] long repoId, [FromRoute] int pullNumber)
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _pullRequestFacade.GetDetailedPullRequest(token, repoId, pullNumber);
            });
        }
    }
}
