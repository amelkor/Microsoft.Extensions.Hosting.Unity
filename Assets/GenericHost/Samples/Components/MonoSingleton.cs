using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Scripting;

namespace GenericHost.Samples
{
    public class MonoSingleton: MonoBehaviour
    {
        private ILogger<MonoSingleton> _logger;
        private IConfiguration _configuration;

        [Preserve]
        private void AwakeServices(ILogger<MonoSingleton> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private void Start()
        {
            _logger.LogInformation("Singleton MonoBehaviour Start()");

            var serverAddress = _configuration.GetValue<string>("Server.BaseUrl");
            
            _logger.LogInformation("Server address is {ServerAddress}", serverAddress);
        }
    }
}