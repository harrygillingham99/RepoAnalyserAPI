using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Objects;
using RepoAnalyser.SqlServer.DAL.Interfaces;

namespace RepoAnalyser.API.Controllers
{ 
    [ApiController]
    [Route("utilities")]
    public class UtilitiesController : BaseController
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IUtilitiesRepository _utilitiesRepository;

        public UtilitiesController(IRepoAnalyserAuditRepository auditRepository,
            IBackgroundTaskQueue backgroundTaskQueue, IOptions<AppSettings> options,
            IUtilitiesRepository utilitiesRepository) : base(auditRepository, backgroundTaskQueue, options)
        {
            _taskQueue = backgroundTaskQueue;
            _utilitiesRepository = utilitiesRepository;
        }

        [HttpDelete("truncate-request-audits")]
        [ProducesResponseType(typeof(bool), (int) HttpStatusCode.OK)]
        public IActionResult GetDetailedRepository()
        {
            return ExecuteAndMapToActionResult(() =>
            {
                _taskQueue.QueueBackgroundWorkItem(token => _utilitiesRepository.TruncateRequestAuditReseedId());
                return true;
            });
        }
    }
}