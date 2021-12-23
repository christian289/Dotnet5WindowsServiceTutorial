using BuildAutomationWindowsService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BuildAutomationWindowsService.Workers
{
    public class JokeWindowsBackgroundService : BackgroundService
    {
        private readonly JokeService _jokeService;
        private readonly ILogger<JokeWindowsBackgroundService> _logger;

        public JokeWindowsBackgroundService(
            JokeService jokeService,
            ILogger<JokeWindowsBackgroundService> logger) =>
            (_jokeService, _logger) = (jokeService, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string joke = await _jokeService.GetJokeAsync();
                _logger.LogWarning(joke);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
