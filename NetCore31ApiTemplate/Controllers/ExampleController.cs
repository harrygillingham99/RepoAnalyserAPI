using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSwag.Annotations;

namespace NetCore31ApiTemplate.Controllers
{
    [ApiController]
    [Route("Example")]
    public class ExampleController : BaseController
    {
        private readonly string _responseStringFromConfig;

        public ExampleController(IOptions<AppSettings> options)
        {
            _responseStringFromConfig = options.Value.ResponseString;
        }

        [HttpGet("{statusToReturn}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(TestResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(ActionResult), Description = "Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ActionResult), Description = "Bad Request")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ActionResult), Description = "Not Found")]
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



        public class TestResponse
        {
            public string TestProp { get; set; }
        }

    }
}
