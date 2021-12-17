
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Events;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    // Application Insights

                    // Add custom TelemetryInitializer
                    //services.AddSingleton<ITelemetryInitializer, MyCustomTelemetryInitializer>();

                    // Add custom TelemetryProcessor
                    //services.AddApplicationInsightsTelemetryProcessor<MyCustomTelemetryProcessor>();

                    // Example on Configuring TelemetryModules.
                    // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Not a real api key, this is example code.")]
                    //services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((mod, opt) => mod.AuthenticationApiKey = "put_actual_authentication_key_here");

                    // instrumentation key is read automatically from appsettings.json
                    // THIS ONE IS NOT GETTING PASSED THROUGH FOR SOME REASON
                    // ALSO IT'S NOT PULLING FROM appsettings.json, EITHER
                    services.AddApplicationInsightsTelemetryWorkerService("INSTRUMENTATION_KEY");
                })
                // The moment you UseSerilog do this it cancels out all the other built-in logging stuff
                // Nothing in appsettings.json for Logging matters at all anymore once this is added
                .UseSerilog((context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .WriteTo
                        // THIS IS THE ONLY PLACE THAT SETTING THE INSTRUMENTATION KEY IN THIS DEMO WORKED
                        .ApplicationInsights("INSTRUMENTATION_KEY", TelemetryConverter.Traces, LogEventLevel.Verbose)
                        .WriteTo
                        .Console());

        internal class MyCustomTelemetryInitializer : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                // Replace with actual properties.
                (telemetry as ISupportProperties).Properties["MyCustomKey"] = "MyCustomValue";
            }
        }

        internal class MyCustomTelemetryProcessor : ITelemetryProcessor
        {
            ITelemetryProcessor next;

            public MyCustomTelemetryProcessor(ITelemetryProcessor next)
            {
                this.next = next;
            }

            public void Process(ITelemetry item)
            {
                // Example processor - not filtering out anything.
                // This should be replaced with actual logic.
                this.next.Process(item);
            }
        }
    }
}
