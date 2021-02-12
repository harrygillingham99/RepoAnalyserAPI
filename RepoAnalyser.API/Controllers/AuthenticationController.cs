using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthFacade _authFacade;

        public AuthenticationController(IAuthFacade authFacade, IRepoAnalyserAuditRepository repository,
            IBackgroundTaskQueue worker, IOptions<AppSettings> options) : base(repository, worker, options)
        {
            _authFacade = authFacade;
        }

        [HttpGet("token")]
        [SwaggerResponse(200, typeof(TokenUserResponse), Description = "Success getting auth token")]
        [SwaggerResponse(400, typeof(ValidationResponse), Description = "Bad request getting auth token")]
        [SwaggerResponse(401, typeof(UnauthorizedResponse), Description = "No token provided when getting user info")]
        [SwaggerResponse(404, typeof(NotFoundResponse), Description = "Error getting auth token, code provided not found")]
        [SwaggerResponse(500, typeof(ProblemDetails), Description = "Error getting auth token")]
        public Task<IActionResult> GetOAuthTokenWithUserInfo(string code, string state)
        {
            return ExecuteAndMapToActionResult(() =>
                _authFacade.GetOAuthTokenWithUserInfo(code, state)
            );
        }

        [HttpGet("login-redirect")]
        [SwaggerResponse(200, typeof(string), Description = "Success getting github redirect url")]
        [SwaggerResponse(400, typeof(ValidationResponse), Description = "Bad request getting redirect url")]
        [SwaggerResponse(404, typeof(NotFoundResponse), Description = "Error getting redirect url")]
        [SwaggerResponse(500, typeof(ProblemDetails), Description = "Error getting redirect url")]
        public Task<IActionResult> GetLoginRedirectUrl()
        {
            return ExecuteAndMapToActionResult(() =>
                _authFacade.GetLoginRedirectUrl());
        }
    }
}