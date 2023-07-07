using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenericHost.Samples
{
    public class MonoHostedService : MonoBehaviour, IHostedService
    {
        private ILogger _logger;

        private void AwakeServices(IServiceProvider services)
        {
            _logger = services.GetRequiredService<ILogger<MonoHostedService>>();
        }
        
        private void Update()
        {
            if (Time.frameCount % 1000 == 0)
            {
                _logger.LogInformation("Update tick from MonoHostedService");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}