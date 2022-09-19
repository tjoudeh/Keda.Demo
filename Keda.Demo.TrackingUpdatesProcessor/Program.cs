using Keda.Demo.Contracts;
using Keda.Demo.TrackingUpdatesProcessor.BackgroundServices;

namespace Keda.Demo.TrackingUpdatesProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHostedService<TrackingUpdateProcessor>();

            builder.Services.AddApplicationInsightsTelemetry();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}