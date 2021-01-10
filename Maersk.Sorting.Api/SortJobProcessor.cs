using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api
{
    public class SortJobProcessor : ISortJobProcessor
    {
        private readonly ILogger<SortJobProcessor> _logger;

        public SortJobProcessor(ILogger<SortJobProcessor> logger)
        {
            _logger = logger;
        }

        public List<SortJob> GetJobs()
        {
            var jobs = new List<SortJob>();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var successedJobs = monitoringApi.SucceededJobs(0, 100);
            var processingJobs = monitoringApi.ProcessingJobs(0, 100);
            jobs.AddRange(successedJobs.Select(x => JsonConvert.DeserializeObject<SortJob>(x.Value.Result.ToString())));
            foreach (var job in processingJobs)
                foreach (var item in job.Value.Job.Args)
                    jobs.Add((SortJob)item);
            return jobs;
        }

        public async Task<SortJob> Process(SortJob job)
        {
            _logger.LogInformation("Processing job with ID '{JobId}'.", job.Id);

            var stopwatch = Stopwatch.StartNew();

            var output = job.Input.OrderBy(n => n).ToArray();
            await Task.Delay(5000); // NOTE: This is just to simulate a more expensive operation

            var duration = stopwatch.Elapsed;

            _logger.LogInformation("Completed processing job with ID '{JobId}'. Duration: '{Duration}'.", job.Id, duration);

            return new SortJob(
                id: job.Id,
                status: SortJobStatus.Completed,
                duration: duration,
                input: job.Input,
                output: output);
        }
    }
}
