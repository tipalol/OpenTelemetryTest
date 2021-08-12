using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetryTest
{
    class Program
    {
        public static readonly ActivitySource MyActivitySource = new ActivitySource(
            "tipalol.OpenTelemetryTest.Dotnet");

        public async static Task Main()
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetSampler(new AlwaysOnSampler())
                .SetErrorStatusOnException()
                .AddSource("tipalol.OpenTelemetryTest.Dotnet")
                .AddSqlClientInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                    o.EnableConnectionLevelAttributes = true;
                    o.RecordException = true;
                })
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

            using (var activity = MyActivitySource.StartActivity("App"))
            {
                var repository = new UserRepository();
            
                var user = new User()
                {
                    Name = "Elon Musk",
                    Money = 500000
                };

                using (var span = MyActivitySource.StartActivity("Add user to DB"))
                {
                    await repository.Add(user);
                }

                using (var span = MyActivitySource.StartActivity("Getting user from DB"))
                {
                    var dbUser = await repository.Get(1);

                    Console.WriteLine(user.Name + " vs " + dbUser.Name);
                }
            }
        }
    }
}