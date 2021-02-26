using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using Serilog;

namespace RepoAnalyser.API.Controllers
{
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

        protected async Task<IActionResult> ExecuteAndMapToActionResult<T>(Func<Task<T>> request)
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
            catch (NullReferenceException ex)
            {
                Log.Error(ex, ex.Message);
                return NotFound(new NotFoundResponse
                {
                    Message = ex.Message,
                    Title = ex.GetType().Name,
                    BadProperties = new Dictionary<string, string>()
                });
            }
            catch (UnauthorizedRequestException ex)
            {
                Log.Error(ex, ex.Message);
                return Unauthorized(new UnauthorizedResponse
                {
                    Message = ex.Message,
                    Title = ex.GetType().Name
                });
            }
            catch (BadRequestException ex)
            {
                Log.Error(ex, $"Bad Request: {ex.Message}");
                return BadRequest(new ValidationResponse
                {
                    Message = ex.Message,
                    Title = ex.GetType().Name,
                    ValidationErrors = new Dictionary<string, string>()
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Problem(ex.Message, statusCode: 500, title: ex.GetType().Name);
            }
            finally
            {
                _stopwatch.Stop();
                RequestAudit(new RequestAudit(HttpContext.Request.GetMetadataFromRequestHeaders(),
                    _stopwatch.ElapsedMilliseconds, HttpContext.Request.Path.Value ?? "unknown"));
            }
        }

        protected async Task<T> ExecuteAndReturn<T>(Func<Task<T>> request)
        {
            try
            {
                _stopwatch.Start();
                return await request.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                RequestAudit(new RequestAudit(HttpContext.Request.GetMetadataFromRequestHeaders(),
                    _stopwatch.ElapsedMilliseconds, HttpContext.Request.Path.Value ?? "unknown"));
            }
        }

        //Doing some performance/debug request logging when deployed on home server, handy to know when react is spamming the backend
        private void RequestAudit(RequestAudit audit)
        {
            if (audit.Metadata != null && _requestLogging)
                _backgroundTaskQueue.QueueBackgroundWorkItem(token =>
                    _auditRepository.InsertRequestAudit(audit));
            else
                Debug.WriteLine(
                    $"****Requested {audit.RequestedEndpoint}, It took {audit.ExecutionTime}ms to respond.****");
        }
    }
}