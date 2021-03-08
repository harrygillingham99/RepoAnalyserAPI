using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
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
        private readonly IAuthenticationFacade _authFacade;

        public AuthenticationController(IAuthenticationFacade authFacade, IRepoAnalyserAuditRepository repository,
            IBackgroundTaskQueue worker, IOptions<AppSettings> options) : base(repository, worker, options)
        {
            _authFacade = authFacade;
        }

        [HttpGet("token/{code}/{state}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(TokenUserResponse), Description = "Success getting auth token")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationResponse), Description = "Bad request getting auth token")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided when getting user info")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Error getting auth token, code provided not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting auth token")]
        public Task<IActionResult> GetOAuthTokenWithUserInfo([FromRoute]string code, [FromRoute]string state)
        {
            return ExecuteAndMapToActionResultAsync(() =>
                _authFacade.GetOAuthTokenWithUserInfo(code, state)
            );
        }

        [HttpGet("login-redirect")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string), Description = "Success getting github redirect url")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationResponse), Description = "Bad request getting redirect url")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Error getting redirect url")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting redirect url")]
        public IActionResult GetLoginRedirectUrl()
        {
            return ExecuteAndMapToActionResult(() =>
                _authFacade.GetLoginRedirectUrl());
        }

        [HttpGet("user-info")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(UserInfoResult), Description = "Success getting user info")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(UnauthorizedResponse), Description = "No token provided")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "User not found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error getting user")]
        public Task<IActionResult> GetUserInformationForToken()
        {
            return ExecuteAndMapToActionResultAsync(() =>
            {
                var token = HttpContext.Request.GetAuthorizationToken();
                return _authFacade.GetUserInformation(token);
            });
        }
    }
}