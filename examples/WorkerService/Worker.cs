using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private TelemetryClient tc;
        private static HttpClient httpClient = new HttpClient();

        public Worker(ILogger<Worker> logger, TelemetryClient tc)
        {
            _logger = logger;
            this.tc = tc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // By default only Warning of above is captured.
                // However the following Info level will be captured by ApplicationInsights,
                // as appsettings.json configured Information level for the category 'WorkerServiceSampleWithApplicationInsights.Worker'
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // If you want to see how DI is not passing a telemetry client with an instrumentation key, you'll
                // see traces in App Insights showing "n/a" because this is null -- however tracing through ILogger >
                // Serilog > app insights *does* work
                _logger.LogInformation(tc.InstrumentationKey);

                using (tc.StartOperation<RequestTelemetry>("workeroperation"))
                {
                    var res = httpClient.GetAsync("https://bing.com").Result.StatusCode;
                    _logger.LogInformation("INFO: bing http call completed with status:" + res);
                    _logger.LogWarning("WARN: bing http call completed with status:" + res);
                }

                // tc.TrackTrace("test");

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
