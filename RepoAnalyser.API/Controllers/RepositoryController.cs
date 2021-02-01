using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Responses;

namespace RepoAnalyser.API.Controllers
{
    [ApiController]
    [Route("Repo")]
    public class RepositoryController : BaseController
    {
        private readonly string _responseStringFromConfig;

        public RepositoryController(IOptions<AppSettings> options)
        {
            _responseStringFromConfig = options.Value.ResponseString;
        }

        [HttpGet("{statusToReturn}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(TestResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ProblemDetails), Description = "Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ValidationResponse), Description = "Bad Request")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(NotFoundResponse), Description = "Not Found")]
        public async Task<IActionResult> Test([FromRoute]int statusToReturn = 200)
        {
            return await ExecuteAndMapToActionResultAsync(() =>
            {
                return statusToReturn switch
                {
                    200 => Task.FromResult(new TestResponse {TestProp = _responseStringFromConfig}),
                    500 => throw new InvalidOperationException("Exception happened"),
                    400 => throw new ValidationException("Wrong"),
                    404 => null,
                    _ => null
                };
            });
        }

    }
}
