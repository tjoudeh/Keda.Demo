
namespace Keda.Demo.TrackingUpdatesProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddControllers().AddDapr();

            var app = builder.Build();

            app.UseAuthorization();

            app.UseCloudEvents();

            app.MapControllers();

            app.MapSubscribeHandler();

            app.Run();
        }
    }
}