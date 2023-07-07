using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GenericHost.Samples
{
    public class MonoHostedService : MonoBehaviour, IHostedService
    {
        private ILogger _logger;
        private SampleUI _ui;

        [Preserve]
        private void AwakeServices(IServiceProvider services)
        {
            _logger = services.GetRequiredService<ILogger<MonoHostedService>>();
            _ui = services.GetRequiredService<SampleUI>();
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
            _ui.InfoText = "MonoHostedService started";
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ui.InfoText = "MonoHostedService stopped";
            
            return Task.CompletedTask;
        }
    }
}