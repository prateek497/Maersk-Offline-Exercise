using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Hangfire;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Maersk.Sorting.Api.Controllers
{
    [ApiController]
    [Route("sort")]
    public class SortController : ControllerBase
    {
        private readonly ISortJobProcessor _sortJobProcessor;
        private readonly IBackgroundJobClient _backgroungJobClient;

        public SortController(ISortJobProcessor sortJobProcessor, IBackgroundJobClient backgroungJobClient)
        {
            _sortJobProcessor = sortJobProcessor;
            _backgroungJobClient = backgroungJobClient;
        }

        [HttpPost("run")]
        [Obsolete("This executes the sort job asynchronously. Use the asynchronous 'EnqueueJob' instead.")]
        public async Task<ActionResult<SortJob>> EnqueueAndRunJob(int[] values)
        {
            var pendingJob = new SortJob(
                id: Guid.NewGuid(),
                status: SortJobStatus.Pending,
                duration: null,
                input: values,
                output: null);

            var completedJob = await _sortJobProcessor.Process(pendingJob);

            return Ok(completedJob);
        }

        [Route("")]
        [HttpPost]
        public ActionResult<SortJob> EnqueueJob(int[] values)
        {
            // TODO: Should enqueue a job to be processed in the background.
            var pendingJob = new SortJob(
               id: Guid.NewGuid(),
               status: SortJobStatus.Pending,
               duration: null,
               input: values,
               output: null);
            _backgroungJobClient.Enqueue(() => _sortJobProcessor.Process(pendingJob));
            return Ok(pendingJob);
        }

        [Route("")]
        [HttpGet]
        public ActionResult<List<SortJob>> GetJobs()
        {
            // TODO: Should return all jobs that have been enqueued (both pending and completed).
            return Ok(_sortJobProcessor.GetJobs());
        }

        [Route("{jobId}")]
        [HttpGet]
        public ActionResult<SortJob> GetJob(Guid jobId)
        {
            // TODO: Should return a specific job by ID.
            var jobDetails = _sortJobProcessor.GetJobs().Find(x => x.Id.ToString().Equals(jobId.ToString()));
            if (jobDetails != null) return Ok(jobDetails);
            else return NotFound();
        }
    }
}
