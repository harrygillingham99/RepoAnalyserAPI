using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using Serilog;

namespace RepoAnalyser.API.Controllers
{
    [ProducesResponseType(typeof(NotFoundResponse), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(UnauthorizedResponse), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ValidationResponse), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    public class BaseController : ControllerBase
    {
        private readonly IRepoAnalyserAuditRepository _auditRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly bool _requestLogging;
        private readonly Stopwatch _stopwatch;

        public BaseController(IRepoAnalyserAuditRepository auditRepository, IBackgroundTaskQueue backgroundTaskQueue,
            IOptions<AppSettings> options)
        {
            _auditRepository = auditRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
            _stopwatch = new Stopwatch();
            _requestLogging = options.Value.RequestLogging;
        }

        protected async Task<IActionResult> ExecuteAndMapToActionResultAsync<T>(Func<Task<T>> request)
        {
            try
            {
                _stopwatch.Start();
                var response = await request.Invoke();
                return response switch
                {
                    null => throw new NullReferenceException($"null response from {request.Method.Name}"),

                    Exception errorResponse => throw errorResponse,

                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                return HandleErrorResponse(ex);
            }
            finally
            {
                _stopwatch.Stop();
                RequestAudit(new RequestAudit(HttpContext.Request.GetMetadataFromRequestHeaders(),
                    _stopwatch.ElapsedMilliseconds, HttpContext.Request.Path.Value ?? "unknown"));
            }
        }

        protected IActionResult ExecuteAndMapToActionResult<T>(Func<T> request)
        {
            try
            {
                _stopwatch.Start();
                var response = request.Invoke();
                return response switch
                {
                    null => throw new NullReferenceException($"null response from {request.Method.Name}"),

                    Exception errorResponse => throw errorResponse,

                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                return HandleErrorResponse(ex);
            }
            finally
            {
                _stopwatch.Stop();
                RequestAudit(new RequestAudit(HttpContext.Request.GetMetadataFromRequestHeaders(),
                    _stopwatch.ElapsedMilliseconds, HttpContext.Request.Path.Value ?? "unknown"));
            }
        }

        //Handle any more error responses with extra data/custom responses here
        private IActionResult HandleErrorResponse(Exception ex)
        {
            switch(ex)
            {
                 case NullReferenceException notFound:
                    Log.Error(notFound, notFound.Message);
                    return NotFound(new NotFoundResponse
                    {
                        Message = notFound.Message,
                        Title = notFound.GetType().Name,
                        BadProperties = new Dictionary<string, string>()
                    });
                case UnauthorizedRequestException unauthorizedRequest:
                    Log.Error(unauthorizedRequest, unauthorizedRequest.Message);
                    return Unauthorized(new UnauthorizedResponse
                    {
                        Message = unauthorizedRequest.Message,
                        Title = unauthorizedRequest.GetType().Name
                    });
                case BadRequestException badRequest:
                    Log.Error(badRequest, $"Bad Request: {badRequest.Message}");
                    return BadRequest(new ValidationResponse
                    {
                        Message = badRequest.Message,
                        Title = badRequest.GetType().Name,
                        ValidationErrors = new Dictionary<string, string>()
                    });
                default:
                    Log.Error(ex, ex.Message);
                    return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError,
                        title: ex.GetType().Name, type: ex.GetType().FullName);
            }
        }

        //Doing some performance/debug request logging when deployed on home server, handy to know when react is spamming the backend
        private void RequestAudit(RequestAudit audit)
        {
            var eventText = $"****Requested {audit.RequestedEndpoint}, It took {audit.ExecutionTime}ms to respond.****";
            Log.Information(eventText);
            if (audit.Metadata != null && _requestLogging)
            {
                _backgroundTaskQueue.QueueBackgroundWorkItem(token =>
                    _auditRepository.InsertRequestAudit(audit));
            }
            else
                Debug.WriteLine(eventText);
        }
    }
}