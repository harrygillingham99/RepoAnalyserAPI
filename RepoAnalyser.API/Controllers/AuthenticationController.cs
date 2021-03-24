using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.BackgroundTaskQueue;
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
        [ProducesResponseType(typeof(TokenUserResponse), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetOAuthTokenWithUserInfo([FromRoute]string code, [FromRoute]string state)
        {
            return ExecuteAndMapToActionResultAsync(() =>
                _authFacade.GetOAuthTokenWithUserInfo(code, state)
            );
        }

        [HttpGet("login-redirect")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult GetLoginRedirectUrl()
        {
            return ExecuteAndMapToActionResult(() =>
                _authFacade.GetLoginRedirectUrl());
        }

        [HttpGet("user-info")]
        [ProducesResponseType(typeof(UserInfoResult), (int)HttpStatusCode.OK)]
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