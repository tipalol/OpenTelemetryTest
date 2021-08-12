using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetryTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetSampler(new AlwaysOnSampler())
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("jaeger-test"))
                .AddSqlClientInstrumentation(
                    options => options.SetDbStatementForText = true)
                .AddSource("Sampler")
                .AddJaegerExporter(o =>
                {
                    // Examples for the rest of the options, defaults unless otherwise specified
                    // Omitting Process Tags example as Resource API is recommended for additional tags
                    o.MaxPayloadSizeInBytes = 4096;

                    // Using Batch Exporter (which is default)
                    // The other option is ExportProcessorType.Simple
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>()
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                })
                .AddConsoleExporter()
                .Build();
            
            using (var activity = MyActivitySource.StartActivity("SayHello"))
            {
                activity?.SetTag("foo", 1);
                activity?.SetTag("bar", "Hello, World!");
                activity?.SetTag("baz", new int[] { 1, 2, 3 });
            }
            
            using (Activity activity = MyActivitySource.StartActivity("SomeWork"))
            {
                await Task.Delay(500);
            }
            
            var repository = new UserRepository();
            
            var user = new User()
            {
                Name = "Elon Musk",
                Money = 500000
            };
            
            await repository.Add(user);

            var dbUser = await  repository.Get(1);

            Console.WriteLine(user.Name + " vs " + dbUser.Name);
        }
        
        private static readonly ActivitySource MyActivitySource = new ActivitySource(
            "MyCompany.MyProduct.MyLibrary");
    }
}