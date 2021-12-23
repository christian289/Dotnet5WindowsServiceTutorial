using BuildAutomationWindowsService.Services;
using BuildAutomationWindowsService.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildAutomationWindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "BuildAutomationWindowsService";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<JokeWindowsBackgroundService>();
                    services.AddHttpClient<JokeService>();
                });
    }
}
