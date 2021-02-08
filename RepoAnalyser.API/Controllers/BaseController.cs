﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RepoAnalyser.API.BackgroundTaskQueue;
using RepoAnalyser.API.Helpers;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Exceptions;
using RepoAnalyser.SqlServer.DAL;
using Serilog;

namespace RepoAnalyser.API.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IRepoAnalyserRepository _repoAnalyserRepository;
        private readonly Stopwatch _stopwatch;

        public BaseController(IRepoAnalyserRepository repoAnalyserRepository, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _repoAnalyserRepository = repoAnalyserRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
            _stopwatch = new Stopwatch();
        }

        protected async Task<IActionResult> ExecuteAndMapToActionResultAsync<T>(Func<Task<T>> request)
        {
            try
            {
                _stopwatch.Start();
                var response = await request.Invoke();
                return response switch
                {
                    null => throw new NullReferenceException("Thing not found"),

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
                QueueInsertingRequestAudit(_stopwatch.ElapsedMilliseconds);
               
            }
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> request)
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
                QueueInsertingRequestAudit(_stopwatch.ElapsedMilliseconds);
            }
        }

        protected T Execute<T>(Func<T> request)
        {
            try
            {
                _stopwatch.Start();
                return request.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                QueueInsertingRequestAudit(_stopwatch.ElapsedMilliseconds);
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
                    null => throw new NullReferenceException(),

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
            catch (ValidationException ex)
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
                QueueInsertingRequestAudit(_stopwatch.ElapsedMilliseconds);
            }
        }

        private void QueueInsertingRequestAudit(long elapsedMilliseconds) => 
            _backgroundTaskQueue.QueueBackgroundWorkItem(token =>
            _repoAnalyserRepository.InsertRequestAudit(HttpContext.Request.GetMetadataFromRequestHeaders(),
                elapsedMilliseconds, HttpContext.Request.Path.Value));
    }
}